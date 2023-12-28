using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using thu3.Model;
using Thu6.model;

namespace thu3.Controllers
{
    [Route("it4788/api/[controller]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public BlockController(IConfiguration config, IConfiguration redis)
        {
            _configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        [HttpPost]
        [Route("get_list_blocks")]
        public IActionResult get_list_blocks(string token , int? index, int? count)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new
                    {
                        Code = "1004",
                        Message = "Parameter value is invalid"
                    });
                }

                var redisDatabase = _configRedis.GetDatabase();
                var cryptoHelper = new CryptoHelper();
                var dataAccess = new DataAccess(_configuration);

                var user = dataAccess.Query<Users>("select * from users where token=@token", new { token }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "Token is invalid"
                    });
                }

                if (user.banned == "1")
                {
                    return BadRequest(new
                    {
                        Code = "9995",
                        message = "User is not validated(user was banned)"
                    });
                }
                

                var list_blocks = dataAccess.QueryList<Users>("select * from users where id in (select id_user_was_banned from list_ban where id_user=@id_user) ", new { id_user = user.id }).ToList();
                
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> properties = new Dictionary<string, object>();
                foreach (var item in list_blocks)
                {
                    properties.Add("id", item.id);
                    properties.Add("name", item.usename);
                    properties.Add("avatar", item.link_avata);
                    list.Add(properties);
                    
                }


                return Ok(new
                {
                    code = "1000",
                    message = "ok",
                    data = list
                });
            }catch(Exception e)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    Message = e.Message,
                    e.StackTrace,
                    e.Data
                });
            }
        }
        [HttpPost]
        [Route("set_block")]
        public IActionResult set_block(string token , string id_user,int type)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new
                    {
                        Code = "1004",
                        Message = "Parameter value is invalid"
                    });
                }

                var redisDatabase = _configRedis.GetDatabase();
                var cryptoHelper = new CryptoHelper();
                var dataAccess = new DataAccess(_configuration);

                var user = dataAccess.Query<Users>("select * from users where token=@token", new { token }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "Token is invalid"
                    });
                }

                if (user.banned == "1")
                {
                    return BadRequest(new
                    {
                        Code = "9995",
                        message = "User is not validated(user was banned)"
                    });
                }

                if(type!=0 && type!=1)
                {
                    return BadRequest(new
                    {
                        Code = "1004",
                        Message = "Parameter value is invalid"
                    });
                }

                var user_block = dataAccess.Query<banned>("select * from list_ban where id_user=@id_user and id_user_was_banned=@id_user_was_banned", new { id_user = user.id, id_user_was_banned = id_user }).FirstOrDefault();
                if(user_block!=null && type==1)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "người dùng đã bị block từ trước"
                    });
                }
                if(user_block==null && type==0)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "người dùng chưa bị block"
                    });
                }
                if (type== 0)
                {
                    dataAccess.Execute("delete from list_ban where id_user=@id_user and id_user_was_banned=@id_user_was_banned", new { id_user = user.id, id_user_was_banned = id_user });
                }else if (type == 1)
                {
                    dataAccess.Execute("insert into list_ban(id_user,id_user_was_banned) values(@id_user,@id_user_was_banned)", new { id_user = user.id, id_user_was_banned = id_user });
                }
                return Ok(new
                {
                    code = "1000",
                    message = "ok",
                    data = ""
                });
            }catch (Exception e)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    Message = e.Message,
                    e.StackTrace,
                    e.Data
                });
            }
        }
    }
}

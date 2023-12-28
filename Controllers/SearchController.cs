using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using thu3.Model;
using thu3.sp;
using Thu6.model;

namespace thu3.Controllers
{
    [Route("it4788/api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public SearchController(IConfiguration config, IConfiguration redis)
        {
            _configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }



        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(string? token, string keyword,string? index, string? count)
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

                dataAccess.Execute("insert into Searchs(id,keyword,created,id_user) values(@id,@keyword,@created,@id_user)", new { id = cryptoHelper.GenerateRandomId(), keyword = keyword, created = DateTime.Now, id_user = user.id });



                var a=dataAccess.QueryList<Searchs>("select * from Searchs where id_user=@id_user", new { id_user = user.id });
                return Ok(new {
                    list_search=a.ToArray()
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("get_saved_search")]
        public IActionResult actionResult(string? token,  int index, int count)
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
                
                var history_search=dataAccess.QueryList<Searchs>("select * from Searchs where id_user=@id_user", new { id_user= user.id });
                List<Searchs> results = new List<Searchs>();
               
                history_search.ForEach(item =>
                {
                    results.Add(new Searchs
                    {
                        id = item.id,
                        keyword = item.keyword,
                        created = item.created,
                        id_user = item.id_user
                    });
                });
                


                return Ok(new
                {
                    code="1000",
                    messae="Ok",
                    data=results.ToArray()
                });
            }


            catch (Exception e)
            {
                return BadRequest(new
                {
                    code="9999",
                    message=e.Message,
                    e.InnerException,
                    
                });
            }
        }

        [HttpPost]
        [Route("del_saved_search")]
        public IActionResult actionResult(string? token, string id,string? all)
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


                if(all=="1")
                {
                    dataAccess.Execute("delete from Searchs", new {});
                }
                else if (all == "0")
                {
                    dataAccess.Execute("delete from Searchs where id=@id", new { id = id });

                }
                else
                {
                    return BadRequest(new
                    {
                        code = "1004",
                        message = "Parameter value is invalid"
                    });
                }


                return Ok(new
                {
                    codo = "1000",
                    message = "Ok"
                });
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    code="9999",
                    message=e.Message,
                });
            }
        }
    }
}

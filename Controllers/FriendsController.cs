using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using thu3.Model;
using Thu6.model;

namespace thu3.Controllers
{
    [Route("it4788/api/[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public FriendsController(IConfiguration config, IConfiguration redis)
        {
            _configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }


        [HttpPost]
        [Route("get_requested_friends")]
        public IActionResult get_requested_friend(string token, int? index, int? count)
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


                List<RFriendReturn> list = new List<RFriendReturn>();
                var requested = dataAccess.QueryList<Requested_friends>("select * from List_friend where id_friend=@id_user and accept='0'", new { id_user = user.id });
                foreach (var item in requested)
                {
                    var a = dataAccess.Query<Users>("select * from users where id=@id ", new { id = item.id_user }).FirstOrDefault();

                    int s = dataAccess.Query<int>(@"SELECT COUNT(*) AS SoBanChung
                                                FROM List_friend AS f1
                                                JOIN List_friend AS f2 ON f1.id_user = f2.id_user
                                                WHERE f1.id_friend = 'id_friend'
                                                  AND f1.accept = 1 -- Đã đồng ý
                                                  AND f2.accept = 1 -- Đã đồng ý
                                                  AND f1.id_user <> f2.id_user;
                                                ", new { id_friend = item.id_user }).FirstOrDefault();
                    RFriendReturn rFriendReturn = new RFriendReturn();
                    rFriendReturn.id = a.id;
                    rFriendReturn.username = a.usename;
                    rFriendReturn.avata = a.link_avata;
                    rFriendReturn.same_friend = s.ToString();
                    rFriendReturn.created = item.modified;
                    list.Add(rFriendReturn);
                }

                return Ok(new
                {
                    code = "1000",
                    message = "OK",
                    data = new
                    {
                        request = list,
                        total = list.Count
                    }
                });
            }
            catch (Exception e)
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
        [Route("get_user_friends")]
        public IActionResult get_user_friends(string token, int index, int count)
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

                List<RFriendReturn> list = new List<RFriendReturn>();
                var requested = dataAccess.QueryList<Requested_friends>("select * from List_friend where id_friend=@id_user and accept='1'", new { id_user = user.id });
                foreach (var item in requested)
                {
                    var a = dataAccess.Query<Users>("select * from users where id=@id ", new { id = item.id_user }).FirstOrDefault();

                    int s = dataAccess.Query<int>(@"SELECT COUNT(*) AS SoBanChung
                                                FROM List_friend AS f1
                                                JOIN List_friend AS f2 ON f1.id_user = f2.id_user
                                                WHERE f1.id_friend = 'id_friend'
                                                  AND f1.accept = 1 -- Đã đồng ý
                                                  AND f2.accept = 1 -- Đã đồng ý
                                                  AND f1.id_user <> f2.id_user;
                                                ", new { id_friend = item.id_user }).FirstOrDefault();
                    RFriendReturn rFriendReturn = new RFriendReturn();
                    rFriendReturn.id = a.id;
                    rFriendReturn.username = a.usename;
                    rFriendReturn.avata = a.link_avata;
                    rFriendReturn.same_friend = s.ToString();
                    rFriendReturn.created = item.modified;
                    list.Add(rFriendReturn);
                }






                return Ok(new
                {
                    code = "1000",
                    message = "ok",
                    data = new
                    {
                        request = list,
                        total = list.Count
                    }
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    Message = ex.Message,
                    ex.StackTrace,
                    ex.Data
                });
            }
        }

        [HttpPost]
        [Route("set_accept_friend")]
        public IActionResult set_accept_friend(string token, string user_id, string is_accept)
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
                var check = dataAccess.Query<Requested_friends>("select * from List_friend where id_user=@id_user and id_friend=@id_friend", new { id_user = user_id, id_friend = user.id }).FirstOrDefault();
                if(check==null)
                {
                    return BadRequest(new
                    {
                        Code = "9999",
                        message = "User is not in your friend list"
                    });
                }
                if(check.accept=="1")
                {
                    return BadRequest(new
                    {
                        Code = "9999",
                        message = "User is already in your friend list"
                    });
                }   





                if (is_accept != "1" && is_accept != "0")
                {
                    return BadRequest(new
                    {
                        Code = "1004",
                        Message = "Parameter value is invalid"
                    });
                }


                if (is_accept == "1")
                {
                    dataAccess.Execute("update List_friend set accept='1' , modified=@time where id_user=@id_user and id_friend=@id_friend", new { id_user = user_id, id_friend = user.id ,time=DateTime.Now});
                    dataAccess.Execute("insert into List_friend(id_user,id_friend,accept,created,modified) values(@id_user,@id_friend,@accept,@created,@modified)", new { id_user = user.id, id_friend = user_id, accept = "1", created = DateTime.Now, modified = DateTime.Now });
                }
                else if(is_accept=="0")
                {
                    dataAccess.Execute("delete from List_friend where id_user=@id_user and id_friend=@id_friend", new { id_user = user_id, id_friend = user.id });
                }


                return Ok(new
                {
                    code = "1000",
                    message = "ok",
                    data = ""
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                    ex.StackTrace,
                    ex.Data
                });
            }
        }

        [HttpPost]
        [Route("set_request_friend")]
        public IActionResult set_request_friend( string token, string id_user)
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
                var check = dataAccess.Query<Requested_friends>("select * from List_friend where id_user=@id_user and id_friend=@id_friend", new { id_user = id_user, id_friend = user.id }).FirstOrDefault();
                if (check != null)
                {
                    return BadRequest(new
                    {
                        Code = "9999",
                        message = "You have sent an invitation to this user"
                    });
                }
                if (check.accept == "1")
                {
                    return BadRequest(new
                    {
                        Code = "9999",
                        message = "User is already in your friend list"
                    });
                }




                dataAccess.Execute("insert into List_friend(id_user,id_friend,accept,created,modified) values(@id_user,@id_friend,@accept,@created,@modified)", new { id_user = user.id, id_friend = id_user, accept = "0", created = DateTime.Now, modified = DateTime.Now });

                int count =dataAccess.Query<int>("select count(*) from List_friend where id_user=@id_user and accept='0'", new { id_user = user.id }).FirstOrDefault();
                return Ok(new
                {
                    code = "1000",
                    message = "ok",
                    data = new
                    {
                        request_friend = count
                    }
                });
            }catch(Exception ex)
            {
                return BadRequest(new
                {
                    code="9999",
                    message=ex.Message,
                    ex.StackTrace,
                    ex.Data
                });
            }
        }



    }
}

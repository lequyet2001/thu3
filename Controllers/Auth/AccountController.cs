using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Data.SqlClient;
using System.Reflection;
using thu3.Model;
using Thu6.model;

namespace thu3.Controllers.Auth
{
    [Route("it4788/api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _Configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public AccountController(IConfiguration config, IConfiguration redis)
        {
            _Configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        [HttpGet("getAll")]
        public ActionResult getAllKeyValue()
        {
            var redisDatabase = _configRedis.GetDatabase();
           
            var keys = _configRedis.GetServer("localhost", 6379).Keys();

            var values = new List<string>();
            var k = new List<string>();

            foreach (var key in keys)
            {
                var value = redisDatabase.StringGet(key);
                values.Add(value);
                values.Add("_________________________");
                k.Add(key);
            }
            return Ok(new { values,k });
        }
      

        [HttpPost]
        [Route("Signup")]
        public IActionResult Signup(SignupModel model  )
        {
            try
            {
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper f = new CryptoHelper();
                string id = f.GenerateRandomId();
                var dataAccess = new DataAccess(_Configuration);
                var existingUser = dataAccess.Query<Users>("select * from Users where email=@Email", new {model.Email}).FirstOrDefault();
                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        Status = 9996,
                        Message = "User existed",
                        data =new { }
                    });
                }
                redisDatabase.StringSet("devtoken: " + id, model.Uuid);
                var newUser = new Users
                {
                    id = id,
                    email = model.Email,
                    password = model.Password
                };
                var result = dataAccess.AddUser(newUser);
                if (result > 0)
                {
                    return Ok(new
                    {
                        Status = 1000,

                        Message = "Ok",
                        Data = dataAccess.Query<Users>("SELECT * FROM users WHERE email = @Email", new { model.Email }).FirstOrDefault(),
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Status = 500,
                        Message = "Signup Falled"
                    });
                }
               
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error =ex.Source ,
                    ex.Message,
                    status = 500
                });
            }
           
        }



        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginModel model)
        {
            try
            {
               


                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper f = new CryptoHelper();
               
                var dataAccess = new DataAccess(_Configuration);
                var user = dataAccess.Query<Users>("select * from Users where email=@email and password=@password", new { model.email,model.password }).FirstOrDefault();
                if (user.active.Equals("0"))
                {
                    return BadRequest(new
                    {
                     code=0,
                     message="User is not Acitve"
                    });
                }
                var oldDevToken = redisDatabase.StringGet("devtoken: " + user.id);
                if(user != null )
                {
                 
                  
                    if(oldDevToken != model.devtoken)
                    {
                        redisDatabase.KeyDelete("devtoken: " + user.id);
                        redisDatabase.StringSet("devtoken: " + user.id, model.devtoken);
                        
                    }
                    CryptoHelper cryptoHelper = new CryptoHelper();
                    var id_login = user.id;
                    var token =cryptoHelper.Encrypt(id_login, "tokenlogin123456");
                    dataAccess.Query<Users>(@" update users 
                                                set token=@token
                                                where id=@id", new {token=token,id=user.id});

                    return Ok(new
                    {
                        code=1000,
                        message="Ok",
                        data = new {
                            user.id,user.username,token,avata=user.link_avata,user.active,user.coins
                        }
                    });
                }
                else
                {
                return BadRequest(new
                {
                    code=9995,
                    message="User is not validated"
                });

                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }



        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout([FromHeader]string token)
        {
            try
            {
                var dataAccess = new DataAccess(_Configuration);
                var a=dataAccess.Query<Users>("select * from users where token=@token", new { token }).FirstOrDefault();
                if (a == null)
                {
                   return Ok(new
                   {
                       code= 500,
                       message="user was logout or invalid"
                   });
                }
                string newtoken = "";
                dataAccess.Query<Users>("update users set token=@token where id=@id", new { a.id ,token=newtoken});

                return Ok(new
                {
                    code=1000,
                    message="Logout success"
                });
                
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);

            }



           

        }


        [HttpPost]
        [Route("check")]
        public IActionResult get_verify_code(string email)
        {

            try
            {

            DataAccess dataAccess = new DataAccess(_Configuration);
            CryptoHelper cryptoHelper = new CryptoHelper();
            IDatabase redisDatabase = _configRedis.GetDatabase();
            var user=dataAccess.Query<Users>("select* from users where email=@email",new {email});
            if(user == null)
            {
                return BadRequest(new
                {
                    code = 500,
                    message = "email chua duoc dang ky"
                });
            }
            var a=cryptoHelper.GenerateRandomId();
            var code = cryptoHelper.Encrypt(a, "1234567891234567");
            redisDatabase.StringSet(a,email);
                return Ok(new
                {
                    code = 1000,
                    message = "Ok",
                    data = new
                    {
                        code_verify = a
                    }
                }) ;
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

       

    }
}

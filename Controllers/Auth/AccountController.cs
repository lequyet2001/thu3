using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using thu3.Model;
using Thu6.model;

namespace thu3.Controllers.Auth
{
    [Route("it4788/api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private const int V = 0;
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
        [HttpGet("rm")]
        public ActionResult rm( string key)
        {
            var redisDatabase = _configRedis.GetDatabase();

            var keys = _configRedis.GetServer("localhost", 6379).Keys();
            redisDatabase.KeyDelete(key);
           
            return Ok(getAllKeyValue());
        }


        [HttpPost]
        [Route("Signup")]
        public IActionResult Signup(SignupModel model  )
        {
            try
            {
                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper f = new CryptoHelper();
              
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
                redisDatabase.StringSet("devtoken: " + model.Email, model.Uuid);
                var newUser = new Users
                {
                   
                    email = model.Email,
                    password = model.Password,
                    created = DateTime.Now,
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
                CryptoHelper cryptoHelper = new CryptoHelper();
                var dataAccess = new DataAccess(_Configuration);

                var user = dataAccess.Query<Users>("SELECT * FROM Users WHERE email = @email AND password = @password", new { model.email, model.password }).FirstOrDefault();

                if (user == null)
                {
                    return BadRequest(new
                    {
                        code = 9995,
                        message = "User is not validated"
                    });
                }
                
                if (user.active == 0)
                {
                    return BadRequest(new
                    {
                        code = 0,
                        message = "User is not Active"
                    });
                }

                var oldDevToken = redisDatabase.StringGet("devtoken: " + model.email);
                if (oldDevToken != model.devtoken)
                {
                    redisDatabase.KeyDelete("devtoken: " + user.email);
                    redisDatabase.StringSet("devtoken: " + user.email, model.devtoken);
                }

                var idLogin = user.id;
                var token = cryptoHelper.Encrypt(idLogin.ToString(), "tokenlogin123456");

                dataAccess.Query<Users>(@"UPDATE users 
                                   SET token = @token
                                   WHERE email = @email", new { token, user.email });

                return Ok(new
                {
                    code = 1000,
                    message = "Ok",
                    data = new
                    {
                        user.id,
                        user.usename,
                        token,
                        avata = user.link_avata,
                        user.active,
                        user.coins
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine(ex);
                return BadRequest("An error occurred");
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
                dataAccess.Query<Users>("update users set token=@token where email=@email", new { a.email ,token=newtoken});

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
        [Route("get_varify_code")]
        public IActionResult get_verify_code([EmailAddress][Required]string email)
        {

            try
            {

            DataAccess dataAccess = new DataAccess(_Configuration);
            CryptoHelper cryptoHelper = new CryptoHelper();
            IDatabase redisDatabase = _configRedis.GetDatabase();

            var user=dataAccess.Query<Users>("select* from users where email=@email",new {email}).FirstOrDefault();
            if(user == null)
            {
                return BadRequest(new
                {
                    code = 500,
                    message = "email chua duoc dang ky"
                });
            }
                else
                {

                if (user.active==1){
                
                        return Ok(new
                        {
                            code=199999,
                            message="user is actived"
                        });
                   }
            
                 var keyId=cryptoHelper.GenerateRandomId();
                var token = cryptoHelper.Encrypt(keyId, "1234567891234567");
                redisDatabase.StringSet(email + ": " + keyId,token);

                return Ok(new
                    {
                        code = 1000,
                        message = "Ok",
                        data = new
                        {
                        code_verify = token
                        }
                    }) ;
                }
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }


        [HttpPost]
        [Route("check_verify_code")]
        public IActionResult check_verify_code([EmailAddress]string email,[FromHeader]string token )
        {
            try
            {
                DataAccess dataAccess = new DataAccess(_Configuration);
                CryptoHelper cryptoHelper = new CryptoHelper();
                IDatabase redisDatabase = _configRedis.GetDatabase();
                var keyId = cryptoHelper.Decrypt(token, "1234567891234567");
                var b = redisDatabase.StringGet(email + ": " + keyId);
                var user=dataAccess.Query<Users>("select * from users where email=@email",new{email }).FirstOrDefault();
                if(user == null)
                {
                    return Ok(new
                    {
                        code=9995,
                        message="User is not validated"
                    });
                }
                 if (user.active.Equals("1") )
                {
                    return Ok(new
                    {
                        code = 9996,
                        message = "User existed"
                    });
                }
                 if (!b.Equals(token))
                {
                    return Ok(new
                    {
                        code=1004,
                        message="Parameter value is invalid"
                    });
                }
                 if(user.active.Equals("0")&& token.Equals("")) {

                    return Ok(new
                    {
                        code=1002,
                        message="Parameter is not enought"
                    });
                }
                var id=cryptoHelper.GenerateRandomId();
                dataAccess.Query<Users>("update users set id=@id, active=1 where email=@email", new { id ,email});
                redisDatabase.KeyDelete(email + ": " + keyId);

                return Ok(new
                {

                    code=1000,
                    message="Ok",
                    data = new
                    {
                        id,active="1"
                    }
                });
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("Change_infor_after_sigup")]
        public async Task<IActionResult> Change_infor_after_sigup([FromHeader] string token, [Required] string username, IFormFile? avatar)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new
                    {
                        code = 1004,
                        message = "Parameter value is invalid"
                    });
                }

                IDatabase redisDatabase = _configRedis.GetDatabase();
                CryptoHelper cryptoHelper = new CryptoHelper();
                var dataAccess = new DataAccess(_Configuration);

                Users user = dataAccess.Query<Users>("select * from users where token=@token", new { token }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        code = 500,
                        message = "Token is invalid"
                    });
                }

                if (user.email == username)
                {
                    return BadRequest(new
                    {
                        code = 1004,
                        message = "Cannot match the user's email"
                    });
                }

                if (Regex.IsMatch(username, "[^a-zA-Z0-9]"))
                {
                    return BadRequest(new
                    {
                        code = 1004,
                        message = "Username cannot contain special characters, links, emails, or addresses"
                    });
                }

                if (!(username.Length >= 3 && username.Length <= 20))
                {
                    return BadRequest(new
                    {
                        code = 1004,
                        message = "Username must be between 3 and 20 characters long"
                    });
                }

                // Handle the avatar file
                byte[] avatarData = null;

                if (avatar != null && avatar.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await avatar.CopyToAsync(memoryStream);
                        avatarData = memoryStream.ToArray();
                    }
                }

                // Update user information, including the avatar path
                byte[] linkAvata = avatarData == null ? user.link_avata : avatarData;

                dataAccess.Query<Users>("update users set usename=@username, link_avata=@linkAvata where id=@id", new { username, linkAvata, user.id });

                // Return the updated user data
                var updatedUser = dataAccess.Query<Users>("select * from users where id=@id", new { user.id }).FirstOrDefault();

                return Ok(new
                {
                    code = 1000,
                    message = "OK",
                    data = new
                    {
                        updatedUser.id,
                        updatedUser.usename,
                        updatedUser.email,
                        updatedUser.created,
                        updatedUser.link_avata
                    },
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Thu6.model;
using thu3.Model;

namespace thu3.Controllers
{
    [Route("it4788/api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _configRedis;
        public UsersController(IConfiguration config, IConfiguration redis)
        {
            _configuration = config;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        [HttpPost]
        [Route("change_password")]
        public IActionResult change_password(string token ,string oldPassword, 
            [Required(ErrorMessage = "Password is required")]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).*$",
                 ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
            [MaxLength(30, ErrorMessage = "Password must be at most 30   characters long")]
            [PasswordPropertyText]string newPaswword)
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
                if(user.password!= oldPassword)
                {
                    return BadRequest(new
                    {
                        Code = "9996",
                        message = "Old password is not correct"
                    });
                }
                if(newPaswword==oldPassword)
                {
                    return BadRequest(new
                    {
                        Code = "9997",
                        message = "New password must be different from old password"
                    });
                }
                if(cryptoHelper.CalculateSimilarity(oldPassword,newPaswword) <= 80)
                {
                    return BadRequest(new
                    {
                        Code = "9998",
                        message = "New password must be different from old password at least 80%"
                    });
                }

                dataAccess.Execute("update users set password=@newPaswword where token=@token", new { newPaswword, token });

    
                return Ok(new
                {
                    code="1000",
                    message="Ok",
                    data="null"
                });
            }catch(Exception e)
            {
                return BadRequest(new
                {
                    Code = "500",
                    Message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("get_user_info")]
        public IActionResult get_user_info(string token,string? id_user)
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
                var user_info = dataAccess.Query<Users>("select * from users where id=@id_user", new { id_user }).FirstOrDefault();
                if(user_info==null)
                {
                    return BadRequest(new
                    {
                        Code = "9999",
                        message = "User is not exist"
                    });
                }
                if(user_info.banned=="1")
                {
                    return BadRequest(new
                    {
                        Code = "9995",
                        message = "User is not validated(user was banned)"
                    });
                }
              
                var is_friend= dataAccess.Query<Requested_friends>("select * from List_friend where id_user=@id_user and id_friend=@id_friend and accept='1'", new { id_user, id_friend = user.id }).FirstOrDefault();
                bool is_friend_bool = true;
                if (is_friend == null)
                {
                    is_friend_bool = false;
                }

                var token_user = dataAccess.Query<string>("select token from users where id=@id_user", new { id_user  }).FirstOrDefault();
                bool online = true;
                if (token_user == null)
                {
                    online = false;
                }
                int listing = dataAccess.Query<int>("select count(*) from List_friend where id_user=@id_user and accept='1'", new { id_user }).FirstOrDefault();
                return Ok(new
                {
                    code="1000",
                    message="Ok",
                    data=new
                    {
                        id=id_user,
                        username=user_info.usename,
                        created=user_info.created,
                        description=user_info.description,
                        avatar=user_info.link_avata!=null?user_info.link_avata:"null",
                        cover_image="",
                        link="",
                        address=user_info.address,
                        city=user_info.city,
                        country=user_info.country,
                        listing=listing.ToString(),
                        is_friend=is_friend_bool.ToString(),
                        online=online.ToString(),
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    Message = e.Message,
                    e.InnerException,
                    e.Data
                });
            }
        }
        [HttpPost]
        [Route("set_user_info")]
        public async Task<IActionResult> set_user_info( string token, string? username, string? description, IFormFile? avatar, string? address, string? city, string? country, string? link,IFormFile? cover_image)
        {
            try
            {
                string set_username= username!=null?username:"";
                string set_description = description != null ? description : "";
                string set_address = address != null ? address : "";
                string set_city = city != null ? city : "";
                string set_country = country != null ? country : "";
                string set_link = link != null ? link : "";

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










                byte[] avatarData = null;

                if (avatar != null && avatar.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await avatar.CopyToAsync(memoryStream);
                        avatarData = memoryStream.ToArray();
                    }
                }
                string base64String = avatarData != null ? Convert.ToBase64String(avatarData) : null;

                string linkAvata = base64String ?? user.link_avata;
                byte[] avatarData2 = null;

                if (cover_image != null && cover_image.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await cover_image.CopyToAsync(memoryStream);
                        avatarData2 = memoryStream.ToArray();
                    }
                }
                string base64String2 = avatarData2 != null ? Convert.ToBase64String(avatarData) : null;

                string linkAvata2 = base64String2 ?? user.cover_image;
                dataAccess.Execute("update users set usename=@set_username,description=@set_description,link_avata=@base64String,address=@set_address,city=@set_city,country=@set_country,link=@set_link, cover_image=@cover_image where id=@id_user",
                    new { set_username, set_description, base64String=linkAvata, set_address, set_city, set_country, set_link, id_user=user.id,cover_image=linkAvata2 });

                // Update user information, including the avatar path
                return Ok(new
                {
                    code = "1000",
                    message = "Ok",
                    data = new
                    {
                        avatar = linkAvata,
                        cover_image = linkAvata2,
                        link = set_link,
                        city = set_city,
                        country = set_country,

                    }
                });
            }catch(Exception e)
            {
                return  BadRequest(new
                {
                    Code = "9999",
                    Message = e.Message,
                    e.InnerException,
                    e.Data
                });
            }
        }
    }
}

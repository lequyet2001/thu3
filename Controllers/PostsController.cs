using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using thu3.Model;
using Thu6.model;
using static System.Net.Mime.MediaTypeNames;

namespace thu3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private const long MaxFileSize = 30 * 1024 * 1024;
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _configRedis;

        public PostsController(IConfiguration configuration, IConfiguration redis)
        {
            _configuration = configuration;
            string redisConnectionString = redis.GetConnectionString("Redis");
            _configRedis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        [HttpPost]
        [Route("add_post")]
        public async Task<IActionResult> AddPost(string token, IFormFile? video, List<IFormFile>? images, string? described, string? status)
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

                if (images != null && video != null && images.Any() && video.Length > 0)
                {
                    return BadRequest(new
                    {
                        Code = "99999",
                        Message = "Video and images cannot coexist"
                    });
                }

                if (images != null && images.Sum(image => image.Length) > MaxFileSize * 20)
                {
                    return BadRequest(new
                    {
                        Code = "1006",
                        Message = $"List Image file size exceeds the maximum allowed size."
                    });
                }

                if (video != null && video.Length > MaxFileSize * 2)
                {
                    return BadRequest(new
                    {
                        Code = "1006",
                        Message = $"Image file Video size or video duration is too big."
                    });
                }

                var postId = cryptoHelper.GenerateRandomId();

                dataAccess.Query<Posts>(
                    @"INSERT INTO [dbo].[Posts] ([id_user], [id], [id_modified],[described], [status], [created], [modified], [name], [fake], [trust], [is_marked], [state],[banned])
                      VALUES (@user, @id_NewPost,@id_modified, @Described, @status, @time,@modified, 'Tên Bài Viết', 0, 0, 0, 'Published',0)",
                    new
                    {
                        id_modified= postId,
                        modified=DateTime.Now,
                        user = user.id,
                        time = DateTime.Now,
                        id_NewPost = postId,
                        described,
                        status
                    });

                if (images != null && images.Any())
                {
                        int a = 0;
                    foreach (var image in images)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await image.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());
                            var id = cryptoHelper.GenerateRandomId();

                            dataAccess.Query<Images>(
                                @"INSERT INTO [dbo].[Image] ([id],[image],[id_post],[index]) VALUES (@id,@linkAvata,@id_newPost,@index)",
                                new { id, linkAvata = base64String, id_NewPost = postId ,index=a});
                        a++;
                        }
                    }
                }

                if (video != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await video.CopyToAsync(stream);
                        var base64Video = Convert.ToBase64String(stream.ToArray());

                        var id_video = cryptoHelper.GenerateRandomId();
                        dataAccess.Query<Video>(
                            @"INSERT INTO [dbo].[Video] ([id],[video],[id_post],[thumbnail]) VALUES (@id_video,@linkAvata,@id_NewPost,'123')",
                            new { id_video, linkAvata = base64Video, id_NewPost = postId });
                    }
                }
                if (user.coins == 0)
                {
                    return BadRequest(new
                    {
                        code = "501",
                        message = "you don't have enough coins"
                    });
                }

                var coins = user.coins - 1;
                dataAccess.Query<Users>(
                    @"update Users set coins=@coins where id=@id",
                    new { coins, user.id });

                return Ok(new
                {
                    Code = "1000",
                    Message = "Ok",
                    Data = new
                    {
                        Id = $"{postId}",
                        Url = "",
                        Coins = $"{user.coins - 1}"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    ex.Message,
                    Data = ex.StackTrace
                });
            }
        }



        [HttpPost]
        [Route("get_post")]
        public   IActionResult GetPost([FromForm]GetPostModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.token))
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

                var user = dataAccess.Query<Users>("select * from users where token=@token", new { model.token }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "Token is invalid"
                    });
                }

                var post= dataAccess.Query<Posts>("select * from Posts where id=@id order by modified DESC", new {model.id}).FirstOrDefault();
                if (post == null)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"

                    });
                }
                var author_post = dataAccess.Query<Users>("select * from users where id=@id_user", new { post.id_user }).FirstOrDefault();
                var list_ban = dataAccess.QueryList<banned>("select * from List_ban where id_user=@id_user", new { id_user = post.id_user });

                bool isUserBanned = list_ban.Any(bannerUser => bannerUser.id_user_was_banned == user.id);
                if (isUserBanned)
                {
                    post.is_blocked = 1;
                    return BadRequest(new
                    {
                        Code = "1000",
                        message = "Ok",
                        Data = new
                        {
                            is_bocked = "1"
                        }
                    });
                }

               
                
                if ( post != null &&post.banned == 1 )
                {
                    return BadRequest(new
                    {
                        Code="9992",
                        message="Post is not exited"
                    });
                }
                
                

                  


                var videos = dataAccess.Query<Video>("select * from Video where id_post=@id_post ", new { id_post = post.id_modified}).FirstOrDefault();
                
                post.video = videos!=null ? (Video)videos : null;

                var image=dataAccess.QueryList<Images>("select* from Image where id_post=@id_post",new { id_post = post.id_modified });
                
                post.image = image.Any() ? image: null ;

                Author author=new Author();
                author.id=post.id_user;
                author.name = author_post.usename;
                author.avatar = author_post.link_avata;

                var disappointed= dataAccess.Query<int>("select count(*) from Feel where id_post=@id_post and type =1 ", new {id_post=post.id}).FirstOrDefault();
                var kudos = dataAccess.Query<int>("select count(*) from Feel where id_post=@id_post and type = 0 ", new { id_post = post.id }).FirstOrDefault();
                post.kudos = kudos;
                post.disappointed= disappointed;
                var listing = dataAccess.QueryList<string>("select id_modified from Posts where id=@id", new { model.id });
                /*                var category= dataAccess.Query<Category>("select * from ")
                */
                author.listing = listing;

                post.author = author;

                


                return Ok(new
                {
                    Code = "1000",
                    Message = "OK",
                    Data = post.ConvertToDictionary(),
                }) ; 
            }
            catch(Exception ex) 
            {
                return BadRequest(new
                {
                    Code="9999",
                    ex.Message,
                    ex.StackTrace,
                    ex.Data
                });
            }
        }


        [HttpPost]
        [Route("edit_post")]
        public async Task<IActionResult> Edit_Post([FromForm]EditPostModel model)
        {
            try
            {

                if (model.images != null && model.images.Sum(image => image.Length) > MaxFileSize * 20)
                {
                    return BadRequest(new
                    {
                        Code = "1006",
                        Message = $"List Image file size exceeds the maximum allowed size."
                    });
                }

                if (model.video != null && model.video.Length > MaxFileSize * 2)
                {
                    return BadRequest(new
                    {
                        Code = "1006",
                        Message = $"Image file Video size or video duration is too big."
                    });
                }

                if (string.IsNullOrEmpty(model.token))
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
                
                var user = dataAccess.Query<Users>("select * from users where token=@token", new { model.token }).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Code = "500",
                        Message = "Token is invalid"
                    });
                }
            
              
                if (model.status == "1011")
                {
                    return BadRequest(new
                    {
                        Code="1011",
                        message= "Could not publish this post "
                    });
                }
                if (model.status == "1012")
                {
                    return BadRequest(new
                    {
                        code="1012",
                        message="OK",
                        warning= "Content not suitable for global posting"
                    });
                }
               
                if (user.banned=="1")
                {
                    return BadRequest(new
                    {
                        Code="9995",
                        message="User is not validated(user was banned)"
                    });
                }

                var post = dataAccess.Query<Posts>("select * from Posts where id=@id order by modified DESC", new { model.id }).FirstOrDefault();
               
                if (post == null)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"

                    });
                }

                if (post.banned == 1)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"
                    });
                }
                if (user.id != post.id_user)
                {
                    return BadRequest(new
                    {
                        Code="501",
                        message ="You are not the owner of the post"
                    });
                }
                if (user.coins == 0)
                {
                    return BadRequest(new
                    {
                        code = "501",
                        message = "you don't have enough coins"
                    });
                }



                var id_modified = cryptoHelper.GenerateRandomId();
                post.id_modified = id_modified;
                post.described = model.described;
                post.status = model.status;
                post.modified=DateTime.Now;
                
                var old_video = dataAccess.Query<Video>("select * from Video where id_post=@id", new { id = post.id_modified }).FirstOrDefault();
                var old_Image =dataAccess.QueryList<Images>("select * from Image where id_post=@id", new { id= post.id_modified});
                
                dataAccess.Query<Posts>(@"INSERT INTO [dbo].[Posts] ([id_user], [id], [id_modified],[described], [status], [created], [modified], [name], [fake], [trust],  [is_marked], [state],[banned])
                                                        VALUES (@id_user, @id,@id_modified, @Described, @status, @created,@modified, @name, @fake, @trust,  @is_marked, @state,@banned)",new
                {   
                    post.id_user,
                    post.id,
                    post.id_modified,
                    post.described,
                    post.status,
                    post.created,
                    post.modified,
                    post.name,
                    post.fake,
                    post.trust,
                 
                    post.is_marked,
                    post.state,
                    post.banned


                });


                if (model.images != null && model.images.Any())
                {
                    int a = 0;
                    foreach (var image in model.images)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await image.CopyToAsync(memoryStream);
                            var base64String = Convert.ToBase64String(memoryStream.ToArray());
                            var id = cryptoHelper.GenerateRandomId();

                            dataAccess.Query<Images>(
                                @"INSERT INTO [dbo].[Image] ([id],[image],[id_post],[index]) VALUES (@id, @linkAvata, @id_newPost, @index)",
                                new { id, linkAvata = base64String, id_NewPost = id_modified, index = a });
                            a++;
                        }
                    }
                }


                if (model.video != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await model.video.CopyToAsync(stream);
                        var base64Video = Convert.ToBase64String(stream.ToArray());

                        var id_video = cryptoHelper.GenerateRandomId();
                        dataAccess.Query<Video>(
                            @"INSERT INTO [dbo].[Video] ([id],[video],[id_post],[thumbnail]) VALUES (@id_video,@linkAvata,@id_NewPost,'123')",
                            new { id_video, linkAvata = base64Video, id_NewPost = id_modified });
                    }
                }

                var coins = user.coins - 1;
                dataAccess.Query<Users>(
                    @"update Users set coins=@coins where id=@id",
                    new { coins, user.id });




                return Ok(new
                {
                    Code = "1000",
                    message = "Ok",
                    coins = $"{user.coins-1}",
                    
                });

            }catch( Exception ex)
            {
                return BadRequest(new
                {
                    Code="9999",
                    ex.Message,
                    Data = ex.StackTrace
                });
            }
        }



        [HttpPost]
        [Route("delete_post")]
        public IActionResult delete_post(string token , string id)
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

                var post = dataAccess.Query<Posts>("select * from Posts where id=@id", new { id }).FirstOrDefault();
                
                if (post == null)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"

                    });
                };

                if (user.id != post.id_user)
                {
                    return BadRequest(new
                    {
                        Code = "501",
                        message = "You are not the owner of the post"
                    });
                }
                if (user.coins == 0)
                {
                    return BadRequest(new
                    {
                        code = "501",
                        message = "you don't have enough coins"
                    });
                }

                var id_modified = dataAccess.QueryList<Posts>("select id_modified from Posts where id=@id", new { id });
                id_modified.ForEach(id => {
                    dataAccess.Execute("delete [dbo].[Image] where id_post=@id_modified", new { id.id_modified });
                    dataAccess.Execute("delete [dbo].[Video] where id_post=@id_modified", new { id.id_modified });
                    dataAccess.Execute("delete [dbo].[Posts] where id=@id", new { post.id });
                });
              
                var coins = user.coins - 1;
                dataAccess.Query<Users>(
                    @"update Users set coins=@coins where id=@id",
                    new { coins, user.id });

                return Ok(new
                {
                    code = "1000",
                    message = "Ok",
                    coins =$"{user.coins-1}"
                });

            }
            catch ( Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                  ex.StackTrace,
                    data = ex.Data
                }); ;
            }
        }


        [HttpPost]
        [Route("report_post")]
        public IActionResult Report_post(string token , string id, string subject, string details)
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



                var post = dataAccess.Query<Posts>("select * from Posts where id=@id", new { id }).FirstOrDefault();
                var list_ban = dataAccess.QueryList<banned>("select * from List_ban where id_user=@id_user", new { id_user = post.id_user });

                bool isUserBanned = list_ban.Any(bannerUser => bannerUser.id_user_was_banned == user.id);
                if (isUserBanned)
                {
                    post.is_blocked = 1;
                    return BadRequest(new
                    {
                        Code = "1002",
                        message = "You have been banned by the post owner",
                        Data = new
                        {
                            is_bocked = "1"
                        }
                    });
                }

                if (post == null)
                {
                    return BadRequest(new
                    {
                        code="9992",
                        message="Posts is not exited"
                    });
                }
                if (post.banned == 1)
                {
                    return BadRequest(new
                    {
                        code="1011",
                        message= "Could not publish this post"
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
                if (user.id == post.id_user)
                {
                    return BadRequest(new
                    {
                        Code = "501",
                        message = "You cannot report yourself"
                    });
                }


                var id_report = cryptoHelper.GenerateRandomId();
                dataAccess.Execute("insert into [dbo].[Report] ([id],[id_user_report],[id_post],[subject],[details]) values (@id,@id_user_report,@id_post,@subject,@details)", new { id = id_report, id_user_report = user.id, subject, details,id_post=post.id });


                
                return Ok(new
                {
                    Code="1000",
                    message="Ok",

                });
            }catch(Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                    ex.StackTrace,
                    data = ex.Data
                });
            }
        }

        [HttpPost]
        [Route("feel")]
        public IActionResult Feel(string token ,string id, string type)
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



                var post = dataAccess.Query<Posts>("select * from Posts where id=@id", new { id }).FirstOrDefault();
                var list_ban = dataAccess.QueryList<banned>("select * from List_ban where id_user=@id_user", new { id_user = post.id_user });

                bool isUserBanned = list_ban.Any(bannerUser => bannerUser.id_user_was_banned == user.id);
                if (isUserBanned)
                {
                    post.is_blocked = 1;
                    return BadRequest(new
                    {
                        Code = "1000",
                        message = "Ok",
                        Data = new
                        {
                            is_bocked = "1"
                        }
                    });
                }
                if (post == null)
                {
                    return BadRequest(new
                    {
                        code = "9992",
                        message = "Posts is not exited"
                    });
                }
                if (post.banned == 1)
                {
                    return BadRequest(new
                    {
                        code = "1011",
                        message = "Could not publish this post"
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

                var feels = dataAccess.Query<Feel>("select * from Feel where id_post=@id_post and id_user=@id_user", new { id_post = post.id, id_user = user.id }).FirstOrDefault();
                if (feels == null)
                {
                    dataAccess.Execute("insert into Feel ([id_user],[id_post],[type]) values(@id_user,@id_post,@type) ", new {id_post=post.id, id_user=user.id,type});
                }
                else
                {
                        dataAccess.Execute("update Feel set type=@type where id_user=@id_user and id_post=@id_post", new {id_post=post.id,id_user=user.id,type});
                    
                }
                

              

                return Ok(new
                {
                    code="1000",
                    message="Ok",
                    data = new
                    {
                        
                    }
                });
            }catch (Exception ex)
            {
                return BadRequest(new
                {
                    code="9999",
                    message=ex.Message,
                    ex.Data,
                    ex.StackTrace,
                });
            }
        }

    }
}

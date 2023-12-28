using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using thu3.Model;
using thu3.Model.PostModel;
using Thu6.model;
using static System.Net.Mime.MediaTypeNames;

namespace thu3.Controllers
{
    [Route("it4788/api/[controller]")]
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
                    @"INSERT INTO [dbo].[Posts] ([id_user], [id], [id_modified],[described], [status], [created], [modified], [name],  [is_marked], [state],[banned])
                      VALUES (@user, @id_NewPost,@id_modified, @Described, @status, @time,@modified, 'Tên Bài Viết',  0, 'Published',0)",
                    new
                    {
                        id_modified = postId,
                        modified = DateTime.Now,
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
                                new { id, linkAvata = base64String, id_NewPost = postId, index = a });
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
        public IActionResult GetPost([FromForm] GetPostModel model)
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

                var post = dataAccess.Query<Posts>("select * from Posts where id=@id order by modified DESC", new { model.id }).FirstOrDefault();
                if (post == null)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"

                    });
                }
                var author_post = dataAccess.Query<Users>("select * from users where id=@id_user", new { post.id_user }).FirstOrDefault();
                var list_ban = dataAccess.QueryList<string>("select id_user_was_banned from List_ban where id_user=@id_user", new { post?.id_user });
             
                bool isUserBanned = list_ban != null && list_ban.Any(bannerUser => bannerUser == user.id);
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



                if (post != null && post.banned == 1)
                {
                    return BadRequest(new
                    {
                        Code = "9992",
                        message = "Post is not exited"
                    });
                }






                var videos = dataAccess.Query<Video>("select * from Video where id_post=@id_post ", new { id_post = post.id_modified }).FirstOrDefault();

                post.video = videos != null ? videos : null;

                var image = dataAccess.QueryList<Images>("select* from Image where id_post=@id_post", new { id_post = post.id_modified });

                post.image = image.Any() ? image : null;

                Author author = new Author();
                author.id = post.id_user;
                author.name = author_post.usename;
                author.avatar = author_post.link_avata;

                var disappointed = dataAccess.Query<int>("select count(*) from Feel where id_post=@id_post and type =1 ", new { id_post = post.id }).FirstOrDefault();
                var kudos = dataAccess.Query<int>("select count(*) from Feel where id_post=@id_post and type = 0 ", new { id_post = post.id }).FirstOrDefault();
                var count_mark_fake = dataAccess.Query<int>(@"select count(*) from mark_comment mc 
                                                            join mark m on mc.id_mark=m.id where mc.id_post=@id_post and m.type_mark='fake'", new { id_post = post.id }).FirstOrDefault();
                var count_mark_trust = dataAccess.Query<int>(@"select count(*) from mark_comment mc 
                                                            join mark m on mc.id_mark=m.id where mc.id_post=@id_post and m.type_mark='trust'", new { id_post = post.id }).FirstOrDefault();
                var count_comment =dataAccess.Query<int>(@"select count(*) from mark_comment mc 
                                                            join comment m on mc.id_comment=m.id where mc.id_post=@id_post", new { id_post = post.id }).FirstOrDefault();
                //There are no obvious bugs in the code snippet provided. However, it is important to ensure that the `dataAccess` object is properly initialized and that the SQL queries are correct and return the expected results. Additionally, it is recommended to handle any potential exceptions that may occur during the execution of the queries.
                post.kudos = kudos;
                post.disappointed = disappointed;
                post.fake = count_mark_fake;
                post.trust = count_mark_trust;
                post.comments=count_comment;
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
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Code = "9999",
                    ex.Message,
                    ex.StackTrace,
                    ex.Data
                });
            }
        }


        [HttpPost]
        [Route("edit_post")]
        public async Task<IActionResult> Edit_Post([FromForm] EditPostModel model)
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
                        Code = "1011",
                        message = "Could not publish this post "
                    });
                }
                if (model.status == "1012")
                {
                    return BadRequest(new
                    {
                        code = "1012",
                        message = "OK",
                        warning = "Content not suitable for global posting"
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



                var id_modified = cryptoHelper.GenerateRandomId();
                post.id_modified = id_modified;
                post.described = model.described;
                post.status = model.status;
                post.modified = DateTime.Now;

                var old_video = dataAccess.Query<Video>("select * from Video where id_post=@id", new { id = post.id_modified }).FirstOrDefault();
                var old_Image = dataAccess.QueryList<Images>("select * from Image where id_post=@id", new { id = post.id_modified });

                dataAccess.Query<Posts>(@"INSERT INTO [dbo].[Posts] ([id_user], [id], [id_modified],[described], [status], [created], [modified], [name],   [is_marked], [state],[banned])
                                                        VALUES (@id_user, @id,@id_modified, @Described, @status, @created,@modified, @name, @is_marked, @state,@banned)", new
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
                    coins = $"{user.coins - 1}",

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
        [Route("delete_post")]
        public IActionResult delete_post(string token, string id)
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
                id_modified.ForEach(id =>
                {
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
                    coins = $"{user.coins - 1}"
                });

            }
            catch (Exception ex)
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
        public IActionResult Report_post(string token, string id, string subject, string details)
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
                var list_ban = dataAccess.QueryList<string>("select id_user_was_banned from List_ban where id_user=@id_user", new { post?.id_user });
            
                bool isUserBanned = list_ban != null && list_ban.Any(bannerUser => bannerUser == user.id);
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
                if (user.id == post.id_user)
                {
                    return BadRequest(new
                    {
                        Code = "501",
                        message = "You cannot report yourself"
                    });
                }


                var id_report = cryptoHelper.GenerateRandomId();
                dataAccess.Execute("insert into [dbo].[Report] ([id],[id_user_report],[id_post],[subject],[details]) values (@id,@id_user_report,@id_post,@subject,@details)", new { id = id_report, id_user_report = user.id, subject, details, id_post = post.id });



                return Ok(new
                {
                    Code = "1000",
                    message = "Ok",

                });
            }
            catch (Exception ex)
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
        public IActionResult Feel(string token, string id, string type)
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
                var list_ban = dataAccess.QueryList<string>("select id_user_was_banned from List_ban where id_user=@id_user", new { post?.id_user });
           
                bool isUserBanned = list_ban != null && list_ban.Any(bannerUser => bannerUser == user.id);
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
                    dataAccess.Execute("insert into Feel ([id_user],[id_post],[type]) values(@id_user,@id_post,@type) ", new { id_post = post.id, id_user = user.id, type });
                }
                else
                {
                    dataAccess.Execute("update Feel set type=@type where id_user=@id_user and id_post=@id_post", new { id_post = post.id, id_user = user.id, type });

                }




                return Ok(new
                {
                    code = "1000",
                    message = "Ok",

                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                    ex.Data,
                    ex.StackTrace,
                });
            }
        }

        [HttpPost]
        [Route("get_mark_comment")]
        public IActionResult getMarkComment(string token, string id, string? index, string? count)
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
                //There doesn't appear to be any bugs in this code snippet.

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

                var list_ban = dataAccess.QueryList<string>("select id_user_was_banned from List_ban where id_user=@id_user", new { post?.id_user });
                if (post == null)
                {
                    return BadRequest(new
                    {
                        code = "9992",
                        message = "Posts is not exited"
                    });
                }
                bool isUserBanned = list_ban != null &&list_ban.Any(bannerUser => bannerUser == user.id);
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

                List<getMarkComment> list_mark_comment = new List<getMarkComment>();


                if (index == null && count == null)
                {
                    var mark_comment = dataAccess.QueryList<getMarkComment>("select Top 20 * from mark_comment where id_post=@id_post",
                        new { id_post = post.id });
                    if (mark_comment != null)
                    {

                        foreach (var item in mark_comment)
                        {
                           
                            var mark = dataAccess.Query<Mark>("select * from mark where id=@id",
                                new { id = item.id_mark }).FirstOrDefault();
                            var comment = dataAccess.Query<Comment>("select * from comment where id=@id",
                                new { id = item.id_comment }).FirstOrDefault();
                            var poster = dataAccess.Query<Poster>("select * from users where id=@id", new { id = item.id_user }).FirstOrDefault();


                            if (mark != null && poster != null)
                            {
                                item.mark = mark;
                                item.mark.poster = poster;
                                list_mark_comment.Add(item);
                            }
                            if (comment != null && poster != null)
                            {
                                item.comments = comment;
                                item.comments.poster = poster;
                                list_mark_comment.Add(item);

                            }

                        }

                    }

                }
                else
                {
                    var mark_comment = dataAccess.QueryList<getMarkComment>("select * from mark_comment where id_post=@id_post order by id DESC OFFSET @index ROWS FETCH NEXT @count ROWS ONLY",
                                       new { id_post = post.id, index, count });
                    if (mark_comment != null)
                    {
                        foreach (var item in mark_comment)
                        {

                            var mark = dataAccess.Query<Mark>("select * from mark where id=@id",
                                                           new { id = item.id_mark }).FirstOrDefault();
                            var comment = dataAccess.Query<Comment>("select * from comment where id=@id",
                                                           new { id = item.id_comment }).FirstOrDefault();
                            var poster = dataAccess.Query<Poster>("select * from users where id=@id", new { id = item.id_user }).FirstOrDefault();
                            if (mark != null && poster!=null)
                            {
                                item.mark = mark;
                                item.mark.poster = poster;
                                list_mark_comment.Add(item);
                            }
                            if (comment != null && poster != null)
                            {
                                item.comments = comment;
                                item.comments.poster= poster;
                                list_mark_comment.Add(item);

                            }
                        }
                    }
                }
                return Ok(new
                {
                    code = "1000",
                    message = "Ok",
                    isUserBanned,
                    list_ban,
                    data = list_mark_comment,

                });



            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                    ex.Data,
                    ex.StackTrace,
                });
            }

            //There doesn't appear to be any bugs in this code snippet.

        }
        [HttpPost]
        [Route("set_mark_comment")]
        public IActionResult set_mark_comment(string token, string id, string content, string? type)
        {
            try
            {
                string typedb = string.Empty;
                if (type != null)
                {

                    if (type == "1")
                    {
                        typedb = "fake";
                    }
                    else if (type == "0")
                    {
                        typedb = "trust";
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            code = "1004",
                            message = "Parameter value is invalid"
                        });
                    }
                }

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new
                    {
                        Code = "1004",
                        Message = "Parameter value is invalid"
                    });

                }

                //There doesn't appear to be any bugs in this code snippet.

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

                var list_ban = dataAccess.QueryList<string>("select id_user_was_banned from List_ban where id_user=@id_user", new { post?.id_user });
                if (post == null)
                {
                    return BadRequest(new
                    {
                        code = "9992",
                        message = "Posts is not exited"
                    });
                }
                bool isUserBanned = list_ban != null && list_ban.Any(bannerUser => bannerUser == user.id);
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



                if (type != null)
                {
                    if (content != null)
                    {

                        var id_mark = cryptoHelper.GenerateRandomId();
                        var id_mark_comment = cryptoHelper.GenerateRandomId();
                        dataAccess.Execute("insert into mark ([id],[mark_content],[type_mark]) values (@id,@mark_content,@type_mark)", new { id = id_mark, mark_content = content, type_mark = typedb });
                        dataAccess.Execute("insert into mark_comment ([id],[id_mark],[id_post],[id_user]) values (@id,@id_mark,@id_post,@id_user)", new { id = id_mark_comment, id_mark, id_post = id, id_user = user.id });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            code = "1004",
                            message = "Parameter value is invalid"
                        });
                    }

                }
                else if (type == null)
                {
                    if (content != null)
                    {
                        var id_commnent = cryptoHelper.GenerateRandomId();
                        var id_mark_comment = cryptoHelper.GenerateRandomId();
                        dataAccess.Execute("insert into comment ([id],[content],[created]) values (@id,@content,@created)", new { id = id_commnent, content, created = DateTime.Now });
                        dataAccess.Execute("insert into mark_comment ([id],[id_comment],[id_post],[id_user]) values (@id,@id_commnent,@id_post,@id_user)", new { id_user = user.id, id = id_mark_comment, id_commnent, id_post = id });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            code = "1004",
                            message = "Parameter value is invalid"
                        });
                    }
                }




                var mark_comment = dataAccess.QueryList<getMarkComment>("select * from mark_comment where id_post=@id_post",
                                           new { id_post = post.id });

                var mark_Comments = new List<getMarkComment>();

                foreach (var item in mark_comment)
                {

                    var mark = dataAccess.Query<Mark>("select * from mark where id=@id",
                                                                          new { id = item.id_mark }).FirstOrDefault();
                    //There doesn't appear to be any bugs in this code snippet. However, it's always a good practice to handle exceptions and null values when using the FirstOrDefault() method.
                    var comment = dataAccess.Query<Comment>("select * from comment where id=@id",
                                                                          new { id = item.id_comment }).FirstOrDefault();


                    var poster= dataAccess.Query<Poster>("select * from users where id=@id",new {id=item.id_user}).FirstOrDefault();
                    if (mark != null && poster!=null)
                    {

                        item.mark = mark;
                        item.mark.poster=poster;
                        mark_Comments.Add(item);
                       
                    }
                    if (comment != null && poster!=null)
                    {
                        item.comments = comment;
                        item.comments.poster=poster;
                        mark_Comments.Add(item);
                    }


                }
                return Ok(new
                {
                    code = "1000",
                    message = "Ok",
                    data = mark_Comments

                });


            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    code = "9999",
                    message = ex.Message,
                    ex.Data,
                    ex.StackTrace,
                });
            }
        }


    }
}

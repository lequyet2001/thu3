using thu3.Model.PostModel;

namespace thu3.Model
{
    public class getMarkComment
    {
        public string id {  get; set; }
        public string id_user { get; set; }
        public string id_post { get; set; }
        public string id_mark { get; set; }
        public string id_comment { get; set; }
        public Mark mark { get; set; }=new Mark();
        public Comment comments { get; set; } = new Comment();

   }
}

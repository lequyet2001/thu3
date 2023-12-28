

namespace thu3.Model.PostModel
{
    public class Comment
    {
        public string content {  get; set; }
        public DateTime created { get; set; }
        public Poster poster { get; set; } = new Poster();
    }
}

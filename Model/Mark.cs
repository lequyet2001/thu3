namespace thu3.Model
{
    public class Mark
    {
        public string mark_content { get; set; }=string.Empty;
        public string type_mark { get; set; } = string.Empty;
        public Poster poster { get; set; } = new Poster();
    }
}

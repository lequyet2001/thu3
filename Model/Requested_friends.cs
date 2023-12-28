namespace thu3.Model
{
    public class Requested_friends
    {
        public string id_user { get; set; }
        public string id_friend { get; set; }
        public string accept { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
    }   
}

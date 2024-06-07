namespace ChatGPT.Models
{
    public class RequestAPI
    {
        public string model { get; set; }
        public List<Mess> messages { get; set; }
    }
}

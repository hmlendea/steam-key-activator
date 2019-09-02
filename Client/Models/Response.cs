namespace SteamKeyActivator.Client.Models
{
    public abstract class Response
    {
        public abstract bool IsSuccess { get; }
        
        public string HmacToken { get; set; }
    }
}

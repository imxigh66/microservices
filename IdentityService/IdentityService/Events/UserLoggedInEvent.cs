namespace IdentityService.Events
{
    public class UserLoggedInEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime LoggedInAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}

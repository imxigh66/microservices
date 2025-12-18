namespace IdentityService.Events
{
    public class UserRegisteredEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

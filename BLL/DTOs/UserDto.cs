namespace BLL.DTOs
{
    /// <summary>
    /// Data Transfer Object for User information between BLL and Presentation layers
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

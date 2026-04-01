namespace UserManagementApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateOnly? BirthDate { get; set; }

        public string BirthCity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}

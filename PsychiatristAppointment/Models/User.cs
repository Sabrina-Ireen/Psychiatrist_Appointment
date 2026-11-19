public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Specialization { get; set; } = ""; // For doctors
    public string Role { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Add these for authentication
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
}

using System.ComponentModel.DataAnnotations;

public enum UserRole
{
    Admin,
    Divisi
}

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; }

    [Required, MaxLength(255)]
    public string Password { get; set; } // nanti bisa hash

    [MaxLength(100)]
    public string Nama { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Divisi;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

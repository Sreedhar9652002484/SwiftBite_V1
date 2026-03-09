namespace SwiftBite.UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string AuthUserId { get; private set; } = string.Empty; // from AuthServer
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public ICollection<Address> Addresses { get; private set; } = new List<Address>();
    public UserPreference? Preference { get; private set; }

    // EF Constructor
    private User() { }

    public static User Create(
        string authUserId,
        string firstName,
        string lastName,
        string email,
        DateTime dateOfBirth)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void UpdateProfile(string firstName, string lastName,
        string? phoneNumber, string? profilePictureUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ProfilePictureUrl = profilePictureUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
}
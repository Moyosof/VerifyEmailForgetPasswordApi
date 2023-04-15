namespace VerifyApiForgetApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public byte[] PasswordHash { get; set; } = new byte[32];

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set;}
    }
}

//Note: This is the model that will pass to the database
// the PasswordHash for hashing users password in byte 32 whiile
// Salting is to make the password encryption unique by giving it a unique id
// Verification Token to verify the user email
// PasswordResetToken for forget password

// install entityframework core, sqlserver and design

// to use the dotnet global tool, use the package manager console
// dotnet tool install --global dotnet-ef
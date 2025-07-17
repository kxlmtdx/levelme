using System.ComponentModel.DataAnnotations;

namespace levelme.Models
{
    public class Accounts
    {
        [Key]
        public int AccountId { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        //JWT
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}

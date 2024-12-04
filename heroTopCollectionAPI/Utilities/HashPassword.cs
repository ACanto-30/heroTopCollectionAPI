using System.Security.Cryptography;
using System.Text;

namespace heroTopCollectionAPI.Utilities
{
    public class HashPassword
    {
        public string GenerateSalt()
        {

            byte[] saltBytes = new byte[16];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string HashPasswordWithSalt(string password, string salt)
        {
            string saltedPassword = salt + password;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string plainPassword, string storedHashedPassword, string storedSalt)
        {
            string recalculatedHash = HashPasswordWithSalt(plainPassword, storedSalt);

            return recalculatedHash == storedHashedPassword;
        }
    }
}

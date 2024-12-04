using System.Security.Cryptography;
using System.Text;

namespace heroTopCollectionAPI.Utilities
{
    public class CreateToken
    {
        public string GenerateToken(string userHashedPassword)
        {

            byte[] randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string uniqueData = userHashedPassword + timestamp;

            byte[] uniqueDataBytes = Encoding.UTF8.GetBytes(uniqueData);
            byte[] combinedBytes = new byte[randomBytes.Length + uniqueDataBytes.Length];
            Buffer.BlockCopy(randomBytes, 0, combinedBytes, 0, randomBytes.Length);
            Buffer.BlockCopy(uniqueDataBytes, 0, combinedBytes, randomBytes.Length, uniqueDataBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyToken(string userInputToken, string storedToken)
        {

            return userInputToken == storedToken;
        }
    }
}

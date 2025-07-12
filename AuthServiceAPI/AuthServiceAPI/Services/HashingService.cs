using AuthServiceAPI.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace AuthServiceAPI.Services
{
    public class HashingService : IHashingService
    {
        private const int SaltSize = 16;   // 128 bits
        private const int HashSize = 32;   // 256 bits
        public byte[] HashValue(byte[] value, byte[] salt)
        {
            var argon2 = new Argon2id(value)
            {
                Salt = salt,
                DegreeOfParallelism = 4,    // number of CPU cores to use
                MemorySize = 65536,         // memory usage in KB (64 MB)
                Iterations = 3              // number of iterations
            };

            return argon2.GetBytes(HashSize);
        }
        public (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] passHash = HashValue(Encoding.UTF8.GetBytes(password), salt);

            
            return (passHash, salt);
        }
        public bool VerifyHash (string password, byte[] salt, byte[] hash)
        {
            if (hash == HashValue(Encoding.UTF8.GetBytes(password), salt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}

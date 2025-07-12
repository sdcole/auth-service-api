namespace AuthServiceAPI.Interfaces
{
    public interface IHashingService
    {
        /// <summary>
        /// Hash a value using Argon2id with a provided salt.
        /// </summary>
        /// <param name="value">The input value as a byte array.</param>
        /// <param name="salt">The salt as a byte array.</param>
        /// <returns>The resulting hash.</returns>
        byte[] HashValue(byte[] value, byte[] salt);

        /// <summary>
        /// Hash a password and generate a new salt.
        /// </summary>
        /// <param name="password">The plaintext password.</param>
        /// <returns>A tuple containing the password hash and salt.</returns>
        (byte[] Hash, byte[] Salt) HashPassword(string password);
    }
}

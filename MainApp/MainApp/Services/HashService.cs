using System.Security.Cryptography;

namespace MainApp.Services
{
    internal class HashService
    {
        // Service for generating a hash of a password
        // and verifying the entered password for correctness
        internal static string HashPassword(string userPassword)
        {
            if (userPassword == null)
            {
                throw new NullReferenceException("password");
            }

            byte[] salt;
            byte[] key;
            byte[] hash = new byte[0x31];

            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(userPassword, 0x10, 0x3e8, HashAlgorithmName.SHA512))
            {
                salt = bytes.Salt;
                key = bytes.GetBytes(0x20);
            }

            Buffer.BlockCopy(salt, 0, hash, 1, 0x10);
            Buffer.BlockCopy(key, 0, hash, 0x11, 0x20);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string userPassword)
        {
            if(hashedPassword == null || userPassword == null)
            {
                return false;
            }

            byte[] hashData = Convert.FromBase64String(hashedPassword);

            if (hashData.Length != 0x31 || hashData[0] != 0)
            {
                return false;
            }

            byte[] hashUser;
            byte[] saltData = new byte[0x10];
            byte[] keyData = new byte[0x20];

            Buffer.BlockCopy(hashData, 1, saltData, 0, 0x10);
            Buffer.BlockCopy(hashData, 0x11, keyData, 0, 0x20);

            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(userPassword, saltData, 0x3e8, HashAlgorithmName.SHA512))
            {
                hashUser = bytes.GetBytes(0x20);
            }

            return hashUser.SequenceEqual(keyData);
        }
    }
}

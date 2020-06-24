using System;
using System.Security.Cryptography;
using NLog;

namespace RealmDBHandler.PasswordHanling
{
    internal class PasswordEncryptor
    {
        #region Private fields

        //salt - byte array for storing randmly generated hashing bytes
        //I mean hashing the password
        private readonly byte[] _salt = new byte[16];
        private Random _rand;

        //logging purposes
        private readonly Logger _logger;

        #endregion

        #region Constructor

        public PasswordEncryptor()
        {
            //assigning logger
            _logger = LogManager.GetCurrentClassLogger();

            //generate new hashing bytes
            _salt = GenerateRandomSalt();

            _logger.Info($"Password encryptor object created.");
        }

        //method for generating random salt
        private byte[] GenerateRandomSalt()
        {
            byte[] output = new byte[16];
            _rand = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < _salt.Length; i++)
            {
                output[i] = (byte)_rand.Next(0, 255);
            }

            return output;
        }

        #endregion

        #region Encrypting and decrypting password

        public (string, byte[]) EnctyptPassword(string password)
        {
            _logger.Info("Method for encrypting password fired.");

            //1. Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(password, _salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            //2. Combine the salt and password bytes for later use
            byte[] hashBytes = new byte[36];
            Array.Copy(_salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            //3. Turn the combined salt+hash into a string for storage
            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            //4. Return hashed password
            pbkdf2.Dispose();
            return (savedPasswordHash, _salt);
        }

        public string DecryptPassword(string hashedPassword, string password, byte[] salt)
        {
            _logger.Info("Method for decrypting password fired.");

            // 1.Fetch the stored value
            string savedPasswordHash = hashedPassword;

            //2. Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);

            //3. Get the salt 
            //salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //4. Compute the hash on the password the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            //5. Compare the results 
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    throw new UnauthorizedAccessException();

            //6. Return decrypted password
            pbkdf2.Dispose();
            return password;
        }

        #endregion
    }
}

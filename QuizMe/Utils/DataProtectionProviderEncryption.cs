using Microsoft.AspNetCore.DataProtection;

namespace QuizMe.Utils
{
    public class DataProtectionProviderEncryption
    {
        readonly IDataProtector _protector;
        public DataProtectionProviderEncryption(IDataProtectionProvider rootProvider, string key)
        {
            _protector = rootProvider.CreateProtector(key);
        }

        //Encrypt function
        public string encrypt(string plaintext)
        {
            //IDataProtector protector = _rootProvider.CreateProtector(key);
            string encrypted = _protector.Protect(plaintext);
            return encrypted;
        }

        //Decrypt function
        public string decrypt(string encrypted)
        {
            //IDataProtector protector = _rootProvider.CreateProtector(key);
            string decrypted = _protector.Unprotect(encrypted);
            return decrypted;
        }
    }
}

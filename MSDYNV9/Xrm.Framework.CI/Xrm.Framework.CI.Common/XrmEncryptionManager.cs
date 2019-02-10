using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public interface IXrmEncryption
    {
        string Encrypt(string secret);
        string Decrypt(string encryptedSecret);
    }

    public class XrmEncryptionManager : IXrmEncryption
    {
        #region Properties

        protected ILogger Logger
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public XrmEncryptionManager(ILogger logger )
        {
            Logger = logger;
        }

        #endregion

        #region Methods

        public string Encrypt(string secret)
        {
            Logger.LogVerbose("Ecrypting secret");

            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("secret can't be empty");
            }

            byte[] bytes = Encoding.Default.GetBytes(secret);

            byte[] encryptedBytes = ProtectedData.Protect(bytes, (byte[])null, DataProtectionScope.CurrentUser);

            string encryptedSecret = BitConverter.ToString(encryptedBytes).Replace("-", "");

            Logger.LogVerbose("Secret encrypted");

            return encryptedSecret;
        }

        public string Decrypt(string encryptedSecret)
        {
            Logger.LogVerbose("Decrypting secret");

            byte[] encryptedBytes = StringToByteArray(encryptedSecret);

            byte[] bytes = ProtectedData.Unprotect(encryptedBytes, (byte[])null, DataProtectionScope.CurrentUser);

            string secret = Encoding.Default.GetString(bytes);

            Logger.LogVerbose("Decryptings secret");

            return secret;
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        #endregion
    }
}

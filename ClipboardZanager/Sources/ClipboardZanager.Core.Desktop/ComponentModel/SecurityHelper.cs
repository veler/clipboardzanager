using System;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides a set of functions designed to manage the security
    /// </summary>
    internal static class SecurityHelper
    {
        #region Fields

        private static UIPermission _uiPermissionAllClipboard;
        private static SecurityPermission _unmanagedCodePermission;

        #endregion

        #region Methods

        /// <summary>
        /// Demands all the permission to access to all clipboards from all windows.
        /// </summary>
        [SecurityCritical]
        internal static void DemandAllClipboardPermission()
        {
            if (_uiPermissionAllClipboard == null)
            {
                _uiPermissionAllClipboard = new UIPermission(UIPermissionWindow.AllWindows, UIPermissionClipboard.AllClipboard);
            }
            _uiPermissionAllClipboard.Demand();
        }

        /// <summary>
        /// Demands the permission to work with unmanaged code.
        /// </summary>
        [SecurityCritical]
        internal static void DemandUnmanagedCode()
        {
            if (_unmanagedCodePermission == null)
            {
                _unmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
            _unmanagedCodePermission.Demand();
            _unmanagedCodePermission.Assert();
        }

        /// <summary>
        /// Convert a <see cref="string"/> to a <see cref="SecureString"/>
        /// </summary>
        /// <param name="input">The string to secure</param>
        /// <returns>The <see cref="SecureString"/> that corresponds to the input string</returns>
        internal static SecureString ToSecureString(string input)
        {
            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }

            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>
        /// Convert a <see cref="SecureString"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="input">The secured string</param>
        /// <returns>Returns an unsecured string</returns>
        internal static string ToUnsecureString(SecureString input)
        {
            string returnValue;
            var ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);

            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }

            return returnValue;
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="input">The <see cref="SecureString"/> that contains the string's value</param>
        /// <returns>An encrypted <see cref="string"/></returns>
        internal static string EncryptString(SecureString input)
        {
            return EncryptString(input, ToSecureString(CoreHelper.GetApplicationVersion().ToString()));
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="input">The <see cref="SecureString"/> that contains the string's value</param>
        /// <param name="password">The <see cref="SecureString"/> that represents the password</param>
        /// <returns>An encrypted <see cref="string"/></returns>
        internal static string EncryptString(SecureString input, SecureString password)
        {
            using (var rdb = GetSaltKeys(password))
            {
                var key = rdb.GetBytes(8);
                var iv = rdb.GetBytes(8);
                using (var algorithm = DES.Create())
                using (var transform = algorithm.CreateEncryptor(key, iv))
                {
                    var inputbuffer = Encoding.Unicode.GetBytes(ToUnsecureString(input));
                    var outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                    return DataHelper.ToBase64(outputBuffer);
                }
            }
        }

        /// <summary>
        /// Decrypt an encrypted string
        /// </summary>
        /// <param name="encryptedData">The encrypted string</param>
        /// <returns>A <see cref="SecureString"/> that corresponds to the string's value</returns>
        internal static SecureString DecryptString(string encryptedData)
        {
            return DecryptString(encryptedData, ToSecureString(CoreHelper.GetApplicationVersion().ToString()));
        }

        /// <summary>
        /// Decrypt an encrypted string
        /// </summary>
        /// <param name="encryptedData">The encrypted string</param>
        /// <param name="password">The <see cref="SecureString"/> that represents the password</param>
        /// <returns>A <see cref="SecureString"/> that conrresponds to the string's value</returns>
        internal static SecureString DecryptString(string encryptedData, SecureString password)
        {
            try
            {
                using (var rdb = GetSaltKeys(password))
                {
                    var key = rdb.GetBytes(8);
                    var iv = rdb.GetBytes(8);

                    using (var algorithm = DES.Create())
                    using (var transform = algorithm.CreateDecryptor(key, iv))
                    {
                        var inputbuffer = DataHelper.ByteArrayFromBase64(encryptedData);
                        var outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                        return ToSecureString(Encoding.Unicode.GetString(outputBuffer));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
                return new SecureString();
            }
        }

        /// <summary>
        /// Generate a <see cref="Rfc2898DeriveBytes"/> that we can use to retrieve the KEY and IV.
        /// </summary>
        /// <param name="password">The password</param>
        /// <returns>A <see cref="Rfc2898DeriveBytes"/></returns>
        internal static Rfc2898DeriveBytes GetSaltKeys(SecureString password)
        {
            Requires.NotNull(password, nameof(password));
            return new Rfc2898DeriveBytes(ToUnsecureString(password), new byte[] { 0x53, 0x69, 0x75, 0x6f, 0x65, 0x20, 0x43, 0x69, 0x61, 0x68, 0x6d, 0x6c, 0x6f, 0x72, 0x64, 0x69, 0x64 });
        }

        #endregion
    }
}

using System;
using ClipboardZanager.Shared.Exceptions;

namespace ClipboardZanager.Shared.Core
{
    /// <summary>
    /// Provides a set of methods that can be used to validate data
    /// </summary>
    public static class Requires
    {
        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        public static void NotNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new NotNullRequiredException(parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null or empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new NotNullOrEmptyRequiredException(parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new NotNullOrWhiteSpaceRequiredException(parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is not false.
        /// </summary>
        /// <param name="value">The value to test.</param>
        public static void IsFalse(bool value)
        {
            if (value)
            {
                throw new IsFalseRequiredException();
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is not true.
        /// </summary>
        /// <param name="value">The value to test.</param>
        public static void IsTrue(bool value)
        {
            if (!value)
            {
                throw new IsTrueRequiredException();
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is > 1.
        /// </summary>
        /// <param name="hresult">The value to test.</param>
        public static void VerifySucceeded(int hresult)
        {
            if (hresult > 1)
            {
                throw new InvalidOperationException("Failed with HRESULT: " + hresult.ToString("X"));
            }
        }

        /// <summary>
        /// Returns a boolean that defines whether an operation succeeded.
        /// </summary>
        /// <param name="hresult">The value to test.</param>
        /// <returns>Returns True if it succeeded</returns>
        public static bool Succeeded(int hresult)
        {
            return hresult >= 0;
        }

        /// <summary>
        /// Returns a boolean that defines whether an operation failed.
        /// </summary>
        /// <param name="hresult">The value to test.</param>
        /// <returns>Returns False if it failed</returns>
        public static bool Failed(int hresult)
        {
            return hresult < 0;
        }
    }
}

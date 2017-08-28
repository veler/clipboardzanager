using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardZanager.Shared.Exceptions;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Shared.Tests.Core
{
    [TestClass]
    public class RequiresTests
    {
        [TestMethod]
        [ExpectedException(typeof(NotNullRequiredException))]
        public void RequiresNotNull()
        {
            Requires.NotNull(null, "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(NotNullOrEmptyRequiredException))]
        public void RequiresNotNullOrEmpty()
        {
            Requires.NotNullOrEmpty("", "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(NotNullOrWhiteSpaceRequiredException))]
        public void RequiresNotNullOrWhiteSpace()
        {
            Requires.NotNullOrWhiteSpace(" ", "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(IsFalseRequiredException))]
        public void RequiresIsFalse()
        {
            Requires.IsFalse(true);
        }

        [TestMethod]
        [ExpectedException(typeof(IsTrueRequiredException))]
        public void RequiresIsTrue()
        {
            Requires.IsTrue(false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RequiresVerifySucceeded()
        {
            Requires.VerifySucceeded(12);
        }
    }
}

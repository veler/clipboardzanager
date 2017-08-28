using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Tests.Mocks;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ClipboardZanager.Shared.Tests.Logs
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Logs()
        {
            try
            {
                var instance = Logger.Instance;
                Assert.Fail();
            }
            catch
            {
            }

            Logger.InstanceLogSession = new LogMock();

            try
            {
                Logger.InstanceLogSession = new LogMock();
                Assert.Fail();
            }
            catch
            {
            }

            var logMock = (LogMock)Logger.InstanceLogSession;
            var date1 = DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss");

            Logger.Instance.Information("This is an info");
            Logger.Instance.Information("This is another info", "Hello");

            Logger.Instance.Debug("This is a debug");
            Logger.Instance.Warning("This is a warn");
            Logger.Instance.Error(new Exception("Error"));
            Logger.Instance.Information("Hello world");

            using (Logger.Instance.Stopwatch("This is a stopwatch"))
            {
                Task.Delay(100).Wait();
            }

            var date2 = DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss");
            Assert.AreEqual(logMock.GetLogs().ToString(), "");
            Logger.Instance.Flush();
            Assert.AreNotEqual(logMock.GetLogs().ToString(), "");

            try
            {
                Logger.Instance.Fatal("This is a fatal error", new Exception("Error"));
                Assert.Fail();
            }
            catch
            {
            }

            try
            {
                Logger.Instance.Information("This is an info");
                Assert.Fail();
            }
            catch
            {
            }

            var logs = logMock.GetLogs().ToString();
            if (Debugger.IsAttached)
            {
                Assert.IsTrue(logs.Contains($"[Debug] [Logs] [{date1}] This is a debug"));
            }

            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date1}] This is an info"));
            Assert.IsTrue(logs.Contains($"[Information] [Hello] [{date1}] This is another info"));
            Assert.IsTrue(logs.Contains($"[Warning] [Logs] [{date1}] This is a warn"));
            Assert.IsTrue(logs.Contains($"[Error] [Logs] [{date1}] Error"));
            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date1}] Hello world"));
            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date1}] This is a stopwatch : stopwatch started !"));
            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date2}] This is a stopwatch : 00:00:00:1"));
            Assert.IsTrue(logs.Contains($"[Fatal] [Logs in LoggerTests.cs, line 59] [{date2}] This is a fatal error"));
            Assert.IsTrue(logs.Contains($"[Fatal] [Additional Information] [{date2}] Additional info..."));
        }
    }
}

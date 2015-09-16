#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;

namespace Axantum.AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestLogging
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            TypeMap.Register.Singleton<ILogging>(() => new Logging());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestLoggingLevels()
        {
            Resolve.Log.SetLevel(LogLevel.Fatal);
            Assert.That(Resolve.Log.IsDebugEnabled, Is.False, "When logging is off, Debug should be off.");
            Assert.That(Resolve.Log.IsInfoEnabled, Is.False, "When logging is off, Info should be off.");
            Assert.That(Resolve.Log.IsWarningEnabled, Is.False, "When logging is off, Warning should be off.");
            Assert.That(Resolve.Log.IsErrorEnabled, Is.False, "When logging is off, Error should be off.");

            Resolve.Log.SetLevel(LogLevel.Error);
            Assert.That(Resolve.Log.IsDebugEnabled, Is.False, "When Error is enabled, Debug should be off.");
            Assert.That(Resolve.Log.IsInfoEnabled, Is.False, "When Error is enabled, Info should be off.");
            Assert.That(Resolve.Log.IsWarningEnabled, Is.False, "When Error is enabled, Warning should be off.");
            Assert.That(Resolve.Log.IsErrorEnabled, Is.True, "When Error is enabled, Error should be on.");

            Resolve.Log.SetLevel(LogLevel.Warning);
            Assert.That(Resolve.Log.IsDebugEnabled, Is.False, "When Warning is enabled, Debug should be off.");
            Assert.That(Resolve.Log.IsInfoEnabled, Is.False, "When Warning is enabled, Info should be off.");
            Assert.That(Resolve.Log.IsWarningEnabled, Is.True, "When Warning is enabled, Warning should be on.");
            Assert.That(Resolve.Log.IsErrorEnabled, Is.True, "When Warning is enabled, Error should be on.");

            Resolve.Log.SetLevel(LogLevel.Info);
            Assert.That(Resolve.Log.IsDebugEnabled, Is.False, "When Info is enabled, Debug should be off.");
            Assert.That(Resolve.Log.IsInfoEnabled, Is.True, "When Info is enabled, Info should be on.");
            Assert.That(Resolve.Log.IsWarningEnabled, Is.True, "When Info is enabled, Warning should be on.");
            Assert.That(Resolve.Log.IsErrorEnabled, Is.True, "When Info is enabled, Error should be on.");

            Resolve.Log.SetLevel(LogLevel.Debug);
            Assert.That(Resolve.Log.IsDebugEnabled, Is.True, "When Verbose is enabled, Debug should be on.");
            Assert.That(Resolve.Log.IsInfoEnabled, Is.True, "When Verbose is enabled, Info should be on.");
            Assert.That(Resolve.Log.IsWarningEnabled, Is.True, "When Verbose is enabled, Warning should be on.");
            Assert.That(Resolve.Log.IsErrorEnabled, Is.True, "When Verbose is enabled, Error should be on.");
        }

        [Test]
        public static void TestLoggingListenerAndLevels()
        {
            string listenerMessage = null;

            DelegateTraceListener traceListener = new DelegateTraceListener("AxCryptTestListener", (string message) =>
            {
                listenerMessage = (listenerMessage ?? String.Empty) + message;
            });

            Trace.Listeners.Add(traceListener);
            try
            {
                Resolve.Log.SetLevel(LogLevel.Fatal);

                listenerMessage = null;
                Resolve.Log.LogDebug("Verbose" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is off, Verbose logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogInfo("Info" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is off, Info logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogWarning("Warning" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is off, Warning logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogError("Error" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is off, Error logging should not generate a message.");

                Resolve.Log.SetLevel(LogLevel.Error);

                listenerMessage = null;
                Resolve.Log.LogDebug("Verbose" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Error, Verbose logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogInfo("Info" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Error, Info logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogWarning("Warning" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Error, Warning logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogFatal("Fatal" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Fatal"), "When logging is Error, Fatal logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogError("Error" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Error"), "When logging is Error, Error logging should generate a message.");

                Resolve.Log.SetLevel(LogLevel.Warning);

                listenerMessage = null;
                Resolve.Log.LogDebug("Verbose" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Warning, Verbose logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogInfo("Info" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Warning, Info logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogWarning("Warning" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Warning"), "When logging is Warning, Warning logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogError("Error" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Error"), "When logging is Warning, Error logging should generate a message.");

                Resolve.Log.SetLevel(LogLevel.Info);

                listenerMessage = null;
                Resolve.Log.LogDebug("Verbose" + Environment.NewLine);
                Assert.That(listenerMessage, Is.EqualTo(null), "When logging is Info, Verbose logging should not generate a message.");

                listenerMessage = null;
                Resolve.Log.LogInfo("Info" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Info"), "When logging is Info, Info logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogWarning("Warning" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Warning"), "When logging is Info, Warning logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogError("Error" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Error"), "When logging is Info, Error logging should generate a message.");

                Resolve.Log.SetLevel(LogLevel.Debug);

                listenerMessage = null;
                Resolve.Log.LogDebug("Verbose" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Verbose"), "When logging is Verbose, Verbose logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogInfo("Info" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Info"), "When logging is Verbose, Info logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogWarning("Warning" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Warning"), "When logging is Verbose, Warning logging should generate a message.");

                listenerMessage = null;
                Resolve.Log.LogError("Error" + Environment.NewLine);
                Assert.That(listenerMessage.Contains("Error"), "When logging is Verbose, Error logging should generate a message.");
            }
            finally
            {
                Trace.Listeners.Remove(traceListener);
            }
        }
    }
}
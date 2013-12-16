#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Ipc;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCommandLine
    {
        private static FakeRequestClient _fakeClient;

        private static FakeRequestServer _fakeServer;

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            FactoryRegistry.Instance.Singleton<FakeRequestClient>(() => new FakeRequestClient());
            FactoryRegistry.Instance.Singleton<FakeRequestServer>(() => new FakeRequestServer());
            _fakeClient = FactoryRegistry.Instance.Singleton<FakeRequestClient>();
            _fakeServer = FactoryRegistry.Instance.Singleton<FakeRequestServer>();
            FactoryRegistry.Instance.Singleton<CommandService>(() => new CommandService(_fakeServer, _fakeClient));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestFailedOpen()
        {
            _fakeClient.FakeDispatcher = (command) => { return CommandStatus.Error; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[] { "file.axx" });
            FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = true;
            cl.Execute();

            Assert.That(FakeRuntimeEnvironment.Instance.ExitCode, Is.EqualTo(1), "An error during Open shall return status code 1.");
        }

        [Test]
        public static void TestExit()
        {
            bool wasExit = false;
            _fakeServer.Request += (sender, e) =>
            {
                wasExit = e.Command.RequestCommand == CommandVerb.Exit;
            };

            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };

            CommandLine cl = new CommandLine("axcrypt.exe", new string[] { "-x" });
            FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = true;
            cl.Execute();

            Assert.That(wasExit, Is.True);
        }

        [Test]
        public static void TestNeedToLaunchFirstInstance()
        {
            FakeRuntimeEnvironment.Instance.Launcher = (string path) => { FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = path == "axcrypt.exe"; return new FakeLauncher(path); };

            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[0]);
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            cl.Execute();
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.True);
        }

        [Test]
        public static void TestFailedToLaunchFirstInstance()
        {
            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[0]);
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            cl.Execute();
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            Assert.That(FakeRuntimeEnvironment.Instance.ExitCode, Is.EqualTo(2), "Failed to start the first instance shall return status code 2.");
        }
    }
}
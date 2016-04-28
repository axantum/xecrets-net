#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

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
            TypeMap.Register.Singleton<FakeRequestClient>(() => new FakeRequestClient());
            TypeMap.Register.Singleton<FakeRequestServer>(() => new FakeRequestServer());
            _fakeClient = New<FakeRequestClient>();
            _fakeServer = New<FakeRequestServer>();
            TypeMap.Register.Singleton<CommandService>(() => new CommandService(_fakeServer, _fakeClient));
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
                wasExit = e.Command.Verb == CommandVerb.Exit;
            };

            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };

            CommandLine cl = new CommandLine("axcrypt.exe", new string[] { "--exit" });
            FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = true;
            cl.Execute();

            Assert.That(wasExit, Is.True);
        }

        [Test]
        public static void TestNeedToLaunchFirstInstance()
        {
            var mock = new Mock<FakeLauncher>() { CallBase = true };
            mock.Setup(x => x.Launch(It.IsAny<string>()))
                .Callback((string path) =>
                {
                    FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = path == "axcrypt.exe";
                });

            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.New<ILauncher>(() => mock.Object);

            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[0]);
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            cl.Execute();
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.True);
        }

        [Test]
        public static void TestFailedToLaunchFirstInstance()
        {
            TypeMap.Register.New<ILauncher>(() => new FakeLauncher());
            _fakeClient.FakeDispatcher = (command) => { _fakeServer.AcceptRequest(command); return CommandStatus.Success; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[0]);
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            cl.Execute();
            Assert.That(FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning, Is.False);
            Assert.That(FakeRuntimeEnvironment.Instance.ExitCode, Is.EqualTo(2), "Failed to start the first instance shall return status code 2.");
        }
    }
}
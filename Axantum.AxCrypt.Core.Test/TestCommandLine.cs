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
            CommandLine cl = new CommandLine(new string[] { "file.axx" });
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

            CommandLine cl = new CommandLine(new string[] { "--exit" });
            cl.Execute();

            Assert.That(wasExit, Is.True);
        }
    }
}
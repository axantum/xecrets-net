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
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            FactoryRegistry.Instance.Singleton<FakeRequestClient>(() => new FakeRequestClient());
            FactoryRegistry.Instance.Singleton<CommandService>(() => new CommandService(new FakeRequestServer(), FactoryRegistry.Instance.Singleton<FakeRequestClient>()));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestFailedOpen()
        {
            FakeRequestClient client = FactoryRegistry.Instance.Singleton<FakeRequestClient>();
            client.FakeDispatcher = (method, content) => { return CommandStatus.Error; };
            CommandLine cl = new CommandLine("axcrypt.exe", new string[] { "file.axx" });
            FakeRuntimeEnvironment.Instance.IsFirstInstanceRunning = true;
            cl.Execute();

            Assert.That(FakeRuntimeEnvironment.Instance.ExitCode, Is.EqualTo(1), "An error during Open shall return status code 1.");
        }
    }
}
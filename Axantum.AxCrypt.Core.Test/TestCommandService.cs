#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCommandService
    {
        [Test]
        public static void TestCall()
        {
            IRequestServer serverMock = Mock.Of<IRequestServer>();
            IRequestClient clientMock = Mock.Of<IRequestClient>(x => x.Dispatch(It.Is<CommandServiceEventArgs>(ca => ca.RequestCommand == CommandVerb.Open)) == CommandStatus.Success);

            CommandService service = new CommandService(serverMock, clientMock);
            CommandStatus status = service.Call(CommandVerb.Open, @"C:\folder\file.axx");

            Assert.That(status, Is.EqualTo(CommandStatus.Success), "The call should indicate success.");
        }

        [Test]
        public static void TestStart()
        {
            IRequestServer serverMock = Mock.Of<IRequestServer>();
            IRequestClient clientMock = Mock.Of<IRequestClient>();

            CommandService service = new CommandService(serverMock, clientMock);
            service.StartListening();

            Assert.DoesNotThrow(() => Mock.Get(serverMock).Verify(s => s.Start()));
        }

        [Test]
        public static void TestServerReceive()
        {
            IRequestServer serverMock = Mock.Of<IRequestServer>();
            IRequestClient clientMock = Mock.Of<IRequestClient>();

            CommandService service = new CommandService(serverMock, clientMock);
            bool received = false;
            service.Received += (sender, e) => received = e.RequestCommand == CommandVerb.Exit;

            Mock.Get(serverMock).Raise(s => s.Request += null, new RequestCommandEventArgs(new CommandServiceEventArgs(CommandVerb.Exit)));

            Assert.That(received, Is.True, "A command should be received.");
        }
    }
}
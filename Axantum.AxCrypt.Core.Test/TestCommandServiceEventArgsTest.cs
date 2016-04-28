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
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Core.Ipc;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCommandServiceEventArgsTest
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestStringSerialization()
        {
            CommandServiceEventArgs args = new CommandServiceEventArgs();
            string serialized = Resolve.Serializer.Serialize(args);

            args = Resolve.Serializer.Deserialize<CommandServiceEventArgs>(serialized);

            Assert.That(args.Verb, Is.EqualTo(CommandVerb.Unknown));
            Assert.That(args.Arguments.Count(), Is.EqualTo(0));
        }
    }
}
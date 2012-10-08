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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestExceptions
    {
        [Test]
        public static void TestAxCryptExceptions()
        {
            Assert.Throws<FileFormatException>(() =>
            {
                throw new FileFormatException();
            });
            try
            {
                throw new FileFormatException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }

            Assert.Throws<InternalErrorException>(() =>
            {
                throw new InternalErrorException();
            });
            try
            {
                throw new InternalErrorException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }

            Assert.Throws<Axantum.AxCrypt.Core.Runtime.InvalidDataException>(() =>
            {
                throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException();
            });
            try
            {
                throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }
        }

        [Test]
        public static void TestInnerException()
        {
            try
            {
                int i = (int)new object();
                // Use the instance to avoid FxCop errors.
                Object.Equals(i, null);
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new FileFormatException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.FileFormatError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }

            try
            {
                if ((int)new object() == 0) { }
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new InternalErrorException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.InternalError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }

            try
            {
                if ((int)new object() == 0) { }
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.DataError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }
        }

        [Test]
        public static void TestAxCryptExceptionSerialization()
        {
            FileFormatException ffe = new FileFormatException("A test-exception");
            IFormatter ffeFormatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                ffeFormatter.Serialize(stream, ffe);
                stream.Position = 0;
                FileFormatException deserializedFfe = (FileFormatException)ffeFormatter.Deserialize(stream);
                Assert.That(deserializedFfe.ErrorStatus, Is.EqualTo(ErrorStatus.FileFormatError), "The deserialized status should be the same as the original.");
                Assert.That(deserializedFfe.Message, Is.EqualTo("A test-exception"), "The deserialized message should be the same as the original.");
            }

            InternalErrorException iee = new InternalErrorException("A test-exception", ErrorStatus.InternalError);
            IFormatter ieeFormatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                ieeFormatter.Serialize(stream, iee);
                stream.Position = 0;
                InternalErrorException deserializedFfe = (InternalErrorException)ieeFormatter.Deserialize(stream);
                Assert.That(deserializedFfe.ErrorStatus, Is.EqualTo(ErrorStatus.InternalError), "The deserialized status should be the same as the original.");
                Assert.That(deserializedFfe.Message, Is.EqualTo("A test-exception"), "The deserialized message should be the same as the original.");
            }

            Axantum.AxCrypt.Core.Runtime.InvalidDataException ide = new Axantum.AxCrypt.Core.Runtime.InvalidDataException("A test-exception");
            IFormatter ideFormatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                ideFormatter.Serialize(stream, ide);
                stream.Position = 0;
                Axantum.AxCrypt.Core.Runtime.InvalidDataException deserializedFfe = (Axantum.AxCrypt.Core.Runtime.InvalidDataException)ideFormatter.Deserialize(stream);
                Assert.That(deserializedFfe.ErrorStatus, Is.EqualTo(ErrorStatus.DataError), "The deserialized status should be the same as the original.");
                Assert.That(deserializedFfe.Message, Is.EqualTo("A test-exception"), "The deserialized message should be the same as the original.");
            }
        }
    }
}
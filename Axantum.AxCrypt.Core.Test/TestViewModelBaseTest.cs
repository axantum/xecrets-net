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
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestViewModelBaseTest
    {
        private enum TestValidationErrorCode
        {
            Unknown = 0,
            BadString = 13,
            BadInt = 14,
        }

        private class TestViewModel : ViewModelBase
        {
            public string StringProperty { get { return GetProperty<string>("StringProperty"); } set { SetProperty("StringProperty", value); } }

            public int IntProperty { get { return GetProperty<int>("IntProperty"); } set { SetProperty("IntProperty", value); } }

            public override string this[string columnName]
            {
                get
                {
                    switch (columnName)
                    {
                        case "StringProperty":
                            if (StringProperty == "Error")
                            {
                                ValidationError = (int)TestValidationErrorCode.BadString;
                                return "StringProperty Error";
                            }
                            break;

                        case "IntProperty":
                            if (IntProperty < 0)
                            {
                                ValidationError = (int)TestValidationErrorCode.BadInt;
                                return "IntProperty Error";
                            }
                            break;
                    }
                    return base[columnName];
                }
            }
        }

        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IUIThread>(() => new FakeUIThread());
        }

        [TearDown]
        public static void Teardown()
        {
        }

        [Test]
        public static void TestGetSetProperty()
        {
            TestViewModel tvm = new TestViewModel();

            Assert.That(tvm.StringProperty, Is.Null);

            tvm.StringProperty = "A string value";
            Assert.That(tvm.StringProperty, Is.EqualTo("A string value"));
        }

        [Test]
        public static void TestSetPropertyValueChanged()
        {
            bool changed = false;
            string value = null;
            TestViewModel tvm = new TestViewModel();
            tvm.BindPropertyChanged<string>("StringProperty", (v) => { value = v; changed = true; });

            Assert.That(tvm.StringProperty, Is.Null);

            tvm.StringProperty = "A string value";
            Assert.That(tvm.StringProperty, Is.EqualTo("A string value"));

            Assert.That(changed, Is.True);
            Assert.That(value, Is.EqualTo("A string value"));

            value = null;
            changed = false;
            tvm.StringProperty = "A string value";
            Assert.That(tvm.StringProperty, Is.EqualTo("A string value"));

            Assert.That(changed, Is.False);
            Assert.That(value, Is.Null);

            tvm.StringProperty = "Another string value";
            Assert.That(tvm.StringProperty, Is.EqualTo("Another string value"));
            Assert.That(changed, Is.True);
            Assert.That(value, Is.EqualTo("Another string value"));
        }

        [Test]
        public static void TestErrorNonExistingProperty()
        {
            TestViewModel tvm = new TestViewModel();

            string s;
            Assert.Throws<ArgumentException>(() => { s = tvm["NonExisting"]; });
        }

        [Test]
        public static void TestNoErrorProperty()
        {
            TestViewModel tvm = new TestViewModel();

            Assert.That(tvm["StringProperty"].Length, Is.EqualTo(0));
            Assert.That(tvm.Error.Length, Is.EqualTo(0));
        }

        [Test]
        public static void TestPropertyError()
        {
            TestViewModel tvm = new TestViewModel();
            tvm.StringProperty = "Error";
            tvm.IntProperty = -2;

            Assert.That(tvm["StringProperty"], Is.EqualTo("StringProperty Error"));
            Assert.That(tvm["IntProperty"], Is.EqualTo("IntProperty Error"));

            Assert.That(tvm.Error, Is.StringContaining("StringProperty Error"));
            Assert.That(tvm.Error, Is.StringContaining("IntProperty Error"));
        }

        [Test]
        public static void TestValidationError()
        {
            TestViewModel tvm = new TestViewModel();
            TestValidationErrorCode errorCode = TestValidationErrorCode.Unknown;
            tvm.BindPropertyChanged<int>("ValidationError", (n) => { errorCode = (TestValidationErrorCode)n; });

            tvm.StringProperty = "Error";
            Assert.That(tvm.Error.Length, Is.GreaterThan(0));
            Assert.That(errorCode, Is.EqualTo(TestValidationErrorCode.BadString));
            Assert.That(tvm.ValidationError, Is.EqualTo((int)TestValidationErrorCode.BadString));

            tvm.IntProperty = -100;
            Assert.That(tvm.Error.Length, Is.GreaterThan(0));
            Assert.That(errorCode, Is.EqualTo(TestValidationErrorCode.BadInt));
            Assert.That(tvm.ValidationError, Is.EqualTo((int)TestValidationErrorCode.BadInt));

            tvm.StringProperty = "";
            tvm.IntProperty = 100;
            Assert.That(tvm.Error.Length, Is.EqualTo(0));
        }
    }
}
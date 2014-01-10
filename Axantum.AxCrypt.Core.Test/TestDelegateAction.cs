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

using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDelegateAction
    {
        [SetUp]
        public static void Setup()
        {
        }

        [TearDown]
        public static void Teardown()
        {
        }

        [Test]
        public static void TestCanExecute()
        {
            int result = 0;
            DelegateAction<int> action = new DelegateAction<int>((i) => result = i + i, (i) => i == result);

            Assert.That(action.CanExecute(0), Is.True);
            Assert.That(action.CanExecute(1), Is.False);
        }

        [Test]
        public static void TestCanExecuteChanged()
        {
            Factory.Instance.Singleton<IUIThread>(() => new Mock<IUIThread>().Object);
            Mock.Get(Instance.UIThread).Setup(u => u.RunOnUIThread(It.IsAny<Action>())).Callback<Action>(a => a());

            int result = 0;
            bool canExecuteChanged = false;
            DelegateAction<int> action = new DelegateAction<int>((i) => result = i + i, (i) => i == result);
            action.CanExecuteChanged += (sender, e) =>
            {
                canExecuteChanged = true;
            };

            action.RaiseCanExecuteChanged();
            Assert.That(canExecuteChanged, Is.True, "This should have raised the CanExecuteChanged event.");
            Mock.Get(Instance.UIThread).Verify(u => u.RunOnUIThread(It.IsAny<Action>()), Times.Once);
        }

        [Test]
        public static void TestExecute()
        {
            int result = 0;
            DelegateAction<int> action = new DelegateAction<int>((i) => result = i + i, (i) => i == result);

            result = 5;
            action.Execute(5);

            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public static void TestExecuteWhenNotCanExecute()
        {
            int result = 0;
            DelegateAction<int> action = new DelegateAction<int>((i) => result = i + i, (i) => i == result);

            Assert.That(action.CanExecute(0), Is.True);
            Assert.Throws<InvalidOperationException>(() => action.Execute(5));
        }

        [Test]
        public static void TestExecuteWithNoConditionForCanExecute()
        {
            int result = 0;
            DelegateAction<int> action = new DelegateAction<int>((i) => result = i + i);

            Assert.That(action.CanExecute(0), Is.True);
            Assert.That(action.CanExecute(1), Is.True);

            result = 5;
            action.Execute(5);

            Assert.That(result, Is.EqualTo(10));
        }
    }
}
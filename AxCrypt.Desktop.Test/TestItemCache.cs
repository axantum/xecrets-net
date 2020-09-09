using AxCrypt.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Desktop.Test
{
    [TestFixture]
    public class TestItemCache
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
        public static void TestSimpleCaching()
        {
            CacheKey cacheKey = new CacheKey("a key");

            int count = 0;
            ItemCache cache = new ItemCache();
            object o = new object();
            object item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));

            item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));
        }

        [Test]
        public static void TestSubKeyCaching()
        {
            CacheKey cacheKey = new CacheKey("a key").Subkey("a subkey").Subkey("another subkey");

            int count = 0;
            ItemCache cache = new ItemCache();
            object o = new object();
            object item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));

            item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));
        }

        [Test]
        public static void TestUpdateCache()
        {
            CacheKey cacheKey = new CacheKey("a key").Subkey("a subkey").Subkey("another subkey");

            int count = 0;
            ItemCache cache = new ItemCache();
            object o = new object();
            object item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));

            cache.UpdateItem(() => { }, cacheKey);

            item = cache.GetItem(cacheKey, () => { ++count; return new object(); });

            Assert.That(count, Is.EqualTo(2));
            Assert.That(item, Is.Not.EqualTo(o));
        }

        [Test]
        public static void TestRemoveCache()
        {
            CacheKey cacheKey = new CacheKey("a key").Subkey("a subkey").Subkey("another subkey");

            int count = 0;
            ItemCache cache = new ItemCache();
            object o = new object();
            object item = cache.GetItem(cacheKey, () => { ++count; return o; });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(o));

            cache.RemoveItem(cacheKey);

            item = cache.GetItem(cacheKey, () => { ++count; return new object(); });

            Assert.That(count, Is.EqualTo(2));
            Assert.That(item, Is.Not.EqualTo(o));
        }

        [Test]
        public static void TestCacheDependencies()
        {
            CacheKey cacheKey = new CacheKey("a key").Subkey("a subkey");

            int count = 0;
            ItemCache cache = new ItemCache();

            int item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(1));
            Assert.That(count, Is.EqualTo(1));

            int item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(2));

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(1));
            Assert.That(count, Is.EqualTo(2));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(2));

            cache.RemoveItem(cacheKey);

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(3));
            Assert.That(count, Is.EqualTo(3));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(4));
            Assert.That(count, Is.EqualTo(4));

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(3));
            Assert.That(count, Is.EqualTo(4));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(4));
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public static void TestFlushCache()
        {
            CacheKey cacheKey = new CacheKey("a key").Subkey("a subkey");

            int count = 0;
            ItemCache cache = new ItemCache();

            int item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(1));
            Assert.That(count, Is.EqualTo(1));

            int item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(2));

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(1));
            Assert.That(count, Is.EqualTo(2));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(2));

            cache.RemoveItem(CacheKey.RootKey);

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(3));
            Assert.That(count, Is.EqualTo(3));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(4));
            Assert.That(count, Is.EqualTo(4));

            item1 = cache.GetItem(cacheKey.Subkey("1"), () => { return ++count; });
            Assert.That(item1, Is.EqualTo(3));
            Assert.That(count, Is.EqualTo(4));

            item2 = cache.GetItem(cacheKey.Subkey("2"), () => { return ++count; });
            Assert.That(item2, Is.EqualTo(4));
            Assert.That(count, Is.EqualTo(4));
        }
    }
}
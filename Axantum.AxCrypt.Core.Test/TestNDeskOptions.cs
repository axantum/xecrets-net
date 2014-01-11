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

using NDesk.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestNDeskOptions
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
        public static void TestOptionContext()
        {
            OptionSetCollection optionSet = new OptionSetCollection();

            OptionContext oc = new OptionContext(optionSet);

            Assert.That(oc.OptionSet, Is.EqualTo(oc.OptionSet));
            Assert.That(oc.OptionValues.Count(), Is.EqualTo(0));

            oc.OptionIndex = 3;
            Assert.That(oc.OptionIndex, Is.EqualTo(3));

            oc.OptionName = "option";
            Assert.That(oc.OptionName, Is.EqualTo("option"));
        }

        [Test]
        public static void TestSimpleOption()
        {
            bool x = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", var => x = true},
            };

            options.Parse(new string[] { "-y" });
            Assert.That(x, Is.False);

            options.Parse(new string[] { "-x" });
            Assert.That(x, Is.True);
        }

        [Test]
        public static void TestExampleFromDocumentation()
        {
            int verbose = 0;
            List<string> names = new List<string>();
            OptionSetCollection options = new OptionSetCollection()
              .Add("v", v => ++verbose)
              .Add("name=|value=", v => names.Add(v));

            IList<string> extra = options.Parse(new string[] { "-v", "--v", "/v", "-name=A", "/name", "B", "extra" });

            Assert.That(verbose, Is.EqualTo(3));
            Assert.That(names[0], Is.EqualTo("A"));
            Assert.That(names[1], Is.EqualTo("B"));
            Assert.That(extra[0], Is.EqualTo("extra"));
        }

        [Test]
        public static void TestStopProcessingMarker()
        {
            int verbose = 0;
            List<string> names = new List<string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"v", v => ++verbose}
            };

            IList<string> extra = options.Parse(new string[] { "-v", "--v", "/v", "--", "-v" });

            Assert.That(verbose, Is.EqualTo(3));
            Assert.That(extra[0], Is.EqualTo("-v"));
        }

        [Test]
        public static void TestDefaultOptionHandler()
        {
            int verbose = 0;
            List<string> names = new List<string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"v", v => ++verbose},
                {"<>", arg => names.Add(arg)}
            };

            IList<string> extra = options.Parse(new string[] { "-v", "--v", "/v", "-y", "--", "-v" });
            Assert.That(verbose, Is.EqualTo(3));
            Assert.That(names[0], Is.EqualTo("-y"));
            Assert.That(names[1], Is.EqualTo("-v"));
        }

        [Test]
        public static void TestMissingTrailingRequiredValueThrows()
        {
            string value = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", v => value = v}
            };

            Assert.Throws<OptionException>(() => options.Parse(new string[] { "-name" }));
        }

        [Test]
        public static void TestMultipleValuesWithSpecifiedCharSeparator()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=,", (key, value) => dictionary.Add(key, value) },
            };

            options.Parse(new string[] { "-name=A,a", "-name=B,b" });
            Assert.That(dictionary["A"], Is.EqualTo("a"));
            Assert.That(dictionary["B"], Is.EqualTo("b"));
        }

        [Test]
        public static void TestMultipleValuesWithDefaultSeparator()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", (key, value) => dictionary.Add(key, value) },
            };

            options.Parse(new string[] { "-name=A=a", "-name=B:b" });
            Assert.That(dictionary["A"], Is.EqualTo("a"));
            Assert.That(dictionary["B"], Is.EqualTo("b"));
        }

        [Test]
        public static void TestOptionWithValue()
        {
            string v = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"y=", value => v = value},
            };
            options.Parse(new string[] { "-y=s" });

            Assert.That(v, Is.EqualTo("s"));
        }

        [Test]
        public static void TestBundledOptionWithValue()
        {
            bool x = false;
            string v = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
                {"y=", value => v = value},
            };
            options.Parse(new string[] { "-xyoption" });

            Assert.That(x, Is.True);
            Assert.That(v, Is.EqualTo("option"));
        }

        [Test]
        public static void TestBundledOptionWithKeyValuePair()
        {
            bool x = false;
            string k = null;
            string v = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
                {"D=", (key, value) => {k = key; v = value;}},
            };
            options.Parse(new string[] { "-xDname=value" });

            Assert.That(x, Is.True);
            Assert.That(k, Is.EqualTo("name"));
            Assert.That(v, Is.EqualTo("value"));
        }

        [Test]
        public static void TestMultipleValuesWithSpecifiedStringSeparator()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name={=>}", (key, value) => dictionary.Add(key, value) },
            };

            options.Parse(new string[] { "-name=A=>a", "-name=B=>b" });
            Assert.That(dictionary["A"], Is.EqualTo("a"));
            Assert.That(dictionary["B"], Is.EqualTo("b"));
        }

        [Test]
        public static void TestMultipleValuesWithNoSeparator()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name={}", (key, value) => dictionary.Add(key, value) },
            };

            options.Parse(new string[] { "-name=A", "a", "-name=B", "b" });
            Assert.That(dictionary["A"], Is.EqualTo("a"));
            Assert.That(dictionary["B"], Is.EqualTo("b"));
        }

        [Test]
        public static void TestTypedValue()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", (int value) => i = value },
            };
            options.Parse(new string[] { "-name=3" });
            Assert.That(i, Is.EqualTo(3));
        }

        [Test]
        public static void TestSimpleWriteOptionDescriptions()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "Description", (int value) => i = value },
            };
            string description = "      --name=VALUE           Description\r\n";
            using (StringWriter writer = new StringWriter())
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
        }

        [Test]
        public static void TestLongerWriteOptionDescriptions()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "This:\nis a fairly long description, with a comma and a new line to boot! It's also so long it has to be broken into another line.", (int value) => i = value },
            };
            string description = "      --name=VALUE           This:\r\n" +
                                 "                               is a fairly long description, with a comma and a \r\n" +
                                 "                               new line to boot! It's also so long it has to be \r\n" +
                                 "                               broken into another line.\r\n";

            using (StringWriter writer = new StringWriter())
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
        }

        [Test]
        public static void TestWriteOptionDescriptionIgnoresDefaultOptionHandler()
        {
            string d;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"<>", "", s => d = s},
            };

            string expectedDescription = String.Empty;
            using (StringWriter writer = new StringWriter())
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithOptionPrototypeLongerThanLine()
        {
            string d;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"averylongoption|anotherverlongoption|athirdverylongoption|yetanotherverylongoption", "The Description", s => d = s},
            };

            string expectedDescription = "      --averylongoption, --anotherverlongoption, --athirdverylongoption, --yetanotherverylongoption\r\n" +
                                         "                             The Description\r\n";
            using (StringWriter writer = new StringWriter())
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
        }

        [Test]
        public static void TestBadlyFormedTypedValueThrows()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", (int value) => i = value },
            };
            Assert.Throws<OptionException>(() => options.Parse(new string[] { "-name=Three" }));
        }

        [Test]
        public static void TestOptionBaseGetValueSeparators()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"a=-+", (key, value) => dictionary.Add(key, value) },
            };
            OptionBase ob = options["a"];
            string[] separators = ob.GetValueSeparators();
            Assert.That(separators[0], Is.EqualTo("-"));
            Assert.That(separators[1], Is.EqualTo("+"));
        }

        [Test]
        public static void TestOptionBaseGetValueSeparatorsWhenNone()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"a={}", (key, value) => dictionary.Add(key, value) },
            };
            OptionBase ob = options["a"];
            string[] separators = ob.GetValueSeparators();
            Assert.That(separators.Length, Is.EqualTo(0));
        }

        [Test]
        public static void TestOptionBaseGetNames()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x|y", v => ++i},
            };
            OptionBase ob = options["x"];
            string[] names = ob.GetNames();
            Assert.That(names[0], Is.EqualTo("x"));
            Assert.That(names[1], Is.EqualTo("y"));
        }

        [Test]
        public static void TestOptionBaseGetPrototype()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x|y", v => ++i},
            };
            OptionBase ob = options["x"];
            Assert.That(ob.Prototype, Is.EqualTo("x|y"));
        }

        [Test]
        public static void TestOptionBaseToString()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x|y", v => ++i},
            };
            OptionBase ob = options["x"];
            Assert.That(ob.ToString(), Is.EqualTo("x|y"));
        }

        [Test]
        public static void TestOptionBaseGetDescription()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x|y", "X or Y", v => ++i},
            };
            OptionBase ob = options["x"];
            Assert.That(ob.Description, Is.EqualTo("X or Y"));
        }

        [Test]
        public static void TestKeyValuePairsThrowsWhenDelegateOnlyTakesOneArgument()
        {
            string s;
            Assert.Throws<InvalidOperationException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"name=-", v => s = v },
                };
            });
        }

        [Test]
        public static void TestDuplicateOptionNameThrows()
        {
            string s;
            Assert.Throws<ArgumentException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"b|a|a", v => s = v },
                };
            });
        }

        [Test]
        public static void TestEmptyOptionNameThrows()
        {
            string s;
            Assert.Throws<ArgumentException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"", v => s = v },
                };
            });
        }

        [Test]
        public static void TestNullOptionNameThrows()
        {
            string s;
            Assert.Throws<ArgumentNullException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {null, v => s = v },
                };
            });
        }

        [Test]
        public static void TestZeroLengthOptionNameThrows()
        {
            string s;
            Assert.Throws<InvalidOperationException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a|", v => s = v },
                };
            });
        }

        [Test]
        public static void TestDoubleKeyValuePairStartThrows()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a={{=>}", (key, value) => dictionary.Add(key, value) },
                };
            });
        }

        [Test]
        public static void TestTooManyOptionValuesThrows()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            OptionSetCollection options = new OptionSetCollection()
            {
                {"a={=>}", (key, value) => dictionary.Add(key, value) },
            };
            Assert.Throws<OptionException>(() => options.Parse(new string[] { "-a", "b=>c=>d" }));
        }

        [Test]
        public static void TestOnlyEndKeyValuePairStartThrows()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a=>}", (key, value) => dictionary.Add(key, value) },
                };
            });
        }

        [Test]
        public static void TestMissingEndKeyValuePairStartThrows()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a={=>", (key, value) => dictionary.Add(key, value) },
                };
            });
        }

        [Test]
        public static void TestConflictingOptionTypesThrows()
        {
            string s;
            Assert.Throws<InvalidOperationException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a:|b=", v => s = v },
                };
            });
        }

        [Test]
        public static void TestNullActionWithDescriptionAndOneArgumentThrows()
        {
            Action<string> nullAction = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a=", "Description", nullAction}
                };
            });
        }

        [Test]
        public static void TestNullActionWithDescriptionAndOneStronglyTypedArgumentThrows()
        {
            Action<int> nullAction = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a=", "Description", nullAction}
                };
            });
        }

        [Test]
        public static void TestNullActionWithDescriptionAndTwoArgumentsThrows()
        {
            OptionAction<string, string> nullAction = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                OptionSetCollection options = new OptionSetCollection()
                {
                    {"a={}", "Description", nullAction}
                };
            });
        }

        [Test]
        public static void TestParsingNullElementThrows()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", var => ++i},
            };

            Assert.Throws<ArgumentNullException>(() => options.Parse(new string[] { "-x", null }));
        }

        [Test]
        public static void TestForcedEnabledOption()
        {
            bool x = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
            };

            options.Parse(new string[] { "-x+" });
            Assert.That(x, Is.True);
        }

        [Test]
        public static void TestForcedDisabledOption()
        {
            bool x = true;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
            };

            options.Parse(new string[] { "-x-" });
            Assert.That(x, Is.False);
        }

        [Test]
        public static void TestBundledOption()
        {
            bool x = false;
            bool y = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
                {"y", value => y = value != null},
            };

            options.Parse(new string[] { "-xy" });
            Assert.That(x, Is.True);
            Assert.That(y, Is.True);
        }

        [Test]
        public static void TestBundledOptionWithNonMinusFlagWhichApparentlyIsNotSupported()
        {
            bool x = false;
            bool y = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
                {"y", value => y = value != null},
            };

            IList<string> extra = options.Parse(new string[] { "/xy" });
            Assert.That(extra[0], Is.EqualTo("/xy"));
        }

        [Test]
        public static void TestBundledOptionWithUnknownOptionThrows()
        {
            bool x = false;
            bool y = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", value => x = value != null},
                {"y", value => y = value != null},
            };
            Assert.Throws<OptionException>(() => options.Parse(new string[] { "-xyz" }));
        }

        [Test]
        public static void TestAddNullOptionToCollectionThrows()
        {
            OptionSetCollection options = new OptionSetCollection();
            OptionBase nullOb = null;
            Assert.Throws<ArgumentNullException>(() => options.Add(nullOb));
        }

        private class TestOptionsForConstructorExceptions : OptionBase
        {
            public TestOptionsForConstructorExceptions(string prototype, string description, int maxValueCount)
                : base(prototype, description, maxValueCount)
            {
            }

            protected override void OnParseComplete(OptionContext optionContext)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public static void TestOptionBaseConstructorExceptions()
        {
            string nullString = null;

            Assert.Throws<ArgumentNullException>(() => new TestOptionsForConstructorExceptions(nullString, "Description", 0));
            Assert.Throws<ArgumentException>(() => new TestOptionsForConstructorExceptions("", "Description", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TestOptionsForConstructorExceptions("prototype", "Description", -1));

            Assert.Throws<ArgumentException>(() => new TestOptionsForConstructorExceptions("x=", "Description", 0));
            Assert.Throws<ArgumentException>(() => new TestOptionsForConstructorExceptions("x:", "Description", 0));
            Assert.DoesNotThrow(() => new TestOptionsForConstructorExceptions("x:", "Description", 1));

            Assert.Throws<ArgumentException>(() => new TestOptionsForConstructorExceptions("x", "Description", 2));
            Assert.Throws<ArgumentException>(() => new TestOptionsForConstructorExceptions("x=|<>", "Description", 2));
        }
    }
}
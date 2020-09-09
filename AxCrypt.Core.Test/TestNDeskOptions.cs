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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using NDesk.Options;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace AxCrypt.Core.Test
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
            Assert.That(extra.Count, Is.EqualTo(0));
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
            Assert.That(value, Is.Null);
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
        public static void TestStronglyTypedKeyValuePair()
        {
            int k = 0, v = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"D=", (int key, int value) => {k = key; v = value;}},
            };
            options.Parse(new string[] { "-D1=2" });
            Assert.That(k, Is.EqualTo(1));
            Assert.That(v, Is.EqualTo(2));
        }

        [Test]
        public static void TestStronglyTypedKeyValuePairWithNullActionThrows()
        {
            OptionAction<int, int> action = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                new OptionSetCollection()
                {
                    {"D=", action},
                };
            });
        }

        [Test]
        public static void TestRedefineOption()
        {
            bool first = false;
            bool second = false;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"a|b", v => first = true },
            };
            OptionSetCollection options2 = new OptionSetCollection()
            {
                {"a|b", v => second = true }
            };

            options[0] = options2[0];

            options.Parse(new string[] { "-b" });
            Assert.That(first, Is.False);
            Assert.That(second, Is.True);
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
            string description = "      --name=VALUE           Description" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithTwoValues()
        {
            string k = null;
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "Description", (string key, int value) => {i = value; k=key;} },
            };
            string description = "      --name=VALUE1:VALUE2   Description" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(k, Is.Null);
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithInsertedValueNamesForSingleArgument()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "Description {Key} and {1:Value}", (int value) => {i = value;} },
            };
            string description = "      --name=Key             Description Key and Value" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithInsertedValueNamesIgnoredBecauseOfUnterminatedPlaceholderMarkerAndDescriptionBug()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "Description {Key and value", (int value) => {i = value;} },
            };
            string description = "      --name=VALUE           Description " + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithInsertedValueNamesForMultipleValueArgument()
        {
            string k = null;
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "Description {Key} and {1:Value}", (string key, int value) => {i = value; k=key;} },
            };
            string description = "      --name=VALUE1:Value    Description Key and Value" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(k, Is.Null);
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithLeftCurlyBraceEscape()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "A {{ Description", (int value) => i = value },
            };
            string description = "      --name=VALUE           A { Description" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithRightCurlyBraceEscape()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "A }} Description", (int value) => i = value },
            };
            string description = "      --name=VALUE           A } Description" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithBadRightCurlyBraceEscape()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "A }x Description", (int value) => i = value },
            };
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.Throws<InvalidOperationException>(() => options.WriteOptionDescriptions(writer));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithUnterminatedRightCurlyBraceEscape()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "A Description }", (int value) => i = value },
            };
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.Throws<InvalidOperationException>(() => options.WriteOptionDescriptions(writer));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestLongerWriteOptionDescriptions()
        {
            int i = 0;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"name=", "This:\nis a fairly long description, with a comma and a new line to boot! It's also so long it has to be broken into another line.", (int value) => i = value },
            };
            string description = "      --name=VALUE           This:" + Environment.NewLine +
                "                               is a fairly long description, with a comma and a " + Environment.NewLine +
                "                               new line to boot! It's also so long it has to be " + Environment.NewLine +
                "                               broken into another line." + Environment.NewLine;

            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(description));
            }
            Assert.That(i, Is.EqualTo(0));
        }

        [Test]
        public static void TestWriteOptionDescriptionIgnoresDefaultOptionHandler()
        {
            string d = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"<>", "", s => d = s},
            };

            string expectedDescription = String.Empty;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
            Assert.That(d, Is.Null);
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithOptionPrototypeLongerThanLine()
        {
            string d = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"averylongoption|anotherverlongoption|athirdverylongoption|yetanotherverylongoption", "The Description", s => d = s},
            };

            string expectedDescription = "      --averylongoption, --anotherverlongoption, --athirdverylongoption, --yetanotherverylongoption" + Environment.NewLine +
                                         "                             The Description" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
            Assert.That(d, Is.Null);
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithValueArguments()
        {
            string value = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"a=", s => value = s},
                {"b:|c:", s => value = s},
                {"d={=>}", (s, v) => value = s + v},
                {"e={}", (s, v) => value = s + v},
                {"f", s => value = s},
            };

            string expectedDescription =
                "  -a=VALUE                   " + Environment.NewLine +
                "  -b, -c[=VALUE]             " + Environment.NewLine +
                "  -d=VALUE1=>VALUE2          " + Environment.NewLine +
                "  -e=VALUE1 VALUE2           " + Environment.NewLine +
                "  -f                         " + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
            Assert.That(value, Is.Null);
        }

        [Test]
        public static void TestWriteOptionDescriptionsWithDescriptionThatMustBeBroken()
        {
            string d = null;
            OptionSetCollection options = new OptionSetCollection()
            {
                {"x", "The_desciption_is_too_long_to_fit_on_a_single_line_so_the_code-must_break_it_up", s => d = s},
            };

            string expectedDescription = "  -x                         The_desciption_is_too_long_to_fit_on_a_single_li-" + Environment.NewLine +
                                         "                               ne_so_the_code-must_break_it_up" + Environment.NewLine;
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                options.WriteOptionDescriptions(writer);
                string s = writer.ToString();
                Assert.That(s, Is.EqualTo(expectedDescription));
            }
            Assert.That(d, Is.Null);
        }

        [Test]
        public static void TestCannotUseTwoValuesWithSimpleArgumentThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"f", (s, v) => {}},
                };
            });
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
            Assert.That(i, Is.EqualTo(0));
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
            Assert.Throws<InvalidOperationException>(() =>
            {
                new OptionSetCollection()
                {
                    {"name=-", v => {} },
                };
            });
        }

        [Test]
        public static void TestDuplicateOptionNameThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"b|a|a", v => {} },
                };
            });
        }

        [Test]
        public static void TestEmptyOptionNameThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"", v => {} },
                };
            });
        }

        [Test]
        public static void TestNullOptionNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new OptionSetCollection()
                {
                    {null, v => {} },
                };
            });
        }

        [Test]
        public static void TestZeroLengthOptionNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new OptionSetCollection()
                {
                    {"a|", v => {} },
                };
            });
        }

        [Test]
        public static void TestDoubleKeyValuePairStartThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"a={{=>}", (key, value) => {} },
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
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"a=>}", (key, value) => {} },
                };
            });
        }

        [Test]
        public static void TestMissingEndKeyValuePairStartThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new OptionSetCollection()
                {
                    {"a={=>", (key, value) => {} },
                };
            });
        }

        [Test]
        public static void TestConflictingOptionTypesThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new OptionSetCollection()
                {
                    {"a:|b=", v => {} },
                };
            });
        }

        [Test]
        public static void TestNullActionWithDescriptionAndOneArgumentThrows()
        {
            Action<string> nullAction = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                new OptionSetCollection()
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
                new OptionSetCollection()
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
                new OptionSetCollection()
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
        public static void TestBundledOptionWithNonMinusSwitchWhichApparentlyIsNotSupported()
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
            Assert.That(x, Is.False);
            Assert.That(y, Is.False);
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
            Assert.That(x, Is.True);
            Assert.That(y, Is.True);
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

            OptionBase option = null;
            Assert.Throws<ArgumentNullException>(() => option = new TestOptionsForConstructorExceptions(nullString, "Description", 0));
            Assert.Throws<ArgumentException>(() => option = new TestOptionsForConstructorExceptions("", "Description", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => option = new TestOptionsForConstructorExceptions("prototype", "Description", -1));

            Assert.Throws<ArgumentException>(() => option = new TestOptionsForConstructorExceptions("x=", "Description", 0));
            Assert.Throws<ArgumentException>(() => option = new TestOptionsForConstructorExceptions("x:", "Description", 0));
            Assert.DoesNotThrow(() => option = new TestOptionsForConstructorExceptions("x:", "Description", 1));

            Assert.Throws<ArgumentException>(() => option = new TestOptionsForConstructorExceptions("x", "Description", 2));
            Assert.Throws<ArgumentException>(() => option = new TestOptionsForConstructorExceptions("x=|<>", "Description", 2));

            Assert.That(option, Is.Not.Null, "Should not reach here. Only here to make FxCop believe we're using the instantiated object.");
        }

        [Test]
        public static void TestOptionException()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            OptionException oe = new OptionException();

            Assert.That(oe.Message, Is.EqualTo("Exception of type 'NDesk.Options.OptionException' was thrown."));
            Assert.That(oe.OptionName, Is.Null);
            Assert.That(oe.InnerException, Is.Null);

            oe = new OptionException("My Message");

            Assert.That(oe.Message, Is.EqualTo("My Message"));
            Assert.That(oe.OptionName, Is.Null);
            Assert.That(oe.InnerException, Is.Null);

            oe = new OptionException("My Message", oe);

            Assert.That(oe.Message, Is.EqualTo("My Message"));
            Assert.That(oe.OptionName, Is.Null);
            Assert.That(oe.InnerException, Is.Not.Null);
        }

        private static OptionContext CreateOptionContext()
        {
            OptionSetCollection optionSet = new OptionSetCollection()
            {
                { "x=", (k, v) => {}},
            };
            OptionContext oc = new OptionContext(optionSet);
            return oc;
        }

        private static OptionValueCollection CreateOptionValueCollection()
        {
            OptionContext oc = CreateOptionContext();
            oc.Option = oc.OptionSet[0];

            OptionValueCollection ovc = oc.OptionValues;

            return ovc;
        }

        [Test]
        public static void TestOptionValueCollectionAssertValid()
        {
            OptionContext oc = CreateOptionContext();
            string s;

            Assert.Throws<InvalidOperationException>(() => s = oc.OptionValues[0]);

            oc.Option = oc.OptionSet[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => s = oc.OptionValues[99]);
        }

        [Test]
        public static void TestOptionValueCollectionItemSetter()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc[0] = "value";

            Assert.That(ovc[0], Is.EqualTo("value"));
        }

        [Test]
        public static void TestOptionValueCollectionIListImplementation()
        {
            IList ilist = CreateOptionValueCollection() as IList;

            Assert.That(ilist.IsReadOnly, Is.False);
            Assert.That(ilist.IsFixedSize, Is.False);
            Assert.That(ilist.IsSynchronized, Is.False);
            Assert.That(ilist.SyncRoot, Is.Not.Null);

            ilist.Add("first");
            Assert.That(ilist[0], Is.EqualTo("first"));

            ilist[0] = "second";
            Assert.That(ilist[0], Is.EqualTo("second"));

            ilist.Add("third");
            Assert.That(ilist.ToString(), Is.EqualTo("second, third"));

            Assert.That(ilist.IndexOf("third"), Is.EqualTo(1));
            Assert.That(ilist.Contains("second"));
            Assert.That(ilist.Contains("first"), Is.False);

            string v = String.Empty;
            foreach (string s in ilist)
            {
                v += s;
            }
            Assert.That(v, Is.EqualTo("secondthird"));

            ilist.RemoveAt(0);
            Assert.That(ilist.Count, Is.EqualTo(1));
            Assert.That(ilist[0], Is.EqualTo("third"));

            ilist.Insert(1, "forth");
            Assert.That(ilist.Count, Is.EqualTo(2));
            Assert.That(ilist[0], Is.EqualTo("third"));
            Assert.That(ilist[1], Is.EqualTo("forth"));

            ilist.Remove("third");
            Assert.That(ilist.Count, Is.EqualTo(1));
            Assert.That(ilist[0], Is.EqualTo("forth"));
        }

        [Test]
        public static void TestOptionValueCollectionGetEnumerator()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            string v = String.Empty;
            foreach (string s in ovc)
            {
                v += s;
            }
            Assert.That(v, Is.EqualTo("firstsecond"));
        }

        [Test]
        public static void TestOptionValueCollectionRemove()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            ovc.Remove("second");

            Assert.That(ovc.Count, Is.EqualTo(1));
            Assert.That(ovc[0], Is.EqualTo("first"));
        }

        [Test]
        public static void TestOptionValueCollectionContains()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            Assert.That(ovc.Contains("first"));
            Assert.That(ovc.Contains("first"));
            Assert.That(ovc.Contains("third"), Is.False);
        }

        [Test]
        public static void TestOptionValueCollectionCopyTo()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            string[] array = new string[] { "one", "two", "three" };
            ovc.CopyTo(array, 1);

            Assert.That(array.Length, Is.EqualTo(3));
            Assert.That(array[0], Is.EqualTo("one"));
            Assert.That(array[1], Is.EqualTo("first"));
            Assert.That(array[2], Is.EqualTo("second"));
        }

        [Test]
        public static void TestOptionValueCollectionICollectionCopyTo()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            ICollection collection = ovc as ICollection;

            string[] array = new string[] { "one", "two", "three" };
            collection.CopyTo(array, 1);

            Assert.That(array.Length, Is.EqualTo(3));
            Assert.That(array[0], Is.EqualTo("one"));
            Assert.That(array[1], Is.EqualTo("first"));
            Assert.That(array[2], Is.EqualTo("second"));
        }

        [Test]
        public static void TestOptionValueCollectionToArray()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            string[] array = ovc.ToArray();

            Assert.That(array.Length, Is.EqualTo(2));
            Assert.That(array[0], Is.EqualTo("first"));
            Assert.That(array[1], Is.EqualTo("second"));
        }

        [Test]
        public static void TestOptionValueCollectionToList()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            List<string> list = ovc.ToList();

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo("first"));
            Assert.That(list[1], Is.EqualTo("second"));
        }

        [Test]
        public static void TestOptionValueCollectionRemoveAt()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("second");

            ovc.RemoveAt(0);

            Assert.That(ovc.Count, Is.EqualTo(1));
            Assert.That(ovc[0], Is.EqualTo("second"));
        }

        [Test]
        public static void TestOptionValueCollectionInsert()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Insert(0, "half");

            Assert.That(ovc.Count, Is.EqualTo(2));
            Assert.That(ovc[0], Is.EqualTo("half"));
            Assert.That(ovc[1], Is.EqualTo("first"));
        }

        [Test]
        public static void TestOptionValueCollectionIndexOf()
        {
            OptionValueCollection ovc = CreateOptionValueCollection();

            ovc.Add("first");
            ovc.Add("Second");

            Assert.That(ovc.IndexOf("Second"), Is.EqualTo(1));
        }
    }
}
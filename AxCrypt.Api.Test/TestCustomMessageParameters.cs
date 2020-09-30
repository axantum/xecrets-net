using System;
using System.Globalization;

using AxCrypt.Abstractions;
using AxCrypt.Api.Implementation;
using AxCrypt.Api.Model;

using NUnit.Framework;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api.Test
{
    [TestFixture]
    public class TestCustomMessageParameters
    {
        [SetUp]
        public void Setup()
        {
            TypeMap.Register.Singleton<IStringSerializer>(() => new StringSerializer());
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestSerializeWithNullCulture()
        {
            IStringSerializer serializer = New<IStringSerializer>();

            CustomMessageParameters parameters;
            string json;

            parameters = new CustomMessageParameters(null, "A message");
            json = serializer.Serialize(parameters);

            Assert.That(json, Is.EqualTo("{\r\n  \"messageCulture\": null,\r\n  \"customMessage\": \"A message\"\r\n}".Replace("\r\n", Environment.NewLine)));
        }

        [Test]
        public void TestSerializeWithEnglishUsCulture()
        {
            IStringSerializer serializer = New<IStringSerializer>();

            CustomMessageParameters parameters;
            string json;

            parameters = new CustomMessageParameters(CultureInfo.CreateSpecificCulture("en-US"), "A message");
            json = serializer.Serialize(parameters);

            Assert.That(json, Is.EqualTo("{\r\n  \"messageCulture\": \"en-US\",\r\n  \"customMessage\": \"A message\"\r\n}".Replace("\r\n", Environment.NewLine)));
        }

        [Test]
        public void TestCrossFrameworkPclCultureInfoSerialization()
        {
            IStringSerializer serializer = New<IStringSerializer>();

            CustomMessageParameters parameters;
            string json;

            parameters = new CustomMessageParameters(CultureInfo.CreateSpecificCulture("en-US"), "A message");
            json = serializer.Serialize(parameters.MessageCulture);

            Assert.That(json, Is.EqualTo("\"en-US\""));
        }

        [Test]
        public void TestDeserializeWithNull()
        {
            IStringSerializer serializer = New<IStringSerializer>();
            string json = "{\r\n  \"messageCulture\": null,\r\n  \"customMessage\": \"A message\"\r\n}".Replace("\r\n", Environment.NewLine);

            CustomMessageParameters parameters = serializer.Deserialize<CustomMessageParameters>(json);
            Assert.That(parameters.MessageCulture, Is.Null);
            Assert.That(parameters.CustomMessage, Is.EqualTo("A message"));
        }

        [Test]
        public void TestDeserializeWithEnglishUsCulture()
        {
            IStringSerializer serializer = New<IStringSerializer>();
            string json = "{\r\n  \"messageCulture\": \"en-US\",\r\n  \"customMessage\": \"A message\"\r\n}".Replace("\r\n", Environment.NewLine);

            CustomMessageParameters parameters = serializer.Deserialize<CustomMessageParameters>(json);
            Assert.That(parameters.MessageCulture.ToString(), Is.EqualTo("en-US"));
            Assert.That(parameters.CustomMessage, Is.EqualTo("A message"));
        }
    }
}
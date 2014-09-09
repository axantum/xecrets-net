using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class EmailAddressJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            EmailAddress mailAddress = new EmailAddress((string)reader.Value);
            return mailAddress;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EmailAddress);
        }
    }
}
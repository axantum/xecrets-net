using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagApiModel : BaseApiModel
    {
        public static UTMTagApiModel Empty = new UTMTagApiModel(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, null);

        public UTMTagApiModel()
        {
        }

        public UTMTagApiModel(Guid id, string utmparameter, string source, string medium, string campaign, string content, string discount, string comment, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc)
        {
            Id = id;
            SubsLevel = utmparameter;
            Source = source;
            Medium = medium;
            Campaign = campaign;
            Content = content;
            Discount = discount;
            Comment = comment;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("subslevel")]
        public string SubsLevel { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("campaign")]
        public string Campaign { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("discount")]
        public string Discount { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

}

using System.Text.Json.Serialization;
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

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("subslevel")]
        public string? SubsLevel { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("medium")]
        public string? Medium { get; set; }

        [JsonPropertyName("campaign")]
        public string? Campaign { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("discount")]
        public string? Discount { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }

}

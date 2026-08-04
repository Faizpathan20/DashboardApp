using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DashboardApp.Models
{
    /// <summary>
    /// Represents a single dashboard row coming from the external API.
    /// Uses flexible converters because the source API is inconsistent
    /// about sending numbers as numbers vs. numbers as strings.
    /// </summary>
    public class DashboardRecord
    {
        [JsonPropertyName("docdate")]
        [JsonConverter(typeof(FlexibleDateConverter))]
        public DateTime? DocDate { get; set; }

        [JsonPropertyName("docno")]
        public string DocNo { get; set; } = string.Empty;

        [JsonPropertyName("clientname")]
        public string ClientName { get; set; } = string.Empty;

        [JsonPropertyName("qty")]
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int Qty { get; set; }

        [JsonPropertyName("art_id")]
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int ArtId { get; set; }

        [JsonPropertyName("artname")]
        public string ArtName { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;

        [JsonPropertyName("colour")]
        public string Colour { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("mrp")]
        [JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal Mrp { get; set; }

        // Handy for display in views: falls back gracefully if DocDate is null
        [JsonIgnore]
        public string DocDateDisplay => DocDate?.ToString("dd/MM/yyyy") ?? "";
    }

    /// <summary>Root wrapper matching your API's { "data": [...] } shape.</summary>
    public class DashboardApiResponse
    {
        [JsonPropertyName("data")]
        public List<DashboardRecord> Data { get; set; } = new();
    }

    // ---------- Flexible converters ----------
    // These let the same property accept "36" (string) or 36 (number) from the API.

    public class FlexibleIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out var i))
                    return i;

                // Handles numbers written with a decimal point, e.g. 1.0, 24.0
                if (reader.TryGetDouble(out var d))
                    return (int)Math.Round(d);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDouble))
                    return (int)Math.Round(parsedDouble);
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);
    }

    public class FlexibleDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var d))
                return d;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;
            }

            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);
    }

    public class FlexibleDateConverter : JsonConverter<DateTime?>
    {
        // Your API sends dates as "dd/MM/yyyy"
        private static readonly string[] Formats =
        {
            "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy"
        };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (DateTime.TryParseExact(s, Formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                return date;

            // last resort, let .NET try to guess
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
                return fallback;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("dd/MM/yyyy"));
            else
                writer.WriteNullValue();
        }
    }
}

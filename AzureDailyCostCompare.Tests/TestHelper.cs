
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureDailyCostCompare.Tests;

public static class TestHelper
{
    public static List<DailyCostData> LoadMockCostData(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new DateOnlyJsonConverter() }  // Convert DateOnly
        };

        var data = JsonSerializer.Deserialize<MockCostDataContainer>(json, options);
        return data?.MockCostData ?? [];
    }
}

// Custom converter for DateOnly
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

// Ensure the JSON structure maps correctly
public class MockCostDataContainer
{
    public required List<DailyCostData> MockCostData { get; set; }
}

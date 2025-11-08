using System.Text.Json;
using System.Text.Json.Serialization;

namespace BingoSyncIntegration;

public class StringBoolJSONConverter : JsonConverter<bool>
{
	public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString();
		return str == "true";
	}

	public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

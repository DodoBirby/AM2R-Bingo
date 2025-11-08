using System.Text.Json.Serialization;

namespace BingoSyncIntegration;

public class AM2RDataJSON
{
	[JsonConverter(typeof(StringBoolJSONConverter))]
	public bool InGame { get; set; }

	public Dictionary<string, double>? Metroids { get; set; }

	public Dictionary<string, double>? Items { get; set; }

	public Dictionary<string, double>? Events { get; set; }

	public Dictionary<string, double>? MapTiles { get; set; }
}

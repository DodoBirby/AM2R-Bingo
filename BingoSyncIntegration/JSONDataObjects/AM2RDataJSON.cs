using System.Text.Json.Serialization;

namespace BingoSyncIntegration.JSONDataObjects;

public class AM2RDataJSON
{
	[JsonConverter(typeof(StringBoolJSONConverter))]
	public bool InGame { get; set; }

	public Dictionary<string, double> Metroids { get; set; } = [];

	public Dictionary<string, double> Items { get; set; } = [];

	public Dictionary<string, double> Events { get; set; } = [];

	public Dictionary<string, double> MapTiles { get; set; } = [];

	public Dictionary<string, double> TrooperLogs { get; set; } = [];

	public Dictionary<string, double> Logs { get; set; } = [];

	public Dictionary<string, double> ItemLocations { get; set; } = [];
}

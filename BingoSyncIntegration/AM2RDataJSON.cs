namespace BingoSyncIntegration;

public class AM2RDataJSON
{
	public required Dictionary<string, int> Metroids { get; set; }

	public required Dictionary<string, int> Items { get; set; }

	public required Dictionary<string, int> Events { get; set; }

	public required Dictionary<string, int> MapTiles { get; set; }
}

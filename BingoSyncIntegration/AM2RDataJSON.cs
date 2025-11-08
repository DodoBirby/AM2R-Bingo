namespace BingoSyncIntegration;

public class AM2RDataJSON
{
	public bool InGame { get; set; }

	public required Dictionary<string, double> Metroids { get; set; }

	public required Dictionary<string, double> Items { get; set; }

	public required Dictionary<string, double> Events { get; set; }

	public required Dictionary<string, double> MapTiles { get; set; }
}

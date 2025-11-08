namespace BingoSyncIntegration;

public class ObjectiveJSON
{
	public required string Name { get; set; }

	public List<int> ItemIds { get; set; } = [];

	public int ItemCount { get; set; }

	public List<List<int>> MapTileCoords { get; set; } = [];

	public int MapTileCount { get; set; }

	public List<int> MetroidIds { get; set; } = [];

	public int MetroidCount { get; set; }

	public List<int> EventIds { get; set; } = [];

	public int EventCount { get; set; }
}

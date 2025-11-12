using System.Text.Json.Serialization;

namespace BingoSyncIntegration.JSONDataObjects;

public class ObjectiveJSON
{
	public required string Name { get; set; }

	public List<int> ItemIds { get; set; } = [];

	public int ItemCount { get; set; }

	[JsonIgnore]
	public int ItemsRequired
	{
		get
		{
			return ItemCount != 0 ? ItemCount : ItemIds.Count;
		}
	}

	public List<List<int>> MapTileCoords { get; set; } = [];

	public int MapTileCount { get; set; }

	[JsonIgnore]
	public int MapTilesRequired
	{
		get
		{
			return MapTileCount != 0 ? MapTileCount : MapTileCoords.Count;
		}
	}

	public List<int> MetroidIds { get; set; } = [];

	public int MetroidCount { get; set; }

	[JsonIgnore]
	public int MetroidsRequired
	{
		get
		{
			return MetroidCount != 0 ? MetroidCount : MetroidIds.Count;
		}
	}

	public List<int> EventIds { get; set; } = [];

	public int EventCount { get; set; }

	[JsonIgnore]
	public int EventsRequired
	{
		get
		{
			return EventCount != 0 ? EventCount : EventIds.Count;
		}
	}

	public List<int> TrooperLogIds { get; set; } = [];

	public int TrooperLogCount { get; set; }

	[JsonIgnore]
	public int TrooperLogsRequired
	{
		get
		{
			return TrooperLogCount != 0 ? TrooperLogCount : TrooperLogIds.Count;
		}
	}

	public List<int> LogIds { get; set; } = [];

	public int LogCount { get; set; }

	[JsonIgnore]
	public int LogsRequired
	{
		get
		{
			return LogCount != 0 ? LogCount : LogIds.Count;
		}
	}
}

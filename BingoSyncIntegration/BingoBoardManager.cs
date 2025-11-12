using BingoSyncIntegration.JSONDataObjects;

namespace BingoSyncIntegration;

public class BingoBoardManager(BingoSyncClient client)
{
	List<BingoSyncBoardJSON> boardData = [];

	public async Task RefreshDataAsync()
	{
		boardData = await client.GetBoardDataAsync();
	}

	public List<string> GetUnmarkedObjectiveNames()
	{
		return boardData.Where(x => x.Colors == "blank").Select(x => x.Name).ToList();
	}

	public Dictionary<string, int> GetBingoSyncObjectives()
	{
		return boardData.Select((x, i) => (x.Name, i + 1)).ToDictionary();
	}
}

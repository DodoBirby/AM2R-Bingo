using BingoSyncIntegration.JSONDataObjects;
using BingoSyncIntegration.NetworkClients;
using System.Text.Json;

namespace BingoSyncIntegration;

class Program
{
	const int SecondsBetweenBoardFetches = 5;
	const int MillisecondsBetweenAM2RFetches = 500;
	const double SecondsBetweenAM2RFetches = MillisecondsBetweenAM2RFetches / 1000.0;
	const int TimesBeforeBoardFetch = (int)(SecondsBetweenBoardFetches / SecondsBetweenAM2RFetches);

	static HashSet<string> PreviouslyCompletedObjectiveNames = [];
	static Dictionary<string, int> BingoSyncObjectiveNameMap = [];
	static List<ObjectiveJSON> CurrentObjectives = [];
	static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	static async Task Main()
	{
		Console.WriteLine("Enter BingoSync room id:");
		var roomId = Console.ReadLine()!;
		Console.WriteLine("Enter BingoSync password:");
		var password = Console.ReadLine()!;

		Console.WriteLine("Enter color:");
		var color = Console.ReadLine()!;

		var am2rClient = new AM2RClient();
		await ConnectToAm2rWithRetry(am2rClient);
		Console.WriteLine("Connected to AM2R");

		var bingoSyncClient = new BingoSyncClient();
		bingoSyncClient.Color = color;
		await bingoSyncClient.ConnectAsync(roomId, password);
		Console.WriteLine("Connected to BingoSync");

		var boardManager = new BingoBoardManager(bingoSyncClient);

		await boardManager.RefreshDataAsync();
		RefreshObjectives(boardManager.GetBingoSyncObjectives());


		while (true)
		{
			for (var i = 0; i < TimesBeforeBoardFetch; i++)
			{
				var data = await am2rClient.GetDataAsync();
				if (data is null)
				{
					Console.WriteLine("Lost connection to am2r, attempting to reconnect");
					await ConnectToAm2rWithRetry(am2rClient);
					continue;
				}
				if (data.InGame)
				{
					var currentlyCompletedObjectives = GetCompletedObjectives(data, CurrentObjectives);
					var newlyCompletedObjectives = currentlyCompletedObjectives.Where(x => !PreviouslyCompletedObjectiveNames.Contains(x.Name)).Select(x => x.Name).ToList();

					var currentlyCompletedObjectiveNames = currentlyCompletedObjectives.Select(x => x.Name).ToHashSet();
					var lostObjectives = PreviouslyCompletedObjectiveNames.Where(x => !currentlyCompletedObjectiveNames.Contains(x)).ToList();

					PreviouslyCompletedObjectiveNames = currentlyCompletedObjectiveNames;

					await bingoSyncClient.UnsendObjectivesAsync(lostObjectives, BingoSyncObjectiveNameMap);
					await bingoSyncClient.SendObjectivesAsync(newlyCompletedObjectives, BingoSyncObjectiveNameMap);
				}
				
				await Task.Delay(MillisecondsBetweenAM2RFetches);
			}
			await boardManager.RefreshDataAsync();
			var newObjectives = boardManager.GetBingoSyncObjectives();
			if (ObjectivesRequireRefresh(newObjectives))
			{
				Console.WriteLine("New bingo card detected, resetting objectives");
				RefreshObjectives(newObjectives);
				continue;
			}

			var unmarkedObjectives = boardManager.GetUnmarkedObjectiveNames();
			var objectivesToSend = unmarkedObjectives.Where(x => PreviouslyCompletedObjectiveNames.Contains(x)).ToList();
			await bingoSyncClient.SendObjectivesAsync(objectivesToSend, BingoSyncObjectiveNameMap);
		}
	}

	static void RefreshObjectives(Dictionary<string, int> newObjectives)
	{
		BingoSyncObjectiveNameMap = newObjectives;
		CurrentObjectives = GetAllObjectives().Where(x => BingoSyncObjectiveNameMap.ContainsKey(x.Name)).ToList();
		PreviouslyCompletedObjectiveNames = [];
	}

	static bool ObjectivesRequireRefresh(Dictionary<string, int> newObjectives)
	{
		var cardRerolled = newObjectives.Any(x => !BingoSyncObjectiveNameMap.ContainsKey(x.Key));
		return cardRerolled;
	}

	static async Task ConnectToAm2rWithRetry(AM2RClient client)
	{
		while (!await client.ConnectAsync())
		{
			Console.WriteLine("Failed to connect to am2r, retrying in 1s");
			await Task.Delay(1000);
		}
	}

	static HashSet<ObjectiveJSON> GetCompletedObjectives(AM2RDataJSON am2rData, List<ObjectiveJSON> objectives)
	{
		var result = new HashSet<ObjectiveJSON>();
		foreach (var objective in objectives)
		{
			if (IsObjectiveComplete(am2rData, objective))
			{
				result.Add(objective);
			}
		}
		return result;
	}

	static bool IsObjectiveComplete(AM2RDataJSON am2rData, ObjectiveJSON objective)
	{
		var completedItems = objective.ItemIds.Where(x => am2rData.Items.ContainsKey(x.ToString())).Count();
		var completedMetroids = objective.MetroidIds.Where(x => am2rData.Metroids.ContainsKey(x.ToString())).Count();
		var completedEvents = objective.EventIds.Where(x => am2rData.Events.ContainsKey(x.ToString())).Count();
		var completedMapTiles = objective.MapTileCoords.Where(x => am2rData.MapTiles.ContainsKey($"{x[0]}:{x[1]}")).Count();
		var completedTrooperLogs = objective.TrooperLogIds.Where(x => am2rData.TrooperLogs.ContainsKey(x.ToString())).Count();
		var completedLogs = objective.LogIds.Where(x => am2rData.Logs.ContainsKey(x.ToString())).Count();

		return completedItems >= objective.ItemsRequired
			&& completedMetroids >= objective.MetroidsRequired
			&& completedEvents >= objective.EventsRequired
			&& completedMapTiles >= objective.MapTilesRequired
			&& completedTrooperLogs >= objective.TrooperLogsRequired
			&& completedLogs >= objective.LogsRequired;
	}

	static List<ObjectiveJSON> GetAllObjectives()
	{
		var objectivesJson = File.ReadAllText("objectives.json");

		var objectivesList = JsonSerializer.Deserialize<List<ObjectiveJSON>>(objectivesJson, jsonSerializerOptions)!;
		return objectivesList;
	}
}
using System.Text.Json;

namespace BingoSyncIntegration;

class Program
{
	const int SecondsBetweenBoardFetches = 5;
	const int MillisecondsBetweenAM2RFetches = 500;
	const double SecondsBetweenAM2RFetches = MillisecondsBetweenAM2RFetches / 1000.0;
	const int TimesBeforeBoardFetch = (int)(SecondsBetweenBoardFetches / SecondsBetweenAM2RFetches);

	static async Task Main()
	{
		Console.WriteLine("Enter BingoSync room id:");
		var roomId = Console.ReadLine()!;
		Console.WriteLine("Enter BingoSync password:");
		var password = Console.ReadLine()!;

		var am2rClient = new AM2RClient();
		await ConnectToAm2rWithRetry(am2rClient);
		Console.WriteLine("Connected to AM2R");

		var bingoSyncClient = new BingoSyncClient();
		await bingoSyncClient.ConnectAsync(roomId, password);
		Console.WriteLine("Connected to BingoSync");

		var bingoSyncObjectives = await bingoSyncClient.GetBingoSyncObjectivesAsync();
		var objectives = GetAllObjectives().Where(x => bingoSyncObjectives.ContainsKey(x.Name)).ToList();

		var previouslyCompletedObjectiveNames = new HashSet<string>();

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

				var currentlyCompletedObjectives = GetCompletedObjectives(data, objectives);
				var newlyCompletedObjectives = currentlyCompletedObjectives.Where(x => !previouslyCompletedObjectiveNames.Contains(x.Name)).Select(x => x.Name).ToList();
				previouslyCompletedObjectiveNames = currentlyCompletedObjectives.Select(x => x.Name).ToHashSet();

				// TODO: Unmark objectives if they haven't been collected (i.e reloading a save)
				await bingoSyncClient.SendObjectivesAsync(newlyCompletedObjectives, bingoSyncObjectives);
				await Task.Delay(MillisecondsBetweenAM2RFetches);
			}
			var unmarkedObjectives = await bingoSyncClient.GetUnmarkedObjectiveNamesAsync();
			var objectivesToSend = unmarkedObjectives.Where(x => previouslyCompletedObjectiveNames.Contains(x)).ToList();
			await bingoSyncClient.SendObjectivesAsync(objectivesToSend, bingoSyncObjectives);
		}
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

		return completedItems >= objective.ItemCount
			&& completedMetroids >= objective.MetroidCount
			&& completedEvents >= objective.EventCount
			&& completedMapTiles >= objective.MapTileCount;
	}

	static List<ObjectiveJSON> GetAllObjectives()
	{
		var objectivesJson = File.ReadAllText("objectives.json");

		var objectivesList = JsonSerializer.Deserialize<List<ObjectiveJSON>>(objectivesJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

		foreach (var objective in objectivesList)
		{
			if (objective.ItemCount == 0)
			{
				objective.ItemCount = objective.ItemIds.Count;
			}
			if (objective.MetroidCount == 0)
			{
				objective.MetroidCount = objective.MetroidIds.Count;
			}
			if (objective.EventCount == 0)
			{
				objective.EventCount = objective.EventIds.Count;
			}
			if (objective.MapTileCount == 0)
			{
				objective.MapTileCount = objective.MapTileCoords.Count;
			}
		}
		return objectivesList;
	}
}
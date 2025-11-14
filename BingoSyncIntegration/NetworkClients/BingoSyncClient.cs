using BingoSyncIntegration.JSONDataObjects;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BingoSyncIntegration;

public class BingoSyncClient : IDisposable
{
	const string BingoSyncUrl = "https://bingosync.com";

	readonly CookieContainer cookieContainer = new();
	readonly HttpClientHandler httpHandler;
	readonly HttpClient client;

	static readonly JsonSerializerOptions options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
	};

	public string ConnectedRoom { get; private set; } = string.Empty;
	public string Color { get; set; } = "red";

	public BingoSyncClient()
	{
		httpHandler = new() { UseCookies = true, CookieContainer = cookieContainer };
		client = new(httpHandler);
	}

	public async Task ConnectAsync(string roomId, string password)
	{
		var postBody = new BingoSyncJoinRoomJSON()
		{
			Nickname = "Bot",
			Password = password,
			Room = roomId
		};

		var postResponse = await client.PostAsJsonAsync($"{BingoSyncUrl}/api/join-room", postBody, options);
		postResponse.EnsureSuccessStatusCode();

		ConnectedRoom = roomId;
		// Now cookie container contains the session id cookie which enables further post requests
	}

	public async Task SetColorAsync(int slotNum, string color, bool removeColor)
	{
		if (string.IsNullOrEmpty(ConnectedRoom))
		{
			return;
		}

		var selectBody = new BingoSyncSelectJSON()
		{
			Color = color,
			Slot = slotNum.ToString(),
			RemoveColor = removeColor,
			Room = ConnectedRoom
		};

		await client.PutAsJsonAsync($"{BingoSyncUrl}/api/select", selectBody, options);
	}

	public async Task<List<BingoSyncBoardJSON>> GetBoardDataAsync()
	{
		if (string.IsNullOrEmpty(ConnectedRoom))
		{
			return [];
		}

		var boardResponse = await client.GetAsync($"{BingoSyncUrl}/room/{ConnectedRoom}/board");
		boardResponse.EnsureSuccessStatusCode();

		var boardJson = await boardResponse.Content.ReadAsStringAsync();

		var board = JsonSerializer.Deserialize<List<BingoSyncBoardJSON>>(boardJson, options)!;
		return board;
	}

	public async Task SendObjectivesAsync(List<string> objectivesToSend, Dictionary<string, int> nameToSlotMapping)
	{
		if (string.IsNullOrEmpty(ConnectedRoom))
		{
			return;
		}

		foreach (var objective in objectivesToSend)
		{
			await SetColorAsync(nameToSlotMapping[objective], Color, false);
		}
	}

	public async Task UnsendObjectivesAsync(List<string> objectivesToRemove, Dictionary<string, int> nameToSlotMapping)
	{
		if (string.IsNullOrEmpty(ConnectedRoom))
		{
			return;
		}

		foreach (var objective in objectivesToRemove)
		{
			await SetColorAsync(nameToSlotMapping[objective], Color, true);
		}
	}

	public void Dispose()
	{
		client.Dispose();
		GC.SuppressFinalize(this);
	}
}

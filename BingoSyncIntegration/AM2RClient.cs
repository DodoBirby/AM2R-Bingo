using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BingoSyncIntegration;

public class AM2RClient : IDisposable
{
	const int Port = 64195;
	TcpClient tcpClient = new();

	static readonly JsonSerializerOptions options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public AM2RClient()
	{
		tcpClient.NoDelay = true;
	}

	public async Task ConnectAsync()
	{
		var endpoint = new IPEndPoint(IPAddress.Loopback, Port);
		await tcpClient.ConnectAsync(endpoint);
	}

	public async Task<AM2RDataJSON?> GetDataAsync()
	{
		var stream = tcpClient.GetStream();
		using (var writer = new StreamWriter(stream, leaveOpen: true))
		{
			await writer.WriteLineAsync();
		}

		using var reader = new StreamReader(stream, leaveOpen: true);
		var dataJson = await reader.ReadLineAsync();

		if (dataJson is null)
		{
			return null;
		}

		var parsedData = JsonSerializer.Deserialize<AM2RDataJSON>(dataJson, options);
		return parsedData;
	}

	public void Dispose()
	{
		tcpClient.Dispose();
		GC.SuppressFinalize(this);
	}
}

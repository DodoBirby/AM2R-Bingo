using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BingoSyncIntegration;

public class AM2RClient : IDisposable
{
	const int Port = 64195;
	TcpClient tcpClient;

	static readonly JsonSerializerOptions options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public AM2RClient()
	{
		tcpClient = CreateTcpClient();
	}

	public async Task<bool> ConnectAsync()
	{
		try
		{
			if (tcpClient.Connected)
			{
				ReplaceTcpClient();
			}
			var endpoint = new IPEndPoint(IPAddress.Loopback, Port);
			await tcpClient.ConnectAsync(endpoint);
			return true;
		}
		catch (SocketException)
		{
			return false;
		}
	}

	void ReplaceTcpClient()
	{
		tcpClient.Dispose();
		tcpClient = CreateTcpClient();
	}

	static TcpClient CreateTcpClient()
	{
		var newTcpClient = new TcpClient();
		newTcpClient.NoDelay = true;
		return newTcpClient;
	}

	public async Task<AM2RDataJSON?> GetDataAsync()
	{
		try
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
		catch (Exception)
		{
			return null;
		}
	}

	public void Dispose()
	{
		tcpClient.Dispose();
		GC.SuppressFinalize(this);
	}
}

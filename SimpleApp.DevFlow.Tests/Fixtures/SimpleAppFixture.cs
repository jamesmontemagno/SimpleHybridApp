using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Maui.DevFlow.Driver;

namespace SimpleApp.DevFlow.Tests.Fixtures;

/// <summary>
/// Builds and launches SimpleApp with DevFlow enabled, then connects an AgentClient.
/// Pattern adapted from https://github.com/dotnet/maui-labs DevFlow integration tests.
/// </summary>
[TestClass]
public static class SimpleAppFixture
{
	static Process? _appProcess;
	static AgentClient? _client;
	static int _agentPort;
	static string? _appPath;

	public static AgentClient Client
		=> _client ?? throw new InvalidOperationException("DevFlow fixture is not initialized. Is SimpleApp running?");

	public static int AgentPort => _agentPort;

	[AssemblyInitialize]
	public static async Task AssemblyInitializeAsync(TestContext context)
	{
		_agentPort = AllocatePort();
		Environment.SetEnvironmentVariable("DEVFLOW_TEST_PORT", _agentPort.ToString());

		var repoRoot = FindRepoRoot();
		var projectPath = Path.Combine(repoRoot, "SimpleApp", "SimpleApp.csproj");
		if (!File.Exists(projectPath))
			Assert.Inconclusive($"SimpleApp project not found at {projectPath}");

		await BuildAppAsync(projectPath);

		_appPath = FindAppExecutable(repoRoot);
		if (_appPath is null)
			Assert.Inconclusive($"Could not locate the {GetTargetFramework()} SimpleApp executable.");

		var psi = new ProcessStartInfo(_appPath)
		{
			UseShellExecute = false,
			WorkingDirectory = Path.GetDirectoryName(_appPath) ?? Environment.CurrentDirectory,
		};
		psi.Environment["DEVFLOW_TEST_PORT"] = _agentPort.ToString();

		_appProcess = Process.Start(psi)
			?? throw new InvalidOperationException($"Failed to launch {_appPath}");

		_client = new AgentClient("localhost", _agentPort);
		await WaitForAgentAsync(_client, TimeSpan.FromSeconds(45));
	}

	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		try
		{
			if (_appProcess is { HasExited: false })
			{
				_appProcess.Kill(entireProcessTree: true);
				_appProcess.WaitForExit(5000);
			}
		}
		catch
		{
			// best effort
		}
		finally
		{
			_appProcess?.Dispose();
			_client?.Dispose();
		}
	}

	static async Task BuildAppAsync(string projectPath)
	{
		var psi = new ProcessStartInfo("dotnet")
		{
			ArgumentList =
			{
				"build",
				projectPath,
				"-f", GetTargetFramework(),
				"-c", "Debug",
				"-p:MauiDevFlowEnabled=true",
				"--nologo",
				"-v", "q"
			},
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};

		using var process = Process.Start(psi)
			?? throw new InvalidOperationException("Failed to start dotnet build");

		var stdout = await process.StandardOutput.ReadToEndAsync();
		var stderr = await process.StandardError.ReadToEndAsync();
		await process.WaitForExitAsync();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException(
				$"dotnet build failed (exit {process.ExitCode}).\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
		}
	}

	static async Task WaitForAgentAsync(AgentClient client, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			var status = await client.GetStatusAsync();
			if (status is not null)
				return;

			await Task.Delay(500);
		}

		throw new TimeoutException($"DevFlow agent did not become ready on port {_agentPort} within {timeout.TotalSeconds}s.");
	}

	static string GetTargetFramework()
	{
		if (OperatingSystem.IsWindows())
			return "net10.0-windows10.0.19041.0";

		if (OperatingSystem.IsMacOS())
			return "net10.0-maccatalyst";

		throw new PlatformNotSupportedException("SimpleApp DevFlow integration tests support Windows and macOS only.");
	}

	static string? FindAppExecutable(string repoRoot)
	{
		var binDir = Path.Combine(repoRoot, "SimpleApp", "bin", "Debug");
		if (!Directory.Exists(binDir))
			return null;

		if (OperatingSystem.IsWindows())
		{
			return Directory.GetFiles(binDir, "SimpleApp.exe", SearchOption.AllDirectories)
			.OrderByDescending(File.GetLastWriteTimeUtc)
			.FirstOrDefault();
		}

		var appBundle = Directory.GetDirectories(binDir, "SimpleApp.app", SearchOption.AllDirectories)
			.OrderByDescending(Directory.GetLastWriteTimeUtc)
			.FirstOrDefault();
		var executablePath = appBundle is null
			? null
			: Path.Combine(appBundle, "Contents", "MacOS", "SimpleApp");

		return executablePath is not null && File.Exists(executablePath) ? executablePath : null;
	}

	static string FindRepoRoot()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir is not null)
		{
			if (File.Exists(Path.Combine(dir.FullName, "SimpleHybridApp.sln")))
				return dir.FullName;
			dir = dir.Parent;
		}

		// Fallback: walk up from current directory.
		dir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (dir is not null)
		{
			if (File.Exists(Path.Combine(dir.FullName, "SimpleHybridApp.sln")))
				return dir.FullName;
			dir = dir.Parent;
		}

		throw new InvalidOperationException("Could not locate repository root containing SimpleHybridApp.sln");
	}

	static int AllocatePort()
	{
		using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
		listener.Start();
		return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
	}
}

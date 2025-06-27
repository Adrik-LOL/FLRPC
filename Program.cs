using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using System.Drawing;
using Console = Colorful.Console;

// Configuration settings
using static ConfigSettings;

// ClientID and settings
using static ConfigValues;

// Memory stuff
using static Utils;
using System.Runtime.InteropServices;

public static class Program
{

    // Import WINAPI functions
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    static extern bool FreeConsole();

    // Constant for hiding the window
    const int SW_HIDE = 0;

    // Initialize a new Rich Presence client
    public static DiscordRpcClient _Client;

    // Configuration file location
    public static readonly string ConfigPath = Path.Combine(Path.GetTempPath(), "fls_rpc_config.json");

    // Initialize a placeholder, because we don't want null reference exceptions
    private static RichPresence _RPC = new RichPresence()
    {
        Details = "",
        State = "",
        Assets = new Assets()
        {
            LargeImageKey = "fl_studio_logo",
        }
    };

    static void InitializeRPC()
    {
        // Create a new Rich Presence client, set the client ID, and most importantly, disable the autoEvents so we can update manually
        _Client = new DiscordRpcClient(ClientID, -1, null, false);

        // Don't update the presence if it didn't change
        _Client.SkipIdenticalPresence = true;

        // Create a console logger
        _Client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Warning, Coloured = true };

        // Register events
        _Client.OnReady += Events.OnReady;
        _Client.OnClose += Events.OnClose;
        _Client.OnError += Events.OnError;
        _Client.OnConnectionEstablished += Events.OnConnectionEstablished;
        _Client.OnConnectionFailed += Events.OnConnectionFailed;
        _Client.OnPresenceUpdate += Events.OnPresenceUpdate;

        // Initialize the client
        _Client.Initialize();
    }


    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-?")
        {
            Console.WriteLine("FL Studio Rich Presence by zfi2 /|\\ Forked by Adrik-LOL", Color.LightSkyBlue);
            Console.WriteLine("Usage: FLRPC [options]", Color.LightSkyBlue);
            Console.WriteLine("Options:", Color.LightSkyBlue);
            Console.WriteLine("  -?          Show this help message", Color.LightSkyBlue);
            Console.WriteLine("  -console    Show the console window", Color.LightSkyBlue);
        } else if (args.Length == 0 || (args.Length > 0 && args[0] != "-console"))
        {
            // Get the current console window
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_HIDE); // Hide the console window
            FreeConsole(); // Make it disappear from existence
            Run();
        } else
        {
            Run();
        }

    }

    static void Run()
    {

        // Save default config with default values (also load it at startup, the function is already called in SaveConfig)
        SaveConfig(ConfigPath);

        // If DisplayConfigInfo is enabled, read all settings from file and print them
        if (DisplayConfigInfo)
        {
            try
            {
                // Read JSON from file
                string json = File.ReadAllText(ConfigPath);

                // Deserialize JSON to dictionary
                var properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                // Print each setting to console
                foreach (var property in properties)
                {
                    Console.WriteLine($"{property.Key} => {property.Value}", Color.White);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., file not found, invalid JSON format)
                Console.WriteLine($"Error loading configuration from file: {ex.Message}", Color.Red);
                Utils.LogException(ex, "Main");
            }
            // Extra newline after the configuration output
            Console.WriteLine();
        }

        Console.WriteLine("Initializing the rich presence...", Color.LightSkyBlue);

        // Initialize the Rich Presence
        InitializeRPC();

        // Initialize a timestamp if it's enabled in the config
        if (ShowTimestamp)
        {
            _RPC.Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            };
        }

        // If client is valid, continue the loop
        while (_Client != null)
        {
            // Retrieve the FL Studio data constantly, so that we're up to date
            FLInfo FLStudioData = GetFLInfo();

            // Invoke event handlers
            _Client.Invoke();

            // Check if AppName and ProjectName are both empty or null
            bool NoProject = string.IsNullOrEmpty(FLStudioData.AppName) && string.IsNullOrEmpty(FLStudioData.ProjectName);

            // Set details and state based on conditions
            _RPC.Details = NoProject ? "FL Studio (inactive)" : FLStudioData.AppName;
            _RPC.State = NoProject ? "No project" : FLStudioData.ProjectName ?? "Empty project";

            // Check if secret mode is enabled and set the state accordingly
            if (SecretMode)
                _RPC.State = "Working on a hidden project";

            // Finally, set the presence
            _Client?.SetPresence(_RPC);

            // Sleep for the interval defined in the config file
            Thread.Sleep(UpdateInterval);
        }
    }
}
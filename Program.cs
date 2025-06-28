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
using System.Diagnostics;
using FLRPC.Config;

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
            Console.WriteLine("  -?            Show this help message", Color.LightSkyBlue);
            Console.WriteLine("  -console      Show the console window", Color.LightSkyBlue);
            Console.WriteLine("  -configfile   Opens the Config file", Color.LightSkyBlue);
            Console.WriteLine("  -reset        Resets (by deleting) the configuration file", Color.LightSkyBlue);
        }
        else if (args.Length > 0 && args[0] == "-configfile")
        {
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("Configuration file not found, Start the program first.", Color.Yellow);
            }
            else
            {
                var configForm = new ConfigFile();
                configForm.ShowDialog();
            }
        }
        else if (args.Length > 0 && args[0] == "-reset")
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                    Console.WriteLine("Configuration file deleted successfully.", Color.Green);
                }
                else
                {
                    Console.WriteLine("Configuration file not found.", Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting configuration file: {ex.Message}", Color.Red);
                Utils.LogException(ex, "Main");
            }
        }
        else if (args.Length == 0 || (args.Length > 0 && args[0] != "-console"))
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_HIDE);
            FreeConsole();
            while (true) // Loop to check if FL Studio is running
            {
                if (Utils.IsFLStudioRunning()) // Check if FL Studio is running
                {
                    Console.WriteLine("FL Studio is running!", Color.Green);
                    Run(); // Start the Rich Presence loop
                }
                else
                {
                    Console.WriteLine("FL Studio is not running. Waiting...", Color.Yellow);
                    Thread.Sleep(5000); // Wait 5 seconds and check again
                }
            }
        }
        else
        {
            while (true)
            {
                if (Utils.IsFLStudioRunning())
                {
                    Console.WriteLine("FL Studio is running!", Color.Green);
                    Run();
                }
                else
                {
                    Console.WriteLine("FL Studio is not running. Waiting...", Color.Yellow);
                    Thread.Sleep(5000);
                }
            }
        }


    }

    static void Run()
    {

        SaveConfig(ConfigPath);

        // Display the config info if enabled
        if (DisplayConfigInfo)
        {
            try
            {
                string json = File.ReadAllText(ConfigPath);
                var properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                foreach (var property in properties)
                    Console.WriteLine($"{property.Key} => {property.Value}", Color.White);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration from file: {ex.Message}", Color.Red);
                Utils.LogException(ex, "Main");
            }

            Console.WriteLine(); // Extra newline after config output
        }

        Console.WriteLine("Initializing the rich presence...", Color.LightSkyBlue);

        // Start Discord RPC client
        InitializeRPC();

        // Add a timestamp to the RPC if the user enabled it
        if (ShowTimestamp)
        {
            _RPC.Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            };
        }

        // Loop while the Discord client is active
        while (_Client != null)
        {
            // Check if FL Studio is no longer running
            if (!IsFLStudioRunning())
            {
                Console.WriteLine("FL Studio was closed. Shutting down RPC...", Color.OrangeRed);

                // Clear presence from Discord
                _Client.ClearPresence();

                // Cleanly shut down the client
                _Client.Dispose();
                _Client = null;

                break; // Exit the loop
            }

            // Get current data from FL Studio (e.g. project name, mode)
            FLInfo FLStudioData = GetFLInfo();

            // Invoke Discord RPC event handlers
            _Client.Invoke();

            // Check if there's no active project
            bool NoProject = string.IsNullOrEmpty(FLStudioData.AppName) && string.IsNullOrEmpty(FLStudioData.ProjectName);

            // Set the RPC "Details" and "State"
            _RPC.Details = NoProject ? "FL Studio (inactive)" : FLStudioData.AppName;
            _RPC.State = NoProject ? "No project" : FLStudioData.ProjectName ?? "Empty project";

            // If SecretMode is enabled, hide the project name
            if (SecretMode)
                _RPC.State = "Working on a hidden project";

            // Push the updated Rich Presence to Discord
            _Client.SetPresence(_RPC);

            // Wait for the interval defined in the config (usually ~1000ms)
            Thread.Sleep(UpdateInterval);
        }
    }

}
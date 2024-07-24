using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FunFactCountdown.Windows;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.Text;

namespace FunFactCountdown;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ISigScanner Scanner { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; set; } = null!;

    public readonly WindowSystem WindowSystem = new("FunFactCountdown");
    public Configuration Configuration { get; init; }

    internal ChatCommon Common { get; } = null!;

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private const string CommandName = "/ffcd";
    //private readonly IChatGui ChatGui;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        Common = new ChatCommon();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Run a countdown that shares fun facts to your party!"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }
    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

    }

    private void OnCommand(string command, string args)
    {
        // Require args
        if (args == "")
        {
            ToggleMainUI();
        } else
        {
            string[] arrArgs = args.Split(" ");
            RunCountdown(arrArgs);
        }


    }

    // Chat messages sent to the server in the user's currently selected channel
    public void ChatHook(string message)
    {
        string trimmed = message.Trim();
        Plugin.Framework.RunOnTick(() =>
        {
            Common.SendMessage(trimmed);
        });        
    }

    // Client-side chat messages not sent to the server
    public static void ChatClient(string message, XivChatType chatType)
    {
        ChatGui.Print(new XivChatEntry()
        {
            Message = message,
            Type = chatType
        });
    }

    public async void RunCountdown(string[] args)
    {
        // Ensure all args are numbers
        if (!args.All(e => e.All(Char.IsDigit)))
        {
            ChatClient($"All arguments must be numbers.", XivChatType.ErrorMessage);
            return;
        }

        // Ensure a length for countdown was passed
        int cdLength = int.Parse(args[0]);
        if (cdLength <= 0)
        {
            ChatClient($"You must specify a length for the countdown. For example: \"/ffcd 15\".", XivChatType.ErrorMessage);
            return;
        }

        // Max countdown is 30
        if (cdLength > 30)
        {
            ChatClient($"You cannot start a countdown of more than 30 seconds.", XivChatType.ErrorMessage);
            return;
        }


        // Build the list of intervals at which to send a fact
        List<int> factIntervals = args.Skip(1).ToList().ConvertAll(e => int.Parse(e));

        // Set intervals for each second, if none provided
        if (factIntervals.Count == 0)
        {
            factIntervals.AddRange(Enumerable.Range(0, cdLength));
        }

        // Intervals can't be greater than the countdown length or less than 0
        if (factIntervals.Any(e => e > cdLength) || factIntervals.Any(e => e < 0))
        {
            ChatClient($"None of the provided intervals can be greater than the countdown length or less than 0.", XivChatType.ErrorMessage);
            return;
        }

        // Ensure the number of intervals doesn't exceed the length of the countdown
        if (factIntervals.Count > cdLength)
        {
            ChatClient($"The number of intervals cannot be greater than the countdown length.", XivChatType.ErrorMessage);
            return;
        }

        // Build Facts array
        List<string> facts = [];
        for (int i = 0; i < factIntervals.Count; i++)
        {
            facts.Add(Configuration.FactsList[new Random().Next(0, Configuration.FactsList.Length - 1)]);
        }

        // Ensure number of facts == factIntervals
        if (facts.Count != factIntervals.Count && facts.Count != cdLength)
        {
            ChatClient($"The number of facts retrieved does not match the number of intervals specified.", XivChatType.ErrorMessage);
            return;
        }

        // Sort intervals descending to iterate properly (i.e. 10 -> 5 -> 0)
        factIntervals = factIntervals.OrderDescending().ToList();

        // Run countdown
        if (Configuration.SendStartingMessage)
        {
            ChatHook($"Starting the fun fact countdown! <se.{Configuration.StartingMessageSE}>");
            await Task.Delay(Configuration.StartingMessageDelayMs);
        }
        
        ChatHook($"/cd {cdLength}");

        for (int i = 0; i < factIntervals.Count; i++)
        {
            //Set the delay between the previous interval and next interval
            //If we want our first fact at 10s in a 15s countdown, calc 15-10 for a 5s delay
            int lastI = i == 0 ? cdLength : factIntervals[i - 1];
            await Task.Delay((lastI - factIntervals[i]) * 1000);
            ChatHook($"{factIntervals[i]}s: {facts[i]} <se.{Configuration.FunFactSE}>");
        }

        return;

    }
}

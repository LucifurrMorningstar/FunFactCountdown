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
using System.Text;
using Dalamud.Game;

namespace FunFactCountdown;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState clientState { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ISigScanner scanner { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;

    public readonly WindowSystem WindowSystem = new("FunFactCountdown");
    public Configuration Configuration { get; init; }

    internal ChatCommon Common { get; } = null!;

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private const string CommandName = "/ffcd";
    private readonly IChatGui chat;

    public Plugin(IChatGui chat)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.chat = chat;

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
        //PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

    }

    public void partyChat(string message)
    {
        string trimmed = message.Trim();
        var bytes = Encoding.UTF8.GetBytes(trimmed);

        Plugin.Framework.RunOnTick(() =>
        {
            Common.SendMessage(trimmed);
        });        
    }

    private void OnCommand(string command, string args)
    {
        // Require args
        if (args == "")
        {
            this.chat.Print($"Use \"/ffcd <length> <interval1> <interval2> ...\"");
            return;
        }
        string[] arrArgs = args.Split(" ");
        runCountdown(arrArgs.ToArray());
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public async void runCountdown(string[] args)
    {
        // Ensure all args are numbers
        if (!args.All(e => e.All(Char.IsDigit)))
        {
            this.chat.Print($"All arguments must be numbers!");
            return;
        }
        int countdown = int.Parse(args[0]);

        // Ensure a length for countdown was passed
        if (countdown < 0)
        {
            this.chat.Print($"You must specify a length for the countdown. For example: \"ffcd st 15\"");
            return;
        }

        // Max countdown is 30
        if (countdown > 30)
        {
            this.chat.Print($"You cannot start a countdown of more than 30 seconds.");
            return;
        }

        List<int> factIntervals = Array.ConvertAll(args.Skip(1).ToArray(), e => int.Parse(e)).ToList();

        // Intervals can't be greater than the countdown length or less than 0
        if (factIntervals.Any(e => e > countdown) || factIntervals.Any(e => e < 0))
        {
            this.chat.Print($"None of the provided intervals can be greater than the countdown length");
            return;
        }

        // Set intervals for each second, if none provided
        if (factIntervals.Count == 0)
        {
            int count = countdown;
            while (count > 0)
            {
                factIntervals.Add(count);
                count--;
            }
        }
        factIntervals.OrderDescending();

        // Determine number of facts to get
        int numFacts = countdown;
        if (factIntervals.Count > 0)
        {
            numFacts = factIntervals.Count;
        }

        if (numFacts > countdown)
        {
            this.chat.Print($"The number of intervals cannot be greater than the countdown length.");
            return;
        }

        // Build Facts array
        string[] factsList = this.Configuration.FactsList;
        List<string> facts = new List<string>();
        for (int i = 0; i < numFacts; i++)
        {
            facts.Add(factsList[new Random().Next(0, factsList.Length - 1)]);
        }

        // Ensure number of facts == factIntervals
        if (facts.Count != factIntervals.Count && facts.Count != countdown)
        {
            this.chat.Print($"The number of facts retrieved does not match the number of intervals specified.");
            return;
        }

        // Run countdown
        partyChat($"Beginning the fun fact countdown!");
        await Task.Delay(1000);

        partyChat($"/cd {countdown}");

        for (int i = 0; i < factIntervals.Count; i++)
        {
            int lastI = i == 0 ? countdown : factIntervals[i - 1];
            await Task.Delay((lastI - factIntervals[i]) * 1000);
            partyChat($"{factIntervals[i]}s: {facts[i]} <se.6>");
        }

        return;

    }
}

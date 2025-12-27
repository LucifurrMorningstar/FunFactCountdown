using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace FunFactCountdown.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("FFCD##MainWindow")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(532, 290);
        SizeCondition = ImGuiCond.Always;
        Plugin = plugin;
    }

    public void Dispose() 
    { 
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        ImGui.TextWrapped("Use /ffcd to start a countdown and send fun facts to your chat at specified intervals.\n\nFor example: \"/ffcd 15 10 5 0\" will start a 15s countdown and send a fun fact at 10s, 5s, and 0s. If you don't specify any intervals, a Fun Fact is sent every second of the countdown (i.e. \"/ffcd 15\")\n\nNOTE: This will send to your currently selected chat channel (i.e. Say, Party, CWLS, etc.)");
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Button("Settings"))
        {
            Plugin.ToggleConfigUI();
        }

    }
}

using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.UI;


namespace FunFactCountdown.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin Plugin) : base("FFCD Settings###FFCD Settings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;
        
        Size = new Vector2(532, 290);
        SizeCondition = ImGuiCond.Always;
        Configuration = Plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Sound effect for the "Starting fun fact countdown!" chat message
        int beginningSE = Array.IndexOf(Configuration.SoundEffectsList, Configuration.StartingMessageSE);
        ImGui.Text("Starting Message Sound Effect");
        if (ImGui.Combo(" ", ref beginningSE, Configuration.SoundEffectsList, Configuration.SoundEffectsList.Length)) {
            string newSE = Configuration.SoundEffectsList[beginningSE];
            Configuration.StartingMessageSE = newSE;
            UIModule.PlayChatSoundEffect(Convert.ToUInt32(newSE));
            Configuration.Save();
        }

        // Set the delay between the "Starting fun fact countdown!" message and the countdown command
        // TODO MAYBE

        // Toggle "Starting fun fact countdown!" chat message
        bool toggle = Configuration.SendStartingMessage;
        if (ImGui.Checkbox("Send \"Starting fun fact countdown!\" chat message", ref toggle))
        {
            Configuration.SendStartingMessage = toggle;
            Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // Sound effect for each fun fact that's sent
        ImGui.Text("Fun Fact Sound Effect");
        int ffSE = Array.IndexOf(Configuration.SoundEffectsList, Configuration.FunFactSE);
        if (ImGui.Combo("  ", ref ffSE, Configuration.SoundEffectsList, Configuration.SoundEffectsList.Length))
        {
            string newSE = Configuration.SoundEffectsList[ffSE];
            Configuration.FunFactSE = newSE;
            UIModule.PlayChatSoundEffect(Convert.ToUInt32(newSE));
            Configuration.Save();
        }

    }
}

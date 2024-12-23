using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Linq;


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
        
        Size = new Vector2(532, 390);
        SizeCondition = ImGuiCond.Always;
        Configuration = Plugin.Configuration;
    }

    public void Dispose() 
    { 
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        ImGui.TextWrapped("Use /ffcd to start a countdown and send fun facts to your chat at specified intervals.\n\nFor example: \"/ffcd 15 10 5 0\" will start a 15s countdown and send a fun fact at 10s, 5s, and 0s.\n\nIf you don't specify any intervals, a Fun Fact is sent every second of the countdown (i.e. \"/ffcd 15\")");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Change chat channel
        string[] channels = Configuration.Channels.Values.ToArray();
        string channel = Configuration.Channel;
        int iCh = Array.IndexOf(channels, channel);
        ImGui.Text("Channel");
        if (ImGui.Combo(" ", ref iCh, Configuration.Channels.Keys.ToArray(), channels.Length))
        {
            int newCh = iCh;
            Configuration.Channel = channels[newCh];
            Configuration.Save();
        }

        // Sound effect for the "Starting fun fact countdown!" chat message
        int beginningSE = Array.IndexOf(Configuration.SoundEffectsList, Configuration.StartingMessageSE);
        ImGui.Text("Starting Message Sound Effect");
        if (ImGui.Combo("  ", ref beginningSE, Configuration.SoundEffectsList, Configuration.SoundEffectsList.Length)) {
            string newSE = Configuration.SoundEffectsList[beginningSE];
            Configuration.StartingMessageSE = newSE;
            UIGlobals.PlayChatSoundEffect(Convert.ToUInt32(newSE));
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
        if (ImGui.Combo("   ", ref ffSE, Configuration.SoundEffectsList, Configuration.SoundEffectsList.Length))
        {
            string newSE = Configuration.SoundEffectsList[ffSE];
            Configuration.FunFactSE = newSE;
            UIGlobals.PlayChatSoundEffect(Convert.ToUInt32(newSE));
            Configuration.Save();
        }

        // Enable Dad Mode
        bool toggleDadMode = Configuration.enableDadMode;
        if (ImGui.Checkbox("Enable Dad Mode", ref toggleDadMode))
        {
            Configuration.enableDadMode = toggleDadMode;
            Configuration.Save();
        }

    }
}

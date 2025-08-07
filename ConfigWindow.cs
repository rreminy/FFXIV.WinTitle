using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace WinTitle
{
    public class ConfigWindow : Window
    {
        public ConfigWindow() : base("WinTitle Configuration###WTConfig")
        {
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse;

            Size = new Vector2(300, 100);
            SizeCondition = ImGuiCond.Always;
        }

        public override void Draw()
        {
            ImGui.BeginGroup();
            if (ImGui.Checkbox("Set logged character as window title", ref WinTitle.Config.SetTitleToLoggedCharacter))
                WinTitle.Config.Save();
            ImGui.EndGroup();
        }
    }
}
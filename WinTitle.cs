using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace WinTitle
{
    public sealed class WinTitle : IDalamudPlugin
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private readonly string _originalTitle;
        private readonly IntPtr _handle;

        private readonly WindowSystem WindowSystem = new("WinTitle");
        private ConfigWindow ConfigWindow { get; init; }
        public static Configuration Config { get; private set; } = default!;

        [PluginService] private static ICommandManager CommandManager { get; set; } = default!;
        [PluginService] private static IPluginLog Logger { get; set; } = default!;
        [PluginService] private static IClientState ClientState { get; set; } = default!;
        [PluginService]
        public static IDalamudPluginInterface PluginInterface { get; private set; } = default!;

        public WinTitle()
        {
            Config = Configuration.Load();
            
            using Process process = Process.GetCurrentProcess();
            this._handle = process.MainWindowHandle;
            this._originalTitle = process.MainWindowTitle;
            
            if (Config.SetTitleToLoggedCharacter)
            {
                /* https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Plugin/Services/IClientState.cs#L44
                // LocalPlayer should always be valid, according to docs
                // If not, implement a validation with a wait in a Task
                */
                ClientState.Login += SetTitleToLoggedCharacter;
            }
            
            ConfigWindow = new ConfigWindow();
            WindowSystem.AddWindow(ConfigWindow);
            PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.Toggle();
            PluginInterface.UiBuilder.OpenMainUi += () => ConfigWindow.Toggle();

            CommandManager.AddHandler("/wintitle", new CommandInfo(WintitleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "Change window title. Empty to revert.",
            });
        }

        public void SetTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) title = this._originalTitle;
            if (string.IsNullOrWhiteSpace(title)) title = "FINAL FANTASY XIV";
            try { SetWindowText(this._handle, title.Trim()); } catch (Exception ex) { Logger.Error(ex, $"Failed to set Window Title to {title}"); }
        }

        private void SetTitleToLoggedCharacter()
        {
            var player = ClientState.LocalPlayer;
            var world = "Unknown";
            if (player == null || !player.IsValid()) return;
            var worldData = player.CurrentWorld.GameData;
            if (worldData != null) world = worldData.Name.ToString();
            SetTitle($"{player.Name}@{world}");
        }

        private void WintitleCommand(string _, string title) => this.SetTitle(title);

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            this.SetTitle(null);
            CommandManager.RemoveHandler("/wintitle");
            WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
        }
    }
}

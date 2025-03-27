using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace WinTitle
{
    public sealed class WinTitle : IDalamudPlugin
    {
        private readonly IntPtr _handle;

        private readonly string _originalTitle;

        private readonly WindowSystem _windowSystem = new("WinTitle");

        public WinTitle()
        {
            Config = Configuration.Load();

            using var process = Process.GetCurrentProcess();
            _handle = process.MainWindowHandle;
            _originalTitle = process.MainWindowTitle;

            if (Config.SetTitleToLoggedCharacter)
                /* https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Plugin/Services/IClientState.cs#L44
                // LocalPlayer should always be valid, according to docs
                // If not, implement a validation with a wait in a Task
                */
                ClientState.Login += SetTitleToLoggedCharacter;
            WorldWatcher = new CurrentWorldWatcher(SetTitleToLoggedCharacter);

            ConfigWindow = new ConfigWindow();
            _windowSystem.AddWindow(ConfigWindow);
            PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
            PluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.Toggle();
            PluginInterface.UiBuilder.OpenMainUi += () => ConfigWindow.Toggle();

            CommandManager.AddHandler("/wintitle", new CommandInfo(WinTitleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "Change window title. Empty to revert."
            });
        }

        private ConfigWindow ConfigWindow { get; }
        public static Configuration Config { get; private set; } = null!;

        private CurrentWorldWatcher WorldWatcher { get; }

        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IPluginLog Logger { get; private set; } =  null!;
        [PluginService] public static IClientState ClientState { get; private set; } =  null!;

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService] public static IFramework Framework { get; private set; } =  null!;

        private bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            SetTitle(null);
            CommandManager.RemoveHandler("/wintitle");
            WorldWatcher.Dispose();
            _windowSystem.RemoveAllWindows();
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private void SetTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) title = _originalTitle;
            if (string.IsNullOrWhiteSpace(title)) title = "FINAL FANTASY XIV";
            try
            {
                SetWindowText(_handle, title.Trim());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to set Window Title to {title}");
            }
        }

        private void SetTitleToLoggedCharacter()
        {
            var player = ClientState.LocalPlayer;
            if (player == null || !player.IsValid() || !player.CurrentWorld.IsValid)
            {
                SetTitle("");
                return;
            };
            var currentWorld = player.CurrentWorld.Value;
            SetTitle($"{player.Name}@{currentWorld.Name}");
        }

        private void WinTitleCommand(string _, string title)
        {
            SetTitle(title);
        }
    }
}
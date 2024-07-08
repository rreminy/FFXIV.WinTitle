using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

namespace WinTitle
{
    public sealed class WinTitle : IDalamudPlugin
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private readonly string _originalTitle;
        private readonly IntPtr _handle;

        [PluginService]
        private ICommandManager CommandManager { get; set; } = default!;
        [PluginService]
        private IPluginLog Logger { get; set; } = default!;

        public WinTitle(IDalamudPluginInterface pluginInterface)
        {
            pluginInterface.Inject(this);

            using Process process = Process.GetCurrentProcess();
            this._handle = process.MainWindowHandle;
            this._originalTitle = process.MainWindowTitle;

            this.CommandManager.AddHandler("/wintitle", new CommandInfo(WintitleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "Change window title. Empty to revert.",
            });
        }

        public void SetTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) title = this._originalTitle;
            if (string.IsNullOrWhiteSpace(title)) title = "FINAL FANTASY XIV";
            try { SetWindowText(this._handle, title.Trim()); } catch (Exception ex) { this.Logger.Error(ex, $"Failed to set Window Title to {title}"); }
        }

        private void WintitleCommand(string _, string title) => this.SetTitle(title);

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            this.SetTitle(null);
            CommandManager.RemoveHandler("/wintitle");
        }
    }
}

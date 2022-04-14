using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;

namespace WinTitle
{
    public sealed class WinTitle : IDalamudPlugin
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private readonly string OriginalTitle;
        private readonly IntPtr Handle;

        public string Name => "Window Title Changer";

        [PluginService]
        private CommandManager CommandManager { get; set; } = default!;

        public WinTitle()
        {
            using Process process = Process.GetCurrentProcess();
            this.Handle = process.MainWindowHandle;
            this.OriginalTitle = process.MainWindowTitle;

            this.CommandManager.AddHandler("/wintitle", new CommandInfo(WintitleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "Change window title. Empty to revert.",
            });
        }

        public void SetTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) title = this.OriginalTitle;
            if (string.IsNullOrWhiteSpace(title)) title = "FINAL FANTASY XIV";
            try { SetWindowText(this.Handle, title.Trim()); } catch (Exception ex) { PluginLog.LogError(ex, $"Failed to set Window Title to {title}"); }
        }

        private void WintitleCommand(string _, string title) => this.SetTitle(title);

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            this.SetTitle(null);
            CommandManager.RemoveHandler("/wintitle");
            GC.SuppressFinalize(this);
        }
        ~WinTitle() => this.Dispose();
    }
}

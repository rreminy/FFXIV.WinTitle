using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;

namespace WinTitle
{
    public class WinTitle : IDalamudPlugin
    {
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        private readonly string OriginalTitle;
        private readonly IntPtr Handle;

        public string Name => "Window Title Changer";

        [PluginService]
        private DalamudPluginInterface PluginInterface { get; set; }
        private CommandManager commandManager { get; init; }

        public WinTitle(CommandManager commandManager)
        {
            using Process process = Process.GetCurrentProcess();
            this.Handle = process.MainWindowHandle;
            this.OriginalTitle = process.MainWindowTitle;

            this.commandManager = commandManager;

            this.commandManager.AddHandler("/wintitle", new CommandInfo(WintitleCommand)
            {
                ShowInHelp = true,
                HelpMessage = "Change window title. Empty to revert.",
            });
        }

        public void SetTitle(string title)
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
            commandManager.RemoveHandler("/wintitle");
            this.PluginInterface.Dispose();

            GC.SuppressFinalize(this);
        }
        ~WinTitle() => this.Dispose();
    }
}

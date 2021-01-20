using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Game.Command;

namespace WinTitle
{
    public class WinTitle : IDalamudPlugin
    {
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        private readonly string OriginalTitle;
        private readonly IntPtr Handle;

        public string Name => "Window Title Changer";
        private DalamudPluginInterface PluginInterface { get; set; }

        public WinTitle()
        {
            using Process process = Process.GetCurrentProcess();
            this.Handle = process.MainWindowHandle;
            this.OriginalTitle = process.MainWindowTitle;
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            pluginInterface.CommandManager.AddHandler("/wintitle", new CommandInfo(WintitleCommand)
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
            this.PluginInterface.CommandManager.RemoveHandler("/wintitle");
            this.PluginInterface.Dispose();

            GC.SuppressFinalize(this);
        }
        ~WinTitle() => this.Dispose();
    }
}

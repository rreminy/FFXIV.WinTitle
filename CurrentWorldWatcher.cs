using System;
using Dalamud.Plugin.Services;

namespace WinTitle
{
    public sealed class CurrentWorldWatcher : IDisposable
    {
        private readonly Action _updateTitle;
        private uint? _lastCurrentWorld;

        public CurrentWorldWatcher(Action updateTitle)
        {
            // removed setting of _lastCurrentWorld here because it'll just get set in OnFrameworkTick anyway
            this._updateTitle = updateTitle;
            WinTitle.Framework.Update += OnFrameworkTick;
        }

        public void Dispose()
        {
            WinTitle.Framework.Update -= OnFrameworkTick;
            GC.SuppressFinalize(this);
        }

        private void OnFrameworkTick(IFramework framework)
        {
            if (!WinTitle.Config.SetTitleToLoggedCharacter) return;
            var currentWorld = GetCurrentWorld();
            if (currentWorld == _lastCurrentWorld) return;

            this._lastCurrentWorld = currentWorld;
            WinTitle.Logger.Information($"The player's current world has updated to {currentWorld}.");
            this._updateTitle();
        }

        private static uint? GetCurrentWorld()
        {
            var player = WinTitle.ClientState.LocalPlayer;
            if (player == null || !player.IsValid() || !player.CurrentWorld.IsValid) return null;
            return player.CurrentWorld.RowId;
        }
    }
}
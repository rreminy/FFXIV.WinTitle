using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace WinTitle
{
    public class CurrentWorldWatcher : IDisposable
    {
        private readonly Action _updateTitle;
        private uint? _lastCurrentWorld;

        public CurrentWorldWatcher(Action updateTitle)
        {
            _lastCurrentWorld = getCurrentWorld();
            _updateTitle = updateTitle;
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
            var currentWorld = getCurrentWorld();
            if (currentWorld == _lastCurrentWorld) return;

            _lastCurrentWorld = currentWorld;
            WinTitle.Logger.Information($"The player's current world has updated to {currentWorld}.");
            _updateTitle();
        }

        private static uint? getCurrentWorld()
        {
            var player = WinTitle.ClientState.LocalPlayer;
            if (player == null || !player.IsValid() || !player.CurrentWorld.IsValid) return null;
            return player.CurrentWorld.RowId;
        }
    }
}
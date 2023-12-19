using CommLines.CommLines;
using CommLines.CommNet;
using KSP.Game;
using KSP.Messages;

namespace CommLines
{
    public static class EventListener
    { 
        public static bool IsInMapView { get; private set; }

        public static void RegisterEvents()
        {
            var messageCenter = GameManager.Instance.Game.Messages;
            messageCenter.PersistentSubscribe<MapInitializedMessage>(OnMapInitialized);
            messageCenter.PersistentSubscribe<MapViewLeftMessage>(OnMapViewLeft);
        }

        private static void OnMapInitialized(MessageCenterMessage _)
        {
            // force the map to update as each time the map gets initialized all objects get generated again
            IsInMapView = true;

            // Don't do anything if the CommNet lines are disabled
            if (CommLinesPlugin.CommNetModeEntry.Value == CommNetMode.Disabled) return;
            PluginCommNetManager.RefreshLinks();
        }

        private static void OnMapViewLeft(MessageCenterMessage _)
        {
            IsInMapView = false;
        }
    }
}

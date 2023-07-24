using KSP.Game;
using KSP.Messages;

namespace Comlines.Commlines
{
    public static class EventListener
    { 
        public static bool isInMapView { get; private set; }

        public static void RegisterEvents()
        {
            var messageCenter = GameManager.Instance.Game.Messages;
            messageCenter.Subscribe<MapInitializedMessage>(OnMapInitialized);
            messageCenter.Subscribe<MapViewLeftMessage>(OnLeftMapView);
        }

        private static void OnMapInitialized(MessageCenterMessage _)
        {
            // force the map to update as each time the map gets initialized all objects get generated again
            isInMapView = true;
            CommlineManager.RefreshLinks();
        }

        private static void OnLeftMapView(MessageCenterMessage msg)
        {
            isInMapView = false;
        }
    }
}

using Commlines.Commlines;
using KSP.Game;
using KSP.Messages;

namespace Comlines.Commlines
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
            CommlineManager.RefreshLinks();
        }

        private static void OnMapViewLeft(MessageCenterMessage _)
        {
            IsInMapView = false;
        }
    }
}

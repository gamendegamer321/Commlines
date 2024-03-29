﻿using CommLines.CommLines;
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
            messageCenter.PersistentSubscribe<VesselChangedMessage>(OnVesselChanged);
        }

        private static void OnMapInitialized(MessageCenterMessage _)
        {
            // force the map to update as each time the map gets initialized all objects get generated again
            IsInMapView = true;

            // Don't do anything if the CommNet lines are disabled
            if (CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.Disabled) return;
            LinkManager.UpdateConnections();
        }

        private static void OnMapViewLeft(MessageCenterMessage _)
        {
            IsInMapView = false;
        }

        private static void OnVesselChanged(MessageCenterMessage _)
        {
            TransmissionMultiplier.SetDirty();
        }
    }
}

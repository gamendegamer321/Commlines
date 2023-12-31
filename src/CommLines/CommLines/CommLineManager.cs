﻿using BepInEx.Logging;
using CommLines.CommNet;
using KSP.Game;
using KSP.Map;
using KSP.Sim.impl;
using UnityEngine;

namespace CommLines.CommLines
{
    public static class CommLineManager
    {
        private const string MapLayer = "Map";
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("CommNet Manager");
        private static readonly List<CommLineConnection> Connections = new();

        private static GameInstance Game => GameManager.Instance.Game;
        private static Dictionary<IGGuid, Map3DFocusItem> MapLookup => _mapCore.map3D.AllMapSelectableItems;
        private static MapCore _mapCore;

        public static void RefreshLinks()
        {
            foreach (var link in LinkManager.Links)
            {
                AddLink(link);
            }
        }

        public static bool AddLink(CommNetLink link)
        {
            // Try to get the map core
            if (_mapCore == null && !Game.Map.TryGetMapCore(out _mapCore))
            {
                Logger.LogError("Could not find map core");
                return false;
            }

            // The first node might be the KSC, in that case we have to swap out the guid (source node uses a separate guid?)
            var guid1 = link.Node1.Owner == LinkManager.SourceGuid ? _mapCore.KSCGUID : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            var mapConnection = GetMapConnection(guid1, guid2);

            // If we already have a line for this link, no need to draw another
            if (mapConnection != null)
            {
                return true;
            }

            // Otherwise create a map connection component and set it up
            var obj = new GameObject("CommNet Connection");
            var connection = obj.gameObject.AddComponent<CommLineConnection>();

            obj.transform.parent = MapLookup[guid1].transform;
            obj.layer = LayerMask.NameToLayer(MapLayer);

            link.Connection = connection;
            
            connection.Setup(MapLookup[guid1], MapLookup[guid2]);
            Connections.Add(connection);

            return true;
        }

        public static void RemoveLink(CommNetLink link)
        {
            // Remove the line if one is already created
            var guid1 = link.Node1.Owner == LinkManager.SourceGuid ? _mapCore.HomeworldGUID : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            GetMapConnection(guid1, guid2)?.Destroy();
        }

        public static void Destroyed(CommLineConnection connection)
        {
            Connections.Remove(connection);
        }

        private static CommLineConnection GetMapConnection(IGGuid comm1, IGGuid comm2)
        {
            foreach (var link in Connections)
            {
                if (link.Source == comm1 && link.Target == comm2)
                {
                    return link;
                }

                if (link.Source == comm2 && link.Target == comm1)
                {
                    return link;
                }
            }

            return null;
        }
    }
}
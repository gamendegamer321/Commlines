using BepInEx.Logging;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;

namespace CommLines.CommLines
{
    public static class CommLineManager
    {
        private const string MapLayer = "Map";
        public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("CommNet Manager");
        private static readonly List<CommLineConnection> Connections = new();

        private static GameInstance Game => GameManager.Instance.Game;

        /// <summary>
        /// Try to create a connection, or get the existing connection if it already exists.
        /// </summary>
        public static bool AddConnection(ConnectionGraphNode node1, ConnectionGraphNode node2, out CommLineConnection connection)
        {
            // Try to get the map core
            if (!Game.Map.TryGetMapCore(out var mapCore))
            {
                Logger.LogError("Could not find map core");
                connection = null;
                return false;
            }

            var mapLookup = mapCore.map3D.AllMapSelectableItems;

            // The node might be kerbal, in that case we have to swap out the guid with the KSC
            var guid1 = node1.IsControlSource ? mapCore.KSCGUID : node1.Owner;
            var guid2 = node2.IsControlSource ? mapCore.KSCGUID : node2.Owner;

            // If we already have a line for this link, no need to draw another
            if (TryGetMapConnection(guid1, guid2, out connection)) return true;

            // Otherwise create a map connection component and set it up
            var obj = new GameObject("CommNet Connection");
            connection = obj.gameObject.AddComponent<CommLineConnection>();

            obj.transform.parent = mapLookup[guid1].transform;
            obj.layer = LayerMask.NameToLayer(MapLayer);

            connection.Setup(mapLookup[guid1], mapLookup[guid2]);
            Connections.Add(connection);

            return true;
        }
        
        public static void RemoveAll()
        {
            foreach (var connection in Connections)
            {
                connection.Destroy();
            }
        }

        public static void RemoveDisconnected(List<CommLineConnection> stillConnected)
        {
            var toRemove = Connections.Where(connection => !stillConnected.Contains(connection)).ToList();
            foreach (var connection in toRemove)
            {
                connection.Destroy();
            }
        }
        
        public static void Destroyed(CommLineConnection connection)
        {
            Connections.Remove(connection);
        }

        private static bool TryGetMapConnection(IGGuid comm1, IGGuid comm2, out CommLineConnection connection)
        {
            foreach (var link in Connections)
            {
                if (link.Source == comm1 && link.Target == comm2)
                {
                    connection = link;
                    return true;
                }

                if (link.Source == comm2 && link.Target == comm1)
                {
                    connection = link;
                    return true;
                }
            }

            connection = null;
            return false;
        }
    }
}
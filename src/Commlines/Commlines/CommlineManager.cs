using BepInEx.Logging;
using Commlines.Commlines;
using KSP.Game;
using KSP.Map;
using KSP.Sim.impl;
using UnityEngine.UIElements.Collections;

namespace Comlines.Commlines
{
    public static class CommlineManager
    {
        private static ManualLogSource logger = Logger.CreateLogSource("Commline Manager");

        private readonly static List<CommnetMapConnection> connections = new List<CommnetMapConnection>();
        private readonly static Dictionary<IGGuid, Map3DFocusItem> mapLookup = new Dictionary<IGGuid, Map3DFocusItem>();

        public static IGGuid kscGuid { get; private set; }

        private static GameInstance Game => GameManager.Instance.Game;

        public static void UpdateMap()
        {
            // We only have to update when something has changed and are actually in the map view
            if (!EventListener.isInMapView)
            {
                return;
            }

            // Try to get the map core
            MapCore core;
            if (!Game.Map.TryGetMapCore(out core))
            {
                logger.LogError("Could not find map core");
                return;
            }

            // Get all icons shown on the map
            var mapObjects = core.map3D.GetComponentsInChildren<Map3DFocusItem>();
            CreateLookup(mapObjects);

            foreach (var link in LinkManager.links)
            {
                var guid1 = link.Node1.Owner == LinkManager.sourceGuid ? kscGuid : link.Node1.Owner;
                var guid2 = link.Node2.Owner;

                var mapConnection = GetMapConnection(guid1, guid2);

                // If we already have a line for this link, no need to draw another
                if (mapConnection != null)
                {
                    continue;
                }

                mapConnection = GetMapConnection(guid1);

                // If we already have a map connection component on this source, we can simply add this line to that map connection component
                if (mapConnection != null)
                {
                    mapConnection.Add(mapLookup.Get(guid2));
                    continue;
                }

                var obj = mapLookup.Get(guid1);
                var connection = obj.gameObject.AddComponent<CommnetMapConnection>();

                connection.Setup(obj, mapLookup.Get(guid2));
                connections.Add(connection);
            }
        }

        public static void Destroyed(CommnetMapConnection connection)
        {
            connections.Remove(connection);
        }

        public static void RemoveLink(CommnetLink link)
        {
            // Remove the line if one is already created
            var guid1 = link.Node1.Owner == LinkManager.sourceGuid ? kscGuid : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            GetMapConnection(guid1, guid2)?.Remove(mapLookup.Get(guid2));
        }

        private static CommnetMapConnection GetMapConnection(IGGuid comm)
        {
            foreach (var link in connections)
            {
                if (link.source == comm)
                {
                    return link;
                }
            }

            return null;
        }

        private static CommnetMapConnection GetMapConnection(IGGuid comm1, IGGuid comm2)
        {
            foreach (var link in connections)
            {
                if (link.source == comm1 && link.targets.Contains(comm2))
                {
                    return link;
                }

                if (link.source == comm2 && link.targets.Contains(comm1))
                {
                    return link;
                }
            }

            return null;
        }        

        private static void CreateLookup(Map3DFocusItem[] mapObjects)
        {
            mapLookup.Clear();

            foreach (var obj in mapObjects)
            {
                mapLookup.Add(obj.AssociatedMapItem.SimGUID, obj);

                if (obj.AssociatedMapItem.ItemName == "kerbin_KSC_Object")
                {
                    kscGuid = obj.AssociatedMapItem.SimGUID;
                }
            }
        }
    }
}

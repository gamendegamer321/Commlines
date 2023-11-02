using BepInEx.Logging;
using Comlines.Commlines;
using KSP.Game;
using KSP.Map;
using KSP.Sim.impl;

namespace Commlines.Commlines
{
    public static class CommlineManager
    {
        private readonly static List<CommnetMapConnection> connections = new List<CommnetMapConnection>();
        private readonly static Dictionary<IGGuid, Map3DFocusItem> mapLookup = new Dictionary<IGGuid, Map3DFocusItem>();

        private static GameInstance Game => GameManager.Instance.Game;
        private static readonly ManualLogSource logger = Logger.CreateLogSource("Commline Manager");
        private static MapCore mapCore;

        public static IGGuid kscGuid { get; private set; }

        public static void RefreshLinks()
        {
            mapLookup.Clear(); // Clear the lookup as the map regenerates each time you open it
            foreach (var link in LinkManager.links)
            {
                AddLink(link);
            }
        }

        public static bool AddLink(CommnetLink link)
        {
            // Try to get the map core
            if (mapCore == null && !Game.Map.TryGetMapCore(out mapCore))
            {
                logger.LogError("Could not find map core");
                return false;
            }

            // The first node might be the KSC, in that case we have to swap out the guid (source node uses a seperate guid)
            var guid1 = link.Node1.Owner == LinkManager.sourceGuid ? kscGuid : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            // Only refresh the lookup if we don't have one of the guids in the lookup
            if (!mapLookup.ContainsKey(guid1) || !mapLookup.ContainsKey(guid2))
            {
                var mapObjects = mapCore.map3D.GetComponentsInChildren<Map3DFocusItem>();
                CreateLookup(mapObjects);

                guid1 = link.Node1.Owner == LinkManager.sourceGuid ? kscGuid : link.Node1.Owner; // We might now know the ksc guid

                // If it still does not exist, we can not add it
                if (!mapLookup.ContainsKey(guid1) || !mapLookup.ContainsKey(guid2))
                {
                    logger.LogWarning($"Could not find an object {guid1} or {guid2}");
                    return false;
                }
            }

            var mapConnection = GetMapConnection(guid1, guid2);

            // If we already have a line for this link, no need to draw another
            if (mapConnection != null)
            {
                return true;
            }

            mapConnection = GetMapConnection(guid1);

            // If we already have a map connection component on this source, we can simply add this line to that map connection component
            if (mapConnection != null)
            {
                mapConnection.Add(mapLookup[guid2]);
                return true;
            }

            // Otherwise create a map connection component and set it up
            var obj = mapLookup[guid1];
            var connection = obj.gameObject.AddComponent<CommnetMapConnection>();

            connection.Setup(obj, mapLookup[guid2]);
            connections.Add(connection);

            return true;
        }

        public static void RemoveLink(CommnetLink link)
        {
            // Remove the line if one is already created
            var guid1 = link.Node1.Owner == LinkManager.sourceGuid ? kscGuid : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            GetMapConnection(guid1, guid2)?.Remove(mapLookup[guid2]);
        }

        public static void Destroyed(CommnetMapConnection connection)
        {
            connections.Remove(connection);
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

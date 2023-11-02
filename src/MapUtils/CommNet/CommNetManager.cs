using BepInEx.Logging;
using KSP.Game;
using KSP.Map;
using KSP.Sim.impl;

namespace MapUtils.CommNet
{
    public static class CommNetManager
    {
        private static readonly List<CommNetMapConnection> Connections = new List<CommNetMapConnection>();
        private static readonly Dictionary<IGGuid, Map3DFocusItem> MapLookup = new Dictionary<IGGuid, Map3DFocusItem>();

        private static GameInstance Game => GameManager.Instance.Game;
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Commline Manager");
        private static MapCore _mapCore;

        private static IGGuid KscGuid { get; set; }

        public static void RefreshLinks()
        {
            MapLookup.Clear(); // Clear the lookup as the map regenerates each time you open it
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

            // The first node might be the KSC, in that case we have to swap out the guid (source node uses a seperate guid)
            var guid1 = link.Node1.Owner == LinkManager.SourceGuid ? KscGuid : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            // Only refresh the lookup if we don't have one of the guids in the lookup
            if (!MapLookup.ContainsKey(guid1) || !MapLookup.ContainsKey(guid2))
            {
                var mapObjects = _mapCore.map3D.GetComponentsInChildren<Map3DFocusItem>();
                CreateLookup(mapObjects);

                guid1 = link.Node1.Owner == LinkManager.SourceGuid ? KscGuid : link.Node1.Owner; // We might now know the ksc guid

                // If it still does not exist, we can not add it
                if (!MapLookup.ContainsKey(guid1) || !MapLookup.ContainsKey(guid2))
                {
                    Logger.LogWarning($"Could not find an object {guid1} or {guid2}");
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
                mapConnection.Add(MapLookup[guid2]);
                return true;
            }

            // Otherwise create a map connection component and set it up
            var obj = MapLookup[guid1];
            var connection = obj.gameObject.AddComponent<CommNetMapConnection>();

            connection.Setup(obj, MapLookup[guid2]);
            Connections.Add(connection);

            return true;
        }

        public static void RemoveLink(CommNetLink link)
        {
            // Remove the line if one is already created
            var guid1 = link.Node1.Owner == LinkManager.SourceGuid ? KscGuid : link.Node1.Owner;
            var guid2 = link.Node2.Owner;

            GetMapConnection(guid1, guid2)?.Remove(MapLookup[guid2]);
        }

        public static void Destroyed(CommNetMapConnection connection)
        {
            Connections.Remove(connection);
        }

        private static CommNetMapConnection GetMapConnection(IGGuid comm)
        {
            foreach (var link in Connections)
            {
                if (link.Source == comm)
                {
                    return link;
                }
            }

            return null;
        }

        private static CommNetMapConnection GetMapConnection(IGGuid comm1, IGGuid comm2)
        {
            foreach (var link in Connections)
            {
                if (link.Source == comm1 && link.Targets.Contains(comm2))
                {
                    return link;
                }

                if (link.Source == comm2 && link.Targets.Contains(comm1))
                {
                    return link;
                }
            }

            return null;
        }

        private static void CreateLookup(Map3DFocusItem[] mapObjects)
        {
            MapLookup.Clear();

            foreach (var obj in mapObjects)
            {
                MapLookup.Add(obj.AssociatedMapItem.SimGUID, obj);

                if (obj.AssociatedMapItem.ItemName == "kerbin_KSC_Object")
                {
                    KscGuid = obj.AssociatedMapItem.SimGUID;
                }
            }
        }
    }
}

using BepInEx.Logging;
using KSP.Game;
using KSP.Map;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine.UIElements.Collections;

namespace Comlines.Commlines
{
    public static class CommlineManager
    {
        private static ManualLogSource logger = Logger.CreateLogSource("Commline Manager");

        private readonly static List<CommnetLink> links = new List<CommnetLink>();
        private readonly static List<CommnetMapConnection> connections = new List<CommnetMapConnection>();
        private readonly static Dictionary<IGGuid, Map3DFocusItem> mapLookup = new Dictionary<IGGuid, Map3DFocusItem>();
        private static bool changed;

        private static IGGuid sourceGuid;
        private static IGGuid kscGuid;

        private static GameInstance Game => GameManager.Instance.Game;

        public static void UpdateConnections(ConnectionGraph graph, List<ConnectionGraphNode> nodes)
        {
            if (!graph.HasResult)
            {
                return;
            }

            List<CommnetLink> currentLinks = new List<CommnetLink>();

            foreach (var node in nodes)
            {
                // If the node is not connected, there will not be a path
                List<ConnectionGraphNode> path = new List<ConnectionGraphNode>();

                if (!graph.TryGetPathFromSourceNode(node, ref path))
                {
                    // If no path was found, continue to the next node
                    continue;
                }

                // Path to the source node itself
                if (path.Count == 1)
                {
                    sourceGuid = path[0].Owner;
                    continue;
                }

                // We start at 1, so we can get the current node and the previous node when going over the path
                for (int i = 1; i < path.Count; i ++)
                {
                    var previousNode = path[i - 1];
                    var currentNode = path[i];

                    var link = GetLink(previousNode, currentNode);

                    if (link != null) // If the link already exists, no need to create a new link
                    {
                        if (!currentLinks.Contains(link))
                        {
                            currentLinks.Add(link);
                        }
                        
                        continue;
                    }

                    link = new CommnetLink(previousNode, currentNode);
                    currentLinks.Add(link);
                    links.Add(link);

                    changed = true;
                }
            }

            RemoveUnusedLinks(currentLinks);
            UpdateMap(false);
        }

        public static void UpdateMap(bool forceUpdate)
        {
            // We only have to update when something has changed and are actually in the map view
            if ((!changed && !forceUpdate) || !EventListener.isInMapView)
            {
                return;
            }

            changed = false;

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

            foreach (var link in links)
            {
                var guid1 = link.Node1.Owner == sourceGuid ? kscGuid : link.Node1.Owner;
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
                    logger.LogInfo($"Adding {guid2} to {mapConnection.source}");
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

        private static CommnetLink GetLink(ConnectionGraphNode comm1, ConnectionGraphNode comm2)
        {
            foreach (var link  in links)
            {
                if (link.Node1.Owner == comm1.Owner && link.Node2.Owner == comm2.Owner)
                {
                    return link;
                }

                if (link.Node1.Owner == comm2.Owner && link.Node2.Owner == comm1.Owner)
                {
                    return link;
                }
            }

            return null;
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

        private static void RemoveUnusedLinks(List<CommnetLink> stillInUse)
        {
            List<CommnetLink> toRemove = new List<CommnetLink>();

            foreach (var link in links)
            {
                if (!stillInUse.Contains(link))
                {
                    toRemove.Add(link);
                }
            }

            if (toRemove.Count > 0)
            {
                changed = true;
            }

            foreach (var link in toRemove)
            {
                links.Remove(link);
            }
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

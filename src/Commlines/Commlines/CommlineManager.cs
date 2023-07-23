using BepInEx.Logging;
using KSP.Game;
using KSP.Map;
using KSP.Sim;
using KSP.Sim.impl;
using Unity.Mathematics;
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

        public static void UpdateConnections(List<ConnectionGraphNode> nodes, ConnectionGraphNode sourceNode)
        {
            if (!EventListener.isInMapView)
            {
                return;
            }

            List<CommnetLink> currentLinks = new List<CommnetLink>();
            sourceGuid = sourceNode.Owner;

            for (int i = 0; i < nodes.Count; i++)
            {
                var currentNode = nodes[i];

                if (!currentNode.IsActive) // If the node is not active, it can not be connected
                {
                    continue;
                }

                var maxDistance1 = currentNode.MaxRange * currentNode.MaxRange; // Calculate here so we don't have to do it within the next loop

                // We start one higher than the previous as this is the first node to check it against
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var nextNode = nodes[j];

                    if (!nextNode.IsActive)
                    {
                        continue;
                    }

                    if (!IsValidConnection(currentNode, nextNode, maxDistance1)) // No need to check anything if they are not connected
                    {
                        continue;
                    }

                    var link = GetLink(currentNode.Owner, nextNode.Owner);

                    if (link != null) // If the link already exists, no need to create a new link
                    {
                        if (!currentLinks.Contains(link))
                        {
                            currentLinks.Add(link);
                        }
                        
                        continue;
                    }

                    link = new CommnetLink(currentNode, nextNode);
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
                    mapConnection.Add(mapLookup.Get(guid2));
                    continue;
                }

                var obj = mapLookup.Get(guid1);
                var connection = obj.gameObject.AddComponent<CommnetMapConnection>();

                connection.Setup(obj, mapLookup.Get(guid2));
                connections.Add(connection);
            }
        }

        public static bool IsStillValid(IGGuid comm1, IGGuid comm2)
        {
            return GetLink(comm1, comm2) != null;
        }

        public static void Destroyed(CommnetMapConnection connection)
        {
            connections.Remove(connection);
        }

        private static CommnetLink GetLink(IGGuid comm1, IGGuid comm2)
        {
            foreach (var link in links)
            {
                if (link.Node1.Owner == comm1 && link.Node2.Owner == comm2)
                {
                    return link;
                }

                if (link.Node1.Owner == comm2 && link.Node2.Owner == comm1)
                {
                    return link;
                }
            }


            // If its the ksc also check if it still has a valid connection to the source node
            if (comm1 == kscGuid)
            {
                return GetLink(sourceGuid, comm2);
            }

            if (comm2 == kscGuid)
            {
                return GetLink(comm1, sourceGuid);
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

                    // Remove the line if one is already created
                    GetMapConnection(link.Node1.Owner)?.Remove(mapLookup.Get(link.Node2.Owner));
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

        private static bool IsValidConnection(ConnectionGraphNode node1, ConnectionGraphNode node2, double maxDistance1)
        {
            var distance = math.distancesq(node1.Position, node2.Position);

            logger.LogInfo($"{node1.Owner} {node2.Owner} Distance: {distance}, range 1 {maxDistance1}, range 2 {node2.MaxRange * node2.MaxRange}, output {distance < maxDistance1 || distance < node2.MaxRange * node2.MaxRange}");

            return distance < maxDistance1 || distance < node2.MaxRange * node2.MaxRange;
        }
    }
}

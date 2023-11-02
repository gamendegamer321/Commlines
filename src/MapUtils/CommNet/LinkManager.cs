using KSP.Sim;
using KSP.Sim.impl;
using Unity.Mathematics;

namespace MapUtils.CommNet
{
    public static class LinkManager
    {
        private static List<ConnectionGraphNode> _nodes = new List<ConnectionGraphNode>();
        private static ConnectionGraph _graph;
        private static bool _updatingGraph;

        public static readonly List<CommNetLink> Links = new List<CommNetLink>();
        public static IGGuid SourceGuid { get; private set; }

        public static void RefreshingCommnet(ConnectionGraph currentGraph, List<ConnectionGraphNode> currentNodes, ConnectionGraphNode sourceNode)
        {
            // Store everything to use when the graph is done updating
            _graph = currentGraph;
            _nodes = currentNodes;
            SourceGuid = sourceNode.Owner;
            _updatingGraph = true;
        }

        public static void UpdateConnections()
        {
            // We only want to update the connection after the game has updated it's CommNet connections.
            if (!EventListener.IsInMapView || !_updatingGraph || !_graph.HasResult)
            {
                return;
            }

            _updatingGraph = false;

            List<CommNetLink> currentLinks = MapUtilsPlugin.ConfigEntry.Value ? GeneratePaths(_graph, _nodes) : GenerateAllConnections(_nodes);

            RemoveUnusedLinks(currentLinks);
        }

        private static List<CommNetLink> GenerateAllConnections(List<ConnectionGraphNode> nodes)
        {
            List<CommNetLink> currentLinks = new List<CommNetLink>();

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

                    link = new CommNetLink(currentNode, nextNode);

                    // Only add it to the discovered links if it has been successfully placed on the map
                    if (CommNetManager.AddLink(link))
                    {
                        currentLinks.Add(link);
                        Links.Add(link);
                    }
                }
            }

            return currentLinks;
        }

        private static List<CommNetLink> GeneratePaths(ConnectionGraph graph, List<ConnectionGraphNode> nodes)
        {
            List<CommNetLink> currentLinks = new List<CommNetLink>();

            foreach (var node in nodes)
            {
                List<ConnectionGraphNode> path = new List<ConnectionGraphNode>();
                if (!graph.TryGetPathFromSourceNode(node, ref path))
                {
                    // If no path was found, continue to the next node
                    continue;
                }

                // We start at 1, so we can get the current node and the previous node when going over the path
                for (int i = 1; i < path.Count; i++)
                {
                    var previousNode = path[i - 1];
                    var currentNode = path[i];

                    var link = GetLink(previousNode.Owner, currentNode.Owner);

                    if (link != null) // If the link already exists, no need to create a new link
                    {
                        if (!currentLinks.Contains(link))
                        {
                            currentLinks.Add(link);
                        }

                        continue;
                    }

                    link = new CommNetLink(previousNode, currentNode);

                    // Only add it to the discovered links if it has been successfully placed on the map
                    if (CommNetManager.AddLink(link))
                    {
                        currentLinks.Add(link);
                        Links.Add(link);
                    }
                }
            }

            return currentLinks;
        }

        private static void RemoveUnusedLinks(List<CommNetLink> stillInUse)
        {
            List<CommNetLink> toRemove = new List<CommNetLink>();

            foreach (var link in Links)
            {
                if (!stillInUse.Contains(link))
                {
                    toRemove.Add(link);

                    CommNetManager.RemoveLink(link);
                }
            }

            foreach (var link in toRemove)
            {
                Links.Remove(link);
            }
        }

        private static bool IsValidConnection(ConnectionGraphNode node1, ConnectionGraphNode node2, double maxDistance1)
        {
            var distance = math.distancesq(node1.Position, node2.Position);

            return distance < maxDistance1 || distance < node2.MaxRange * node2.MaxRange;
        }

        private static CommNetLink GetLink(IGGuid comm1, IGGuid comm2)
        {
            foreach (var link in LinkManager.Links)
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

            return null;
        }
    }
}

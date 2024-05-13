using CommLines.CommLines;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CommLines.CommNet
{
    public static class LinkManager
    {
        private static ViewController ViewController => GameManager.Instance.Game.ViewController;

        public static List<ConnectionGraphNode> Nodes;

        private static ConnectionGraph _graph;
        private static NativeArray<ConnectionGraph.ConnectionGraphJobNode> _jobNodes;
        private static NativeArray<int> _previousIndices;

        private static bool _isRefreshing;

        /// <summary>
        /// Whenever the CommNet starts refreshing, store all the information of the CommNet
        /// so we can calculate the connectivity or all connections after
        /// </summary>
        public static void RefreshingCommnet(ConnectionGraph graph, List<ConnectionGraphNode> nodes,
            NativeArray<ConnectionGraph.ConnectionGraphJobNode> jobNodes, NativeArray<int> previousIndices)
        {
            Nodes = nodes;

            _graph = graph;
            _jobNodes = jobNodes;
            _previousIndices = previousIndices;

            _isRefreshing = true;
        }

        public static void OnUpdate()
        {
            if (!_isRefreshing || !_graph.HasResult) return;

            // When the game has finished updating the CommNet it's our turn
            CommNetJobHandler.CalculateNetwork(Nodes, _jobNodes, _previousIndices);
            _isRefreshing = false;
        }

        public static void UpdateConnections()
        {
            // Only need to update this when we are in mapview, the previousIndices array is up to date and have CommLines enabled
            if (!EventListener.IsInMapView || !_graph.HasResult ||
                CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.Disabled) return;

            var currentLinks = CommLinesPlugin.CommNetModeEntry.Value switch
            {
                CommLineMode.Hop => GenerateHop(),
                CommLineMode.VesselLinks => GenerateVesselLinks(),
                CommLineMode.ActivePathOnly => GenerateActivePath(),
                CommLineMode.PathOnly => GeneratePaths(),
                CommLineMode.All => GenerateAllConnections(),
                _ => []
            };

            CommLineManager.RemoveDisconnected(currentLinks);
        }

        public static List<ConnectionGraphNode> GetPath(IGGuid guid)
        {
            List<ConnectionGraphNode> path = new();

            foreach (var node in Nodes.Where(node => node.Owner == guid))
            {
                _graph.TryGetPathFromSourceNode(node, ref path);
                break;
            }

            return path;
        }

        private static List<CommLineConnection> GenerateAllConnections()
        {
            var currentConnections = new List<CommLineConnection>();

            for (var i = 0; i < Nodes.Count; i++)
            {
                var currentNode = Nodes[i];

                if (!currentNode.IsActive) // If the node is not active, it can not be connected
                {
                    continue;
                }

                // We start one higher than the previous as this is the first node to check it against
                for (var j = i + 1; j < Nodes.Count; j++)
                {
                    var connectivity = CommNetJobHandler.GetConnectivity(i, j);

                    if (connectivity <= 0) continue;
                    if (!CommLineManager.AddConnection(currentNode, Nodes[j], out var connection)) continue;
                    currentConnections.Add(connection);
                    connection.SetColor(GetColorForConnectivity(connectivity));
                }
            }

            return currentConnections;
        }

        private static List<CommLineConnection> GeneratePaths()
        {
            var currentConnections = new List<CommLineConnection>();

            for (var i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                var previousIndex = _previousIndices[i];

                if (previousIndex < 0 || previousIndex > Nodes.Count) continue;

                var previousNode = Nodes[previousIndex];

                var connectivity = CommNetJobHandler.GetConnectivity(i);
                var color = GetColorForConnectivity(connectivity);

                // Only add it to the discovered links if it has been successfully placed on the map
                if (!CommLineManager.AddConnection(node, previousNode, out var connection)) continue;

                currentConnections.Add(connection);
                connection.SetColor(color);
            }

            return currentConnections;
        }

        private static List<CommLineConnection> GenerateActivePath()
        {
            var currentConnections = new List<CommLineConnection>();
            if (!TryGetSourceId(out var guid)) return currentConnections;

            var currentNode = Nodes.FirstOrDefault(node => node.Owner == guid);
            if (currentNode == null) return currentConnections;

            // Move over all nodes in the path from the active vessel to the KSC
            var nodeIndex = Nodes.IndexOf(currentNode);
            while (!currentNode.IsControlSource)
            {
                var previousIndex = _previousIndices[nodeIndex];
                if (previousIndex < 0 || previousIndex > Nodes.Count) return currentConnections;

                var previousNode = Nodes[previousIndex];
                var connectivity = CommNetJobHandler.GetConnectivity(nodeIndex);
                var color = GetColorForConnectivity(connectivity);

                if (!CommLineManager.AddConnection(currentNode, previousNode, out var connection)) continue;

                currentConnections.Add(connection);
                connection.SetColor(color);

                nodeIndex = previousIndex;
                currentNode = previousNode;
            }

            return currentConnections;
        }

        private static List<CommLineConnection> GenerateVesselLinks()
        {
            var currentConnections = new List<CommLineConnection>();
            if (!TryGetSourceId(out var guid)) return currentConnections;

            var currentNode = Nodes.FirstOrDefault(node => node.Owner == guid);
            if (currentNode == null) return currentConnections;

            // Loop over all other nodes and check if both are in range
            foreach (var otherNode in Nodes.Where(node => node != currentNode))
            {
                var connectivity = CommNetJobHandler.GetConnectivity(currentNode, otherNode);

                if (connectivity <= 0) continue;
                if (!CommLineManager.AddConnection(currentNode, otherNode, out var connection)) continue;
                currentConnections.Add(connection);
                connection.SetColor(GetColorForConnectivity(connectivity));
            }

            return currentConnections;
        }

        private static List<CommLineConnection> GenerateHop()
        {
            if (!TryGetSourceId(out var guid)) return [];

            var currentNode = Nodes.FirstOrDefault(node => node.Owner == guid);
            if (currentNode == null) return [];

            var nodeIndex = Nodes.IndexOf(currentNode);

            // Get the first hop for this node
            var previousIndex = _previousIndices[nodeIndex];
            if (previousIndex < 0 || previousIndex > Nodes.Count) return []; // Skip if nothing was found

            // Get the node information
            var previousNode = Nodes[previousIndex];
            var connectivity = CommNetJobHandler.GetConnectivity(nodeIndex);
            var color = GetColorForConnectivity(connectivity);

            if (!CommLineManager.AddConnection(currentNode, previousNode, out var connection)) return [];
            connection.SetColor(color);

            return [connection];
        }

        private static bool TryGetSourceId(out IGGuid guid)
        {
            if (!ViewController.TryGetActiveSimVessel(out var simVessel))
            {
                guid = IGGuid.Empty;
                return false;
            }

            guid = simVessel.SimulationObject.GlobalId;
            return true;
        }

        private static Color GetColorForConnectivity(float connectivity) => Color.HSVToRGB(connectivity / 3f, 1, 1);
    }
}
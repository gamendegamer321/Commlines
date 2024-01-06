using CommLines.CommLines;
using KSP.Sim;
using KSP.Sim.impl;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CommLines.CommNet
{
    public static class LinkManager
    {
        private static ConnectionGraph _graph;
        private static List<ConnectionGraphNode> _nodes;
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
            _graph = graph;
            _nodes = nodes;
            _jobNodes = jobNodes;
            _previousIndices = previousIndices;

            _isRefreshing = true;
        }

        public static void OnUpdate()
        {
            if (!_isRefreshing || !_graph.HasResult) return;

            // When the game has finished updating the CommNet its our turn
            CommNetJobHandler.CalculateNetwork(_nodes, _jobNodes, _previousIndices);
            _isRefreshing = false;
        }

        public static void UpdateConnections()
        {
            // Only need to update this when we are in mapview, the previousIndices array is up to date and have CommLines enabled
            if (!EventListener.IsInMapView || !_graph.HasResult ||
                CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.Disabled) return;

            var currentLinks = CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.PathOnly
                ? GeneratePaths()
                : GenerateAllConnections();

            CommLineManager.RemoveDisconnected(currentLinks);
        }

        private static List<CommLineConnection> GenerateAllConnections()
        {
            var currentConnections = new List<CommLineConnection>();

            for (var i = 0; i < _nodes.Count; i++)
            {
                var currentNode = _nodes[i];

                if (!currentNode.IsActive) // If the node is not active, it can not be connected
                {
                    continue;
                }

                // Calculate here so we don't have to do it within the next loop
                var maxDistance = currentNode.MaxRange * currentNode.MaxRange;

                // We start one higher than the previous as this is the first node to check it against
                for (var j = i + 1; j < _nodes.Count; j++)
                {
                    var nextNode = _nodes[j];

                    if (!nextNode.IsActive) // If the node is inactive, it can never connect
                    {
                        continue;
                    }

                    if (!IsValidConnection(currentNode, nextNode,
                            maxDistance, out Color color)) // No need to check anything if they are not connected
                    {
                        continue;
                    }

                    // Try to create or get the connection, if that's not possible go to the next
                    if (!CommLineManager.AddConnection(currentNode, nextNode, out var connection)) continue;

                    currentConnections.Add(connection);
                    connection.SetColor(color);
                }
            }

            return currentConnections;
        }

        private static List<CommLineConnection> GeneratePaths()
        {
            var currentConnections = new List<CommLineConnection>();

            for (var i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                var previousIndex = _previousIndices[i];

                if (previousIndex < 0 || previousIndex > _nodes.Count) continue;

                var previousNode = _nodes[previousIndex];

                var connectivity = CommNetJobHandler.GetConnectivity(i);
                var color = GetColorForConnectivity(connectivity);

                // Only add it to the discovered links if it has been successfully placed on the map
                if (!CommLineManager.AddConnection(node, previousNode, out var connection)) continue;

                currentConnections.Add(connection);
                connection.SetColor(color);
            }

            return currentConnections;
        }

        private static bool IsValidConnection(ConnectionGraphNode node1, ConnectionGraphNode node2, double maxDistance,
            out Color color)
        {
            var distance = math.distancesq(node1.Position, node2.Position);
            var maxDistance2 = node2.MaxRange * node2.MaxRange;
            color = Color.Lerp(Color.green, Color.red, (float)(distance / Math.Min(maxDistance, maxDistance2)));

            return distance < maxDistance && distance < maxDistance2;
        }

        private static Color GetColorForConnectivity(float connectivity) => Color.HSVToRGB(connectivity / 3f, 1, 1);
    }
}
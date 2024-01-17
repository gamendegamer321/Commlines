using CommLines.CommLines;
using CommLines.Jobs;
using KSP.Sim;
using Unity.Collections;
using Unity.Jobs;

namespace CommLines.CommNet;

public static class CommNetJobHandler
{
    private static bool _running;

    private static JobHandle _jobHandle;

    private static int _nodeCount;
    private static bool _calculatedUsingPath;
    private static NativeArray<float> _speedToPrevious;

    public static void OnUpdate()
    {
        if (!_running || !_jobHandle.IsCompleted) return;

        _jobHandle.Complete();
        _running = false;

        // After our job is complete, update the visual lines
        LinkManager.UpdateConnections();
        
        // Inform the transmission multiplier there might be a new connectivity value
        TransmissionMultiplier.SetDirty();
    }

    public static void CalculateNetwork(List<ConnectionGraphNode> nodes,
        NativeArray<ConnectionGraph.ConnectionGraphJobNode> nativeNodes, NativeArray<int> previousNodes)
    {
        if (_running) return;

        _calculatedUsingPath = CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.PathOnly;
        CreateArrays(nodes.Count, _calculatedUsingPath);

        _running = true;
        _nodeCount = nodes.Count;

        if (_calculatedUsingPath)
        {
            _jobHandle = new PathConnectivityJob
            {
                Nodes = nativeNodes,
                PrevNode = previousNodes,
                ConnectionSpeed = _speedToPrevious
            }.Schedule();
        }
        else
        {
            _jobHandle = new AllConnectionsJob
            {
                Nodes = nativeNodes,
                SpeedToPrevious = _speedToPrevious
            }.Schedule();
        }
    }

    /// <summary>
    /// Get the connectivity between two nodes, only available in ALL mode.
    /// </summary>
    public static float GetConnectivity(int node1, int node2)
    {
        if (_running || _calculatedUsingPath || !_speedToPrevious.IsCreated) return -1.0f;
        var index = GetTableIndex(node1, node2);
        if (index < 0 || index > _speedToPrevious.Length) return 0.0f;
        return _speedToPrevious[index];
    }

    /// <summary>
    /// Get the connectivity from this node to it's previous node, only available in PATH mode
    /// </summary>
    public static float GetConnectivity(int index)
    {
        if (_running || !_calculatedUsingPath || !_speedToPrevious.IsCreated) return -1.0f;
        if (index < 0 || index >= _speedToPrevious.Length) return 0.0f;
        return _speedToPrevious[index];
    }

    public static float GetConnectivity(ConnectionGraphNode node1, ConnectionGraphNode node2)
        => GetConnectivity(LinkManager.Nodes.IndexOf(node1), LinkManager.Nodes.IndexOf(node2));

    public static float GetConnectivity(ConnectionGraphNode node)
        => GetConnectivity(LinkManager.Nodes.IndexOf(node));

    public static void DisposeAll()
    {
        if (_speedToPrevious.IsCreated) _speedToPrevious.Dispose();
    }

    private static void CreateArrays(int nodeCount, bool pathMode)
    {
        DisposeAll();

        var tableNodeCount = pathMode ? nodeCount : GetAllTableSize(nodeCount);
        _speedToPrevious = new NativeArray<float>(tableNodeCount, Allocator.Persistent);
    }

    /// <summary>
    /// Get the table index using the indices of the two nodes. This can be used for the connectivity table.
    /// </summary>
    private static int GetTableIndex(int i, int j)
    {
        if (i == j) return -1;
        if (i > j) // Swap the values so that i < j
        {
            (i, j) = (j, i);
        }

        j--;
        return i * (_nodeCount - 1) - i * (i + 1) / 2 + j;
    }

    private static int GetAllTableSize(int n) => (n * n - n) / 2;
}
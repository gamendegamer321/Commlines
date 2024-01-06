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
    
    private static NativeArray<int> _previousNodes; // The previous node when using ALL
    private static NativeArray<float> _speedToPrevious;

    public static void OnUpdate()
    {
        if (!_running || !_jobHandle.IsCompleted) return;
        
        _jobHandle.Complete();
        _running = false;
        
        // After our job is complete, update the visual lines
        LinkManager.UpdateConnections();
    }

    public static void CalculateNetwork(List<ConnectionGraphNode> nodes,
        NativeArray<ConnectionGraph.ConnectionGraphJobNode> nativeNodes, NativeArray<int> previousNodes)
    {
        if (_running) return;
        
        var usingPathMode = CommLinesPlugin.CommNetModeEntry.Value == CommLineMode.PathOnly;
        CreateArrays(nodes.Count, usingPathMode);
        
        _running = true;
        
        if (!usingPathMode) return;
        
        _jobHandle = new PathConnectivityJob
        {
            Nodes = nativeNodes,
            PrevNode = previousNodes,
            ConnectionSpeed = _speedToPrevious
        }.Schedule();
    }
    
    public static float GetConnectivity(int index)
    {
        if (_running) return 0.0f;
        if (index < 0 || index >= _speedToPrevious.Length) return 0.0f;
        return _speedToPrevious[index];
    }
    
    public static void DisposeAll()
    {
        if (_previousNodes.IsCreated) _previousNodes.Dispose();
        if (_speedToPrevious.IsCreated) _speedToPrevious.Dispose();
    }

    private static void CreateArrays(int nodeCount, bool pathMode)
    {
        DisposeAll();
        
        var tableNodeCount = pathMode ? nodeCount : GetAllTableSize(nodeCount);
        _speedToPrevious = new NativeArray<float>(tableNodeCount, Allocator.Persistent);

        // We only calculate our own when not using path mode
        if (!pathMode) _previousNodes = new NativeArray<int>(tableNodeCount, Allocator.Persistent);
    }

    private static int GetAllTableSize(int n) => (n * n - n) / 2;
}
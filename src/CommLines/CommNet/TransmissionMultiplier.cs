using CommLines.CommLines;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;

namespace CommLines.CommNet;

public static class TransmissionMultiplier
{
    private static ViewController ViewController => GameManager.Instance.Game.ViewController;

    private static float _cachedValue = 1f;
    private static bool _isDirty;

    public static float GetMultiplier()
    {
        // If the system is disabled, simply return 1
        if (!CommLinesPlugin.TransmissionMultiplier.Value)
        {
            return 1f;
        }
        
        // If the cached value is dirty, update it
        if (_isDirty)
        {
            Update();
        }

        return Math.Clamp(_cachedValue, 0.05f, 1f);
    }

    public static void SetDirty()
    {
        _isDirty = true;
    }

    private static void Update()
    {
        // Make sure there is an active vessel
        if (!ViewController.TryGetActiveSimVessel(out var simVessel))
        {
            _cachedValue = 1f; // If there is no active vessel, set cached value to full connectivity
            return;
        }

        var guid = simVessel.SimulationObject.GlobalId;
        var path = LinkManager.GetPath(guid);

        if (path == null || path.Count == 0) // Make sure we actually got a path, otherwise set to 1
        {
            _cachedValue = 1f;
            return;
        }

        if (CommLinesPlugin.CommNetModeEntry.Value != CommLineMode.PathOnly)
        {
            CalculateForAll(path);
        }
        else
        {
            CalculateForPath(path);
        }

        _isDirty = false;
    }

    private static void CalculateForPath(IEnumerable<ConnectionGraphNode> path)
    {
        var currentLowest = 1f;

        foreach (var connectivity in path.Select(CommNetJobHandler.GetConnectivity)
                     .Where(connectivity => connectivity < currentLowest && connectivity >= 0))
        {
            currentLowest = connectivity;
        }

        _cachedValue = currentLowest;
    }

    private static void CalculateForAll(IReadOnlyList<ConnectionGraphNode> path)
    {
        var currentLowest = 1f;

        for (var i = 1; i < path.Count; i++)
        {
            var connectivity = CommNetJobHandler.GetConnectivity(path[i - 1], path[i]);

            if (connectivity < currentLowest && connectivity >= 0)
            {
                currentLowest = connectivity;
            }
        }

        _cachedValue = currentLowest;
    }
}
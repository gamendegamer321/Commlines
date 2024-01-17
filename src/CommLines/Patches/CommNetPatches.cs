using HarmonyLib;
using KSP.Sim;
using CommLines.CommNet;
using KSP.Game;
using KSP.Sim.impl;
using Unity.Collections;

namespace CommLines.Patches
{
    public static class CommNetPatches
    {
        // Add a postfix for each time the CommNet connections get checked
        [HarmonyPatch(typeof(ConnectionGraph), "RebuildConnectionGraph")]
        [HarmonyPostfix]
        public static void OnUpdatePostfix(ConnectionGraph __instance)
        {
            var traverse = Traverse.Create(__instance);

            var nodes = traverse.Field("_allNodes").GetValue() as List<ConnectionGraphNode>;
            var jobNodes = (NativeArray<ConnectionGraph.ConnectionGraphJobNode>)traverse.Field("_nodes").GetValue();
            var previousIndices = (NativeArray<int>)traverse.Field("_previousIndices").GetValue();

            if (nodes == null) return;

            LinkManager.RefreshingCommnet(__instance, nodes, jobNodes, previousIndices);
        }

        [HarmonyPatch(typeof(SessionManager), "ShutdownSession")]
        [HarmonyPostfix]
        public static void SessionShutdown()
        {
            CommNetJobHandler.DisposeAll(); // Dispose everything on shutdown of the session
        }

        [HarmonyPatch(typeof(PartComponentModule_DataTransmitter), "DataPacketSize", MethodType.Getter)]
        [HarmonyPostfix]
        public static void GetMultipliedPacketSize(ref float __result)
        {
            // If the system is disabled, don't do anything
            if (!CommLinesPlugin.TransmissionMultiplier.Value) return;

            var newValue = __result * TransmissionMultiplier.GetMultiplier();
            __result = newValue;
        }
    }
}
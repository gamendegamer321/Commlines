using HarmonyLib;
using KSP.Sim;
using CommLines.CommNet;
using Unity.Collections;

namespace CommLines.Patches
{
    [HarmonyPatch(typeof(ConnectionGraph))]
    [HarmonyPatch("RebuildConnectionGraph")]
    public static class CommNetPatch
    {
        // Add a postfix for each time the CommNet connections get checked
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
    }
}

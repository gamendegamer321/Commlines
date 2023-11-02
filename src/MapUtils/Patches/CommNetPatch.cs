using HarmonyLib;
using KSP.Sim;
using MapUtils.CommNet;

namespace MapUtils.Patches
{
    [HarmonyPatch(typeof(ConnectionGraph))]
    [HarmonyPatch("RebuildConnectionGraph")]
    public class CommNetPatch
    {
        // Add a postfix for each time the CommNet connections get checked
        [HarmonyPostfix]
        public static void OnUpdatePostfix(ConnectionGraph __instance)
        {
            var traverse = Traverse.Create(__instance);
            var nodes = traverse.Field("_allNodes").GetValue() as List<ConnectionGraphNode>; // Get all nodes in the _allNodes list
            var sourceIndex = (int)traverse.Field("_prevSourceIndex").GetValue();
            LinkManager.RefreshingCommnet(__instance, nodes, nodes[sourceIndex]); // Tell the LinkManager the CommNet is being refreshed
        }
    }
}

using HarmonyLib;
using KSP.Sim;

namespace Comlines.Commlines
{
    [HarmonyPatch(typeof(ConnectionGraph))]
    [HarmonyPatch("OnUpdate")]
    public class CommlinesPatch
    {
        [HarmonyPostfix]
        public static void OnUpdatePostfix(ConnectionGraph __instance)
        {
            var traverse = Traverse.Create(__instance);
            var nodes = traverse.Field("_allNodes").GetValue() as List<ConnectionGraphNode>;
            var sourceIndex =(int)traverse.Field("_prevSourceIndex").GetValue();
            CommlineManager.UpdateConnections(nodes, nodes[sourceIndex]);
        }
    }
}

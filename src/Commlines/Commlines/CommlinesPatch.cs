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
            var nodes = Traverse.Create(__instance).Field("_allNodes").GetValue() as List<ConnectionGraphNode>;
            CommlineManager.UpdateConnections(__instance, nodes);
        }
    }
}

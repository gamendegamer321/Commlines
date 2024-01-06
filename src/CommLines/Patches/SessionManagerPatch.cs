using CommLines.CommNet;
using HarmonyLib;
using KSP.Game;

namespace CommLines.Patches;

[HarmonyPatch(typeof(SessionManager))]
public static class SessionManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("ShutdownSession")]
    public static void SessionShutdown()
    {
        CommNetJobHandler.DisposeAll(); // Dispose everything on shutdown of the session
    }
}
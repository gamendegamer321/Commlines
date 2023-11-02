using BepInEx;
using BepInEx.Configuration;
using Comlines.Commlines;
using Commlines.Commlines;
using HarmonyLib;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace MapUtils;

[BepInPlugin("com.gamendegamer.maputils", "MapUtils", "1.0.1")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class MapUtilsPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    // ReSharper disable UnusedMember.Global
    public const string ModGuid = "com.gamendegamer.commlines";
    public const string ModName = "MapUtils";
    public const string ModVer = "1.0.1";
    // ReSharper restore UnusedMember.Global

    // Singleton instance of the plugin class
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static MapUtilsPlugin Instance { get; set; }

    public static ConfigEntry<bool> ConfigEntry { get; private set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        // Create the patch
        Harmony.CreateAndPatchAll(typeof(CommlinesPatch));

        // Start the event listener
        EventListener.RegisterEvents();

        ConfigEntry = Config.Bind("MapUtils Section", "Use path", false, "Only display commnet paths to the source node, instead of all paths between all vessels.");

        Logger.LogInfo($"Initialized MapUtils");
    }

    public void Update()
    {
        LinkManager.UpdateConnections();
    }
}

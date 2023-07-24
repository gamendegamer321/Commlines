using BepInEx;
using BepInEx.Configuration;
using Comlines.Commlines;
using Commlines.Commlines;
using HarmonyLib;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace Comlines;

[BepInPlugin("com.gamendegamer.commlines", "Commlines", "1.0.1")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommlinesPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    // ReSharper disable UnusedMember.Global
    public const string ModGuid = "com.gamendegamer.commlines";
    public const string ModName = "Commlines";
    public const string ModVer = "1.0.1";
    // ReSharper restore UnusedMember.Global

    // Singleton instance of the plugin class
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static CommlinesPlugin Instance { get; set; }

    public static ConfigEntry<bool> configEntry { get; private set; }

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

        configEntry = Config.Bind("Commlines Section", "Use path", false, "Only display commnet paths to the source node, instead of all paths between all vessels.");

        Logger.LogInfo($"Initialized commlines");
    }

    public void Update()
    {
        LinkManager.UpdateConnections();
    }
}

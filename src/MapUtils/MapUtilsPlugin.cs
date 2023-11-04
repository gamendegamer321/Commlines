using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MapUtils.CommNet;
using MapUtils.Patches;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace MapUtils;

[BepInPlugin("com.gamendegamer.maputils", "MapUtils", "1.0.1")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class MapUtilsPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    // ReSharper disable UnusedMember.Global
    public const string ModGuid = "com.gamendegamer.maputils";
    public const string ModName = "MapUtils";
    public const string ModVer = "1.0.1";
    // ReSharper restore UnusedMember.Global

    // Singleton instance of the plugin class
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static MapUtilsPlugin Instance { get; set; }

    public static ConfigEntry<CommNetMode> CommNetModeEntry { get; private set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        // Create the patch
        Harmony.CreateAndPatchAll(typeof(CommNetPatch));

        // Start the event listener
        EventListener.RegisterEvents();

        CommNetModeEntry = Config.Bind("MapUtils Section", "Use path", CommNetMode.All,
            "Set the display mode for the CommNet lines");
        CommNetModeEntry.SettingChanged += OnUpdateCommNetMode;

        Logger.LogInfo($"Initialized MapUtils");
    }

    public void Update()
    {
        if (CommNetModeEntry.Value != CommNetMode.Disabled)
        {
            LinkManager.UpdateConnections();
        }
    }

    private void OnUpdateCommNetMode(object entry, EventArgs _)
    {
        var configEntry = (ConfigEntry<CommNetMode>)entry;

        if (configEntry.Value != CommNetMode.Disabled) return;

        // Remove all links
        foreach (var link in LinkManager.Links)
        {
            CommNetManager.RemoveLink(link);
        }

        LinkManager.Links.Clear();
    }
}
using BepInEx;
using BepInEx.Configuration;
using CommLines.CommLines;
using HarmonyLib;
using CommLines.CommNet;
using CommLines.Patches;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace CommLines;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommLinesPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    // ReSharper disable UnusedMember.Global
    public const string ModGuid = "com.gamendegamer.commlines";
    public const string ModName = "CommLines";
    public const string ModVer = "1.0.2";
    // ReSharper restore UnusedMember.Global

    // Singleton instance of the plugin class
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static CommLinesPlugin Instance { get; set; }

    public static ConfigEntry<CommLineMode> CommNetModeEntry { get; private set; }

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

        CommNetModeEntry = Config.Bind("CommNet Utils Section", "Use path", CommLineMode.All,
            "Set the display mode for the CommNet lines");
        CommNetModeEntry.SettingChanged += OnUpdateCommNetMode;
        
        Logger.LogInfo("Creating materials");
        PluginMaterials.GenerateMaterials();

        Logger.LogInfo($"Initialized CommNet Utils");
    }

    public void Update()
    {
        if (CommNetModeEntry.Value != CommLineMode.Disabled)
        {
            LinkManager.UpdateConnections();
        }
    }

    private void OnUpdateCommNetMode(object entry, EventArgs _)
    {
        var configEntry = (ConfigEntry<CommLineMode>)entry;

        if (configEntry.Value != CommLineMode.Disabled) return;

        // Remove all links
        foreach (var link in LinkManager.Links)
        {
            CommLineManager.RemoveLink(link);
        }

        LinkManager.Links.Clear();
    }
}
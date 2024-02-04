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
    public const string ModGuid = "com.gamendegamer.commlines";
    public const string ModName = "CommLines";
    public const string ModVer = "1.0.2";

    public static CommLinesPlugin Instance { get; set; }

    private const string ConfigSection = "Stable features";
    private const string ExperimentalConfigSection = "Experimental features";
    
    public static ConfigEntry<CommLineMode> CommNetModeEntry { get; private set; }
    public static ConfigEntry<bool> TransmissionMultiplier { get; private set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        // Create the patch
        Harmony.CreateAndPatchAll(typeof(CommNetPatches));

        // Start the event listener
        EventListener.RegisterEvents();

        CommNetModeEntry = Config.Bind(ConfigSection, "Use path", CommLineMode.All,
            "Set the display mode for the CommNet lines");
        CommNetModeEntry.SettingChanged += OnUpdateCommNetMode;

        TransmissionMultiplier = Config.Bind(ExperimentalConfigSection, "Transmission multiplier", false,
            "Use a multiplier for transmission time depending on the distance between CommNet nodes");

        Logger.LogInfo("Creating materials");
        PluginMaterials.GenerateMaterials();

        Logger.LogInfo("Initialized CommNet Utils");
    }

    public void Update()
    {
        if (CommNetModeEntry == null || CommNetModeEntry.Value == CommLineMode.Disabled) return;

        LinkManager.OnUpdate();
        CommNetJobHandler.OnUpdate();
    }

    private static void OnUpdateCommNetMode(object entry, EventArgs _)
    {
        var configEntry = (ConfigEntry<CommLineMode>)entry;

        if (configEntry.Value != CommLineMode.Disabled) return;

        // Remove all links and the job memory when disabling CommLines
        CommLineManager.RemoveAll();
        CommNetJobHandler.DisposeAll();
    }
}
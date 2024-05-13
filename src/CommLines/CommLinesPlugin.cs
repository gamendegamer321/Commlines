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
    public const string ModVer = "1.1.0";

    public static CommLinesPlugin Instance { get; set; }

    private const string ConfigSection = "Stable features";
    private const string ExperimentalConfigSection = "Experimental features";

    public static ConfigEntry<CommLineMode> CommNetModeEntry { get; private set; }
    public static ConfigEntry<bool> TransmissionMultiplier { get; private set; }
    public static ConfigEntry<int> LineOpacity { get; private set; }

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

        CommNetModeEntry = Config.Bind(ConfigSection, "Display mode", CommLineMode.All,
            "Set the display mode for the CommNet lines.\n\n" +
            "It may take multiple seconds for the lines to update!");
        CommNetModeEntry.SettingChanged += OnUpdateCommNetMode;

        TransmissionMultiplier = Config.Bind(ConfigSection, "Transmission multiplier", false,
            "Use a multiplier for transmission time depending on the distance between CommNet nodes");

        LineOpacity = Config.Bind(ExperimentalConfigSection, "Line opacity", 100,
            "Sets the opacity of the commlines where 0 is invisible/black and 100 fully visible");
        LineOpacity.SettingChanged += OnUpdateLineOpacity;

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

    private void OnUpdateLineOpacity(object entry, EventArgs _)
    {
        var configEntry = (ConfigEntry<int>)entry;

        switch (configEntry.Value)
        {
            case < 0:
                configEntry.Value = 0;
                Config.Save();
                break;
            case > 100:
                configEntry.Value = 100;
                Config.Save();
                break;
        }
    }
}
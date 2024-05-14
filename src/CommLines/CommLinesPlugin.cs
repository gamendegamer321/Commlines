using BepInEx;
using BepInEx.Configuration;
using CommLines.CommLines;
using HarmonyLib;
using CommLines.CommNet;
using CommLines.Patches;
using KSP.Game;
using KSP.UI.Binding;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace CommLines;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommLinesPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    public const string ModGuid = "com.gamendegamer.commlines";
    public const string ModName = "CommLines";
    public const string ModVer = "1.2.0";

    public static CommLinesPlugin Instance { get; set; }

    private const string AppbarId = "Commlines.Toggle";
    private const string ConfigSection = "Stable features";
    private const string ExperimentalConfigSection = "Experimental features";

    public static ConfigEntry<CommLineMode> CommNetModeEntry { get; private set; }
    public static ConfigEntry<bool> TransmissionMultiplier { get; private set; }
    public static ConfigEntry<int> LineOpacity { get; private set; }

    private bool _reopenTray;

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

        Logger.LogInfo("Registering button");
        Appbar.RegisterAppButton(
            "Switch commlines",
            AppbarId,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            _ => SelectNextMode()
        );

        Logger.LogInfo("Initialized CommNet Utils");
    }

    public void Update()
    {
        if (_reopenTray)
        {
            OpenTray();
            _reopenTray = false;
        }

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

    private void SelectNextMode()
    {
        if (CommNetModeEntry.Value == CommLineMode.All)
        {
            CommNetModeEntry.Value = CommLineMode.Disabled;
            Appbar.SetAppBarButtonIndicator(AppbarId, false);
        }
        else
        {
            CommNetModeEntry.Value += 1;
            Appbar.SetAppBarButtonIndicator(AppbarId, true);
        }

        Game.Notifications.ProcessNotification(new NotificationData
        {
            Tier = NotificationTier.Passive,
            Importance = NotificationImportance.None,
            Primary =
            {
                LocKey = "CommLines/Notifications/Changed",
                ObjectParams = [CommNetModeEntry.Value]
            }
        });

        _reopenTray = true;
    }

    private static void OpenTray()
    {
        var appbar = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/" +
                                     "Scaled Popup Canvas/Container/ButtonBar/BTN-App-Tray");

        if (appbar == null)
        {
            return;
        }

        var value = appbar.GetComponent<UIValue_WriteBool_Toggle>();
        if (value == null)
        {
            return;
        }
        
        value.SetValue(true);
    }
}
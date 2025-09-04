[![GitHub release](https://flat.badgen.net/github/release/gamendegamer321/Commlines/)](https://github.com/gamendegamer321/Commlines/releases/latest)
![KSP Version](https://flat.badgen.net/static/Game%20Version/v0.2.1+)
[![SpaceWarp Version](https://flat.badgen.net/static/SpaceWarp%20Version/v1.9.5+)](https://github.com/SpaceWarpDev/SpaceWarp)
[![License](https://flat.badgen.net/github/license/gamendegamer321/Commlines/)](https://github.com/gamendegamer321/Commlines/blob/master/LICENSE)

> [!NOTE]
> The tags used in this repository for version 1.2.0 and older are broken, and their releases are no longer available
> GitHub.
> Previous versions and the full changelog are still available
> on [SpaceDock](https://spacedock.info/mod/3433/Commlines#changelog).

# About CommLines

[![image](https://github.com/gamendegamer321/Commlines/blob/master/Commlines-1690204648.png)](https://github.com/gamendegamer321/Commlines/blob/master/Commlines-1690204648.png)

Adds the CommNet lines from KSP 1 to KSP 2. This will finally allow you to see the CommNet you have set up in KSP 2.

# Installation

> [!IMPORTANT]
> **Required dependencies:**
> - [SpaceWarp](https://spacedock.info/mod/3277/Space%20Warp%20+%20BepInEx) (v1.9.5+)


> [!TIP]
> CKAN makes downloading, managing and updating mods easier. You can download CKAN
> from [their GitHub](https://github.com/KSP-CKAN/CKAN).
> CKAN can also automatically download the required dependencies when adding the mod.

Download the release from the latest release from [GitHub](https://github.com/gamendegamer321/Commlines/releases/latest)
or [SpaceDock](https://spacedock.info/mod/3433/Commlines).

Open the downloaded zip file and copy the BepInEx folder to the KSP 2 root folder (<code>Kerbal Space Program 2</code>).
You can find the KSP 2 root folder by right-clicking the game in your steam library,
selecting <code>Manage</code> and then clicking <code>Browse local files</code>.

# Usage

After installing the mod there is no additional setup required. When opening a save you should already be presented with
the communication lines between vessels. If this is not the case make sure the mod is loaded!

## Lines

Whenever you open the map screen, you can see the green lines between vessels that can
communicate with each other. As vessels get further away from each other, the lines will slowly start fading from green
to red. Where a green line indicates a strong connection, and red a weak connection. If the lines become too much, you
can enable path mode in the settings, which will only show the path the connection takes from that vessel back to the
KSC. (see also the images below for a comparison)

To change the viewing mode, open the ingame settings menu and go to the "mods" tab. There you can find a section
called "CommLines", where you can change the mode. When changing the viewing mode, it might take a few seconds to
update. The available viewing modes are:

- Hop -> Only shows the first hop in your vessels path to the KSC
- VesselLinks -> Only shows connection to the vessel you can reach in 1 hop
- ActivePathOnly -> Show only the path from the active vessel to the KSC
- PathOnly -> Show the path from each vessel to the KSC
- All -> Show all connections between all vessels

## Quick switch button

While in flight mode there is a button in the app bar. Pressing this button will switch your CommLines view to the next
in the same order as the different modes are listed above. This is an easier way to go through all the different modes
and see the result than going into the settings menu. To make it quicker to toggle switch through the modes, the mod
will try to keep the app tray open.

> [!NOTE]
> The disabled or enabled indication next to the button can be ignored.
> Keep in mind, it will still take a few seconds for the lines to actually update!

## Transmission speed

When enabling the transmission multiplier in the settings or config, the amount of time needed to transmit a science
experiment will be increased depending on how well you are connected to the KSC. This calculation is done using the
slowest link between your vessel and the KSC, meaning that you can go very far distances without a significant increase,
as long as there are a lot of relays between the vessel and the KSC.

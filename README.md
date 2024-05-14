# CommLines

Adds the CommNet lines from KSP 1 to KSP 2. This will finally allow you to see the CommNet you have set up in KSP 2.

This mod requires [SpaceWarp](https://spacedock.info/mod/3277/Space%20Warp%20+%20BepInEx), it is recommended to always
use the newest version.

## How to use

### Lines

Simply install the mod and whenever you open the map screen, you can see the green lines between vessels that can
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

### Quick switch button

While in flight mode there is a button in the app bar. Pressing this button will switch your commlines view to the next
in the same order as the different modes are listed above. This is an easier way to go through all the different modes
and see the result than going into the settings menu. To make it quicker to toggle switch through the modes, the mod
will try to keep the app tray open.
The disabled or enabled indication next to the button can be ignored.

Keep in mind, it will still take a few seconds for the lines to actually update!

### Transmission speed

When enabling the transmission multiplier in the settings or config, the amount of time needed to transmit a science
experiment will be increased depending on how well you are connected to the KSC. This calculation is done using the
slowest link between your vessel and the KSC, meaning that you can go very far distances without a significant increase,
as long as there are a lot of relays between the vessel and the KSC.

## Images

![image](https://github.com/gamendegamer321/Commlines/assets/74590966/741801d9-59c8-4acf-b759-587172343d41)
![image](https://github.com/gamendegamer321/Commlines/assets/74590966/a7a83b2b-310e-435c-b918-90bd53c6249c)
![image](https://github.com/gamendegamer321/Commlines/assets/74590966/9362865f-c45b-4c7b-9d33-74689d884d6c)
![image](https://github.com/gamendegamer321/Commlines/assets/74590966/f65d884a-c834-48ec-b446-6703504cc2df)

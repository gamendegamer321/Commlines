# CommLines

Adds the CommNet lines from KSP 1 to KSP 2. This will finally allow you to see the CommNet you have set up in KSP 2.

This mod requires [SpaceWarp](https://spacedock.info/mod/3277/Space%20Warp%20+%20BepInEx) 1.5.2+

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

- DISABLED => Show none of the lines, the same as not having the mod installed.
- PATHONLY => Only show the path the connection takes.
- ALL => Show lines between all vessels that are within range of eachother

### Transmission speed [Experimental]

When enabling the transmission multiplier in the settings or config, the amount of time needed to transmit a science
experiment will be increased depending on how well you are connected to the KSC. This calculation is done using the
slowest link between your vessel and the KSC, meaning that you can go very far distances without a significant increase,
as long as there are a lot of relays between the vessel and the KSC.

## Images

Below I have added a few images of how the lines look. I have added images with pathmode off and images of the same
scenario with pathmode on.

### Pathmode = off

![image](https://github.com/gamendegamer321/Commlines/assets/74590966/741801d9-59c8-4acf-b759-587172343d41)
![image](https://github.com/gamendegamer321/Commlines/assets/74590966/a7a83b2b-310e-435c-b918-90bd53c6249c)

### Pathmode = on

![image](https://github.com/gamendegamer321/Commlines/assets/74590966/9362865f-c45b-4c7b-9d33-74689d884d6c)
![image](https://github.com/gamendegamer321/Commlines/assets/74590966/f65d884a-c834-48ec-b446-6703504cc2df)

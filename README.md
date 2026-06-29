# Setup camp
Both Nandonalt and Syrchalis have stepped away from the RimWorld modding scene, as such I am adopting the mod and will be maintaining it going forward.

Set up camp, once again²!

## Main Features
With the addition of caravan camps in RimWorld 1.6, this mod is no longer needed to provide temporary rest stops for caravanning colonists, captives, critters, and/ or cannibals. It instead provides mod settings that can be used to customize various aspects of the vanilla camps.

## Summary
Settings:
- Camp resource: allow camp maps to generate non-rock resources.
- Raid timer: Roughly the number of days before a campsite will be raided (set 0 to disable).
- Abandoned camp duration: The number of days abandoned campsites will persist on the world map (set 0 to disable).
- Camp map size: It's not necessarily recommended to change this, but the option is there.

Note: these settings are not retroactive, changing them will not effect existing camps

## Compatibility
Camps from previous version of this mod will be converted into vanilla camps. This mod doesn't add any defs to the game, so it should be safe to add or remove for existing saves at will.

Incompatibilities & Interactions:

- ReGrowth2: when re-entering an abandoned camp, the configurations for map size and timers will be ignored.
- This mod is rather aggressive in how it patches the vanilla camp generation code (the delegate is replaced entirely), which isn't necessarily great in terms of inter-mod compatibility. I'm not aware of any mods that also mess with that particular code, but I'm disclosing it because it has to potential to create a subtle conflict. If it's an issue, open an issue or drop me a bug report and I'll rework it.


## Thanks
* to [Syrchalis](https://steamcommunity.com/id/Syrchalis) for updating the mod v1.1 - 1.4
* to [Nandonalt](https://ludeon.com/forums/index.php?action=profile;u=58544) for making the mod
* to all the wonderful people on the RimWorld Discord

# Used Dropship Salesman
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that makes Urban combat more dangerous and exciting. It introduces various types of ambushes that will occur in non-interleaved (i.e. non-combat) mode. When the player's non-interleaved turn ends and control flips to the enemy team, the mod will scan for buildings near to the player's current position. If there are buildings around, there is a chance an ambush is spawned. Ambushes can take multiple forms:

:warning: This mod requires several other mods to function properly. For each of them, you should download the latest copy and include them in your Mods/ folder.

* [IRBTModUtils](https://github.com/iceraptor/IRBTModUtils/)
* [CustomAmmoCategories](https://github.com/BattletechModders/CustomBundle/tree/master/CustomAmmoCategories)

## Configuration

The mod is designed to be heavily configurable, and express that configuration through mod.json settings. It's brown down into multiple sections, which are covered in sequence. General settings are:

* **Debug**: If true, the *Mods/ConcreteJungle/concrete_jungle.log* will be more verbose.
* **Trace**: If true, the *Mods/ConcreteJungle/concrete_jungle.log* will include every line.

:information_source: Many mod options have an `Ambushes: [ ]`. These represent different ranges of contract difficulty that you can configure. You MUST supply an appropriate ambush definition for all possible contract difficulties. If a ambush definition cannot be found for the contract's difficulty, it will be discarded and that contract will not experience any mod logic. Ranges cannot overlap and must be distinct - so you cannot have 1-3 and 2-4 ranges, for instance. 

### Dropship Configuration

These options are available under the `Settings.Dropships`:

* **MaxPerMap**: The total number of ambushes that will spawn per map (*integer*)
* **MinDistanceBetween**: A minimum distance between ambush origins. Once an ambush is triggered, another ambush won't spawn until the player's units have moved as least this far away. Defaults to 300m. (*float*)
* **BaseChance**: The base chance for an ambush to spring each turn. This base chance increases for each actor that the player activates (see below). Defaults to 0.3 or 30%. (*float*)
* **ChancePerActor**: The incremental chance for an ambush to spawn for each player actor that activates. Defaults to 0.05f or 5% (*float*).
* **SearchRadius**: When determining if an ambush should be triggered, the algorithm will search from the origin point up to this radius for suitable buildings. If insufficient buildings are found, the ambush will not occur.
* **AmbushWeights**: An array of ambush types, weighted for frequency. Values must be `Explosion`, `Infantry`, `Mech`, or `Vehicle`.  When an ambush is triggered, a random selection will be made from list for the type of ambush to use. More frequent values will therefore be more common.

## DEV NOTES

### Todo

* Integrate dropship replacement for drop screen
* Integrate dropship replacement for in-mission
* Add jump costs for additional dropships?
* Implement dropship change only at planets
* Implement zoom out for camera
* 
* HangerBays fix - adjust size by dropship
* Check build from storage for hanger limits
*
* OnCareerStart - allow defaultDropship by startID, fix dropship

* Implement listener for dropship upgrade from event
* 
* Implement mechbay handling for changed dropships w/ less bay sizes
* OnChange - dialog prompting for storing mechs, stopping argo upgrades, etc
* Implement upgrade logic XX - need cleanup logic, persistence logic handled

* Allow granting items / itemCoillections on receiving dropship (StratOps integration)

* Replace event text that relies upon argo
* Implement chassis specific tag at change time
* Implement tag-based restrictions for events


### Ideas

* Create 'select dropship' option in command center
* Create argo + 1 dropship where drop-bays are tied to the dropship, not argo
* Add upkeep costs for additional dropships
* Create option to auto-magically grant airsrike beacons for dropships that have bays
* Create option for mechtech, medtech multiplers on units (cramped conditions)
* Create option to limit pilots size by dropship
* Create main + child dropship approach (for Shade)
* Add upgrade stats for mechbays?
* 
### Interesting methods
  
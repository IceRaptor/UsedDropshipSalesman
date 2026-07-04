# Used Dropship Salesman
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that allows players to use different Dropships at the SimGame level. 
Mod authors can customize dropship configuration to support different hangar bay limits, upgrade paths, number of crew berths, and more. 
The mod does not change the combat layer.

:warning: This mod requires several other mods to function properly. For each of them, you should download the latest copy and include them in your Mods/ folder.

* [IRBTModUtils](https://github.com/BattletechModders/IRBTModUtils/)
* [IRTweaks](https://github.com/BattletechModders/IRTweaks/)
* [CustomAmmoCategories](https://github.com/BattletechModders/CustomBundle/tree/master/CustomAmmoCategories)
* [CustomUnits](https://github.com/BattletechModders/CustomBundle/tree/master/CustomUnits) - version 0.0.0.199 (with CustomHangarConstraints) minimum
* [JwTweaks](https://github.com/wmtorode/JwTweaks)

:no_entry: This mod conflicts with [BiggerDrops](https://github.com/BattletechModders/BiggerDrops/) and will not load if that mod is present. 
Do not attempt to circumvent these; these mods WILL step on each other and result in an incorrect working state.

## Configuration

The mod is designed to be heavily configurable, but that configuration is spread across multiple sources:

* *Mods/UsedDropshipSalesman/settings.json* contains general configuration
* *Mods/UsedDropshipSalesman/customDropships* contains custom dropship variants, expressed as a ModTek custom resource
* *Mods/UsedDropshipSalesman/shipUpgrades* contains ShipModuleDef upgrades (aka Argo upgrades)
* *Mods/UsedDropshipSalesman/icons* contains icons for ShipModuleDef upgrades (aka Argo upgrades)
* *Mods/UsedDropshipSalesman/simGameStateDesc* contains statistic descriptions for events
* *Mods/UsedDropshipSalesman/advancedJsonMerges* contains logging config for the ModTek logger (see below)

Finally, some debugging configuration is made available as part of this mod package. You are encouraged to remove this once your testing is complete.

* *Mods/UsedDropshipSalesman/events* contains useful testing events, all prefixed with "uds_" in the debug career event screen.

### Logging

This mod relies upon [ModTek Logging](https://github.com/BattletechModders/ModTek/blob/master/doc/LOGGING.md). 
It ships by default configured to log at the DEBUG level, which is useful to help diagnose configuration issues. 
You're encouraged to change the logging level to INFO once your configuration is stable. To do so, edit 
*Mods/UsedDropshipSalesman/advancedJsonMerges/logging.json* and set the v: value to `Info` instead of `Debug`. 

You can increase the logging level to Trace by setting the v: value to `200` instead, as documented on the ModTek logging page. 
Trace is really only useful to me as a way to walk through code paths, but it's here if you are curious.

### General Configuration

These options are provided under *Mods/UsedDropshipSalesman/settings.json*. 

| Setting | Default Value | Description |
| ------- | ------- | ----------- |
| FallbackDropship | **union_base** |  The custom dropship ID that will be used when existing values aren't explicit. Set this equal to the dropship you want used on an existing career. |
| CareerStartDropshipByPlanetName | NA | An dictionary (key-value pairs) that links dropship IDs to starting planets. If a starting planet isn't present, on career start the player will receive the `FallbackDropship` instead |
| PersistentUpgrades | None | Upgrades that should carry (i.e. persist) across dropship changes. These are only carried forward if the new dropship has them in their list of possible upgrades. If you list `argoUpgrade_MyUpgrade123` here, and the player changes to a variant that doesn't include `argoUpgrade_MyUpgrade123` as an available upgrade, the upgrade is 'hidden' from that dropship. If the player later changes to another dropship that has `argoUpgrade_MyUpgrade123` as an option, they will receive it at that time. |
| Colors.Upgrades | NA | A dictionary of arrays that defines the colors used for dropship upgrades (aka Argo Upgrades). Each value is represented as a Unity RGBA float array, such as [ 1.0, 1.0, 1.0, 1.0 ] for a fully saturated white or [ 0.0, 0.0, 0.0, 0.5 ] for a half-saturated black. Each setting MUST have two values; the first is the hovered color and the second is the active color.	 |
| Colors.Upgrades.Purchased | [ 0.253, 1.0, 0.0, 0.5 ], [ 0.253, 1.0, 0.0, 1.0 ] | Colors for ShipUpgrades that have already been purchased |
| Colors.Upgrades.Available | [ 1.0, 1.0, 1.0, 0.5 ], [ 1.0, 1.0, 1.0, 1.0 ] | Colors for ShipUpgrades that are available for purchased |
| Colors.Upgrades.Unavailable | [ 1.0, 0.235, 0.0, 0.196 ], [ 1.0, 0.235, 0.0, 0.5 ] | Colors for ShipUpgrades that are unavailable due to dependencies  |
| Colors.Upgrades.Innate | [ 0.0, 0.65, 1.0, 0.5 ], [ 1.0, 1.0, 1.0, 1.0 ] | Colors for ShipUpgrades that are innate to the dropship |


### Dropship Configuration

Dropship variants are configured through a ModTek custom resource named `CustomDropshipDef`. Each file in *Mods/UsedDropshipSalesman/customDropships* is a single dropship variant. Each CustomDropshipDef defines gameplay specifics of a dropship, 
but also associates it with prefabs that are displayed on screen. It's expected that you'll have more than one *Dropship Variant* pointing to the same prefabs, each variant reflecting a specific role for that chassis. 
UDS ships with a standard Leopard (`leopard_base`) and a vehicle-centric Leopard (`leopard_tank_variant`) but both use the HBS provided Leopard visuals. Through the rest of this file, 
assume 'dropship variant' refers to a specific CustomDropshipDef (i.e. either `leopard_base` or `leopard_tank_variant`), NOT the prefab and the common association of the terms.

:no_entry: Each CustomDropshipDef MUST have a unique `Description.Id` value. UDS relies upon this key to identify the custom resource. Multiple files with the same Id are likely to cause odd, hard to detect errors. You've been warned.

**Description**

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| Description.Id | `leopard_tank-variant` | As described above, `Description.Id` is the unique key that identifies this dropship variant. It MUST be unique, and is the value you'll use when you want to give the player a specific dropship variant.  |
| Description.Name | `Leopard-T` | This value will be used throughout the game UI, and is what the player will see. It will be added to the left-nav button on the SimGame screen, and used in events. You should try to limit this to 10-12 characters or less.  |

**Visuals**

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| assetBundleId | `chrprfvhcl_uds_union` | The assetbundle of the dropship that should be shown on the main screen. This should be just the assetbundle name listed in the manifest. | 
| prefabPath | `assets/character/vehicle/prefabs/uds/chrprfvhcl_uds_union.prefab` | The prefab that will be overlayed on the existing HBS meshes. This should be a full path within the assetbundle, as described in the manfiest. |
| attachEngineGlow | `ap_engine_lights_1` | A Tranform within the prefab that indicates where the engine glow should emanate from. See the Modeling section below |
| attachDecal | `ap_decal` | A Tranform within the prefab that indicates where player company logo decal should attach. See the Modeling section below |
| attachesEngines | `[ "ap_engine_jets_1", "ap_engine_jets_2", "ap_engine_jets_3", "ap_engine_jets_4" ]` | A Tranform within the prefab that indicates where engine jets should be created. See the Modeling section below |
| attachesSpotLights | `[ "ap_spotlight_1", "ap_spotlight_2" ]` | A Tranform within the prefab that indicates where spot lights should be created. See the Modeling section below |
| attachesRunningLights| `[ "ap_runLight_green_1", "ap_runLight_green_2", "ap_runLight_red_1", "ap_rightLight_red_2" ]` | A Tranform within the prefab that indicates where blinking running lights should be created. See the Modeling section below |

**Costs**

:warning: NOT IMPLEMENTED YET

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| purchase | 30000.0 | The amount of c-bills required to purchase this dropship. TBD |
| upkeep | 3000.0 | The amount of c-bills required each month for this dropship. Will be shown on the monthly upkeep screen in the commander's office. |
| drop | 300.0 | The amount of c-bills required for each combat drop with this dropship. Will be shown in the after-combat screen. |


** Requirements**

:warning: NOT IMPLEMENTED YET

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| eventTag | `uds_require_dropship_union` | Events that have dropship tags (TODO: Add) will be skipped if they don't have this tag |
| factionReputation | 100 | The amount of faction reputation required for the faction that owns a planet before the player can purchase a dropship. |
| mustBeAllied | `true` | If true, the player can only purchase the dropship if they are allied with the faction that owns the planet. |
| planetTags | `[ "planet_industry_electronics", "planet_pop_large" ]` | If present, the dropship can only be purchased on a planet with these tags. |

**HangarBays**

This value defines the maximum amount of units available in hangar bays. It relies upon the CustomUnits CustomHangarDef configuration. 
For each hangar defined in *Mods/CustomUnits/hangardefs* (or your own location) should have it's key and value listed in this dictionary. 
The keys below should work for both RogueTech and BTAU:

| Key | Example| Description | 
| --- | ----- | ----------- |
| `BASE_HANGER` | 8 | The base hangar is the 'vanilla' hangar, whose label is controlled by CustomUnits' `MechBayDefaultLabel`. This is typically the mechbay. The example value of 8 means 8 total Mechs can be active or readied. |
| `vehicle_bays` | 2 | Configured in RT and BTA as the 'vehicle bays', the example value of 2 means two vehicles can be active or readied |
| `battle_armor_bays` | 0 | Configured in RT and BTA as the 'battle armor bays', the example value of 0 means no battle armor can be active or ready on this dropship.

:information_source: Note that the HBS bays system has been completely replaced by this approach. 
You MUST NOT change the `Constants.Story.MechBayPodsID` value while using this mod, as it expects the value to be 3 at all times. This is done to allow CU to function normally without this mod, 
but implement our constrained hangars for our purposes. If this value gets set to anything other than 3, odd things are very likely to occur!

**DropBays**

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| labels | `[ "Lance Alpha", "Lance Beta" ]` | The labeled names you want to display on the drop screen. The count of labels MUST match the count of arrays in the `slots` value! |
| maxTonnage | 200 | The maximum amount of tonnage you want available for drop purposes. :warning: NOT YET IMPLEMENTED
| slots | `[ "default_mech_slot", "default_mech_slot", "default_mech_slot", "default_mech_slot" ]` | The CustomUnits config labels that define what units can be assocaited with a specific drop slot. You can have multiple arrays representing multiple squads. The count of arrays must match the `labels` value above. |

**Upgrades**

UDS allows you to completely customize the upgrade screen for each dropship. There are three entities involved in the configuration of the screen:

* **Upgrades** are the lowest level, and are directly linked to a ShipUpgradeDefs. An upgrade can belong to multiple systems. 
* **Upgrade Systems** are a set of related upgrades grouped for progression purposes. Systems MUST have one or more upgrades.
* **Upgrade Categories** define a row of associated systems grouped under a common header and icon. Categories MUST have one or more systems.

Unlike vanilla, categoires are assembled in a straight column that's left-oriented in the upgrade screen. I've been able to fit about 6 or 7 on the screen. 

*Upgrade Categories*

| Setting | Example | Description |
| ------- | ------- | ----------- | 
| categoryId | `movement` | A unique id used for this category, used internally. |
| headerText | `Movement` | The category name that will be shown to players on the argo upgrade screen. |
| icon | `lv_brass-eye` | An SVGImage used on the left-side of the category. Can be any loaded asset. |
| systems | NA | An array of *Upgrade Systems* (see below) |

*Upgrade Systems*

| Setting | Example | Description |
| ------- | ------- | ----------- |
| systemId | `main_engine` | A unique id for this system, used internally. |
| headerText | `Main Engines` | The text shown to the players as the system 'group' name |
| innateUpgrades | `[ "argoUpgrade_uds_union_base_engine_1" ]` | An array of ShipModuleDefs that will be automatically applied when this dropship is applied. These are reflected in different colors on the screen. |
| optionaUpgrades | ` [ "argoUpgrade_uds_union_base_engine_2", "argoUpgrade_uds_union_base_engine_3", "argoUpgrade_uds_union_base_engine_4" ]` | An array of ShipModuleDefs that are available for purchase in this dropship. |

## Dropship Upgrades

To grant a player a new dropship, set the Company statistic `UDS_CURRENT_DROPSHIP` to the custom dropshipID of your choice. 
You can easily do this via event, or via reward from ROI (TODO: Add details).

When UDS detects a change in dropship (technically - SaveState differs from Company stat value), it will wait until the player enters orbit around a planet. 
Then it will validate the new dropship can fit the player's current hangar bay, crew count, and check for upgrades. If blocking events are detected, 
it will warn the player and defer the upgrade to the following day. 

Once an upgrade is clear to proceed, UDS iterates through every registered ShipModuleUpgrade. Any that match the current definition of the old dropship's config 
will be *reverted*. This means that any statistic changes will be rolled back, and any tags added will be removed. Any `PersistentUpgrades` defined in the 
current mod config, AND also present in the new dropship config will be retained (i.e. skipped). On the next day, the dropship visuals and labels will be updated. 

:warning: Currently persistent upgrades don't intersect with the new dropship config, this can leave upgrades in place that should be reverted. 

There is currently no way to stop an upgrade once it's begun. 

## Modeling Dropships

TBD

## DEV NOTES

### Todo

Dropship Graphics
- [ ] Fix/Inject camo holder on SimScreen
- [ ] Integrate dropship replacement for drop screen
- [ ] Implement transform delta for dropscreen (for spheriods)
- [ ] Integrate dropship replacement for in-mission
- [ ] Add jump costs for additional dropships?
- [ ] Implement zoom out for camera
- [ ] Implement running lights - refactor customdEF to red/green lights

Visuals
- [ ]  Replace event text that relies upon argo
- [ ]  Replace UI text that relies upon Argo (argo upgrades, argo timeline, etc)

Gameplay
- [ ] Implement dropship change only at planets X
- [X] OnCareerStart - allow defaultDropship by startID, fix dropship
- [X] Implement listener for dropship upgrade from event
- [X] Implement upgrade logic XX - need cleanup logic, persistence logic handled
- [ ] Validate that persistentUpgrades are carried forward across dropships. 
- [ ] Persist persistentUpgrades in save game state
- [ ] Persist per-variant ugprades in save game state
- [ ] Implement costs - upkeep, drop
- [ ] Implement costs - purchase (plus item?)
- [ ] Localize Upgrade category, system text
- [ ] Fix reversion logic for persistent upgrades; only those defined on the new ship should be kept, not all of them.

- [X] HangerBays fix - adjust size by dropship 
- [X] Check build from storage for hanger limits
- [ ] Limit crew berths by dropship 
- [ ] Limit medtech values by dropship 
- [ ] Limit mechtech values by dropship

- [X] Implement mechbay handling for changed dropships w/ less bay sizes
- [X] OnChange - dialog prompting for storing mechs, stopping argo upgrades, etc

StratOps integration
- [ ]  Allow granting items / itemCollections on receiving dropship (StratOps integration)
- [ ] Allow for artillery strike (Fortress)
- [ ]  Implement dropship specific tag at change time
- [ ]  Implement tag-based restrictions for events

BiggerDrops Features
- [ ] Kill-BD: Implement drop tonnage (defaultMaxTonnage)
- [ ]  Kill-BD: Implement custom drop sizes (by type)
- [ ]  Kill-BD: Respect flashpoint and 4 unit drop limitations (respectFourDropLimit, limitFlashpointDrop)


### Ideas

* Create 'select dropship' option in command center
* Create option to auto-magically grant airsrike beacons for dropships that have bays
* Create option for mechtech, medtech multiplers on units (cramped conditions)
* Create option to limit pilots size by dropship
* Add upgrade stats for mechbays?

Multiple Dropships
* Create main + child dropship approach (for Shade)
* Create argo + 1 dropship where drop-bays are tied to the dropship, not argo
* Add upkeep costs for additional dropships

### Interesting methods
  
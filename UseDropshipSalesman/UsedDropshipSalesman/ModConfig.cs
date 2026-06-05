
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UsedDropshipSalesman;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman
{
    public record DropshipConfig
    {
        public DropshipPrefabConfig prefab;
        public DropshipCosts costs;
        public DropshipRequirements requirements;
        public DropshipBays bays;
        public ColorConfig colors;
        public List<DropshipUpgradeCategory> upgrades;
    }

    public record DropshipPrefabConfig
    {
        public String AssetBundleId;
        public String PrefabPath;
        public String AttachEngineGlow;
        public String AttachDecal;
        public List<String> AttachesEngines;

        // TODO:TBD
        public List<String> AttachesSpotLights;
        public List<String> AttachesRunningLights;
    }

    public record DropshipRequirements
    {
        public int FactionReputation;
        public bool MustBeAllied = false;
        public String[] PlanetTags = Array.Empty<String>();
    }

    public record DropshipCosts
    {
        public float Purchase;
        public float Upkeep;
        public float Drop;
    }

    public record DropshipBays
    {
        public int MechBays = 4;
        public int VehicleBays = 0;
        public int BattleArmorBays = 0;
    }

    public record ColorConfig
    {
        public String UpgradePurchased;
        public String UpgradePurchasedHovered;
        public String UpgradeAvailable;
        public String UpgradeAvailableHovered;
        public String UpgradeUnavailable;
        public String UpgradeUnavailableHovered;
        public String UpgradeInnate;
        public String UpgradeInnateHovered;
    }

    public class ModConfig
    {

        // If true, many logs will be printed
        public bool Debug = false;
        // If true, all logs will be printed
        public bool Trace = false;

        public string DefaultDropship;
        public Dictionary<String, DropshipConfig> Dropships = new Dictionary<String, DropshipConfig>();

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG:{this.Debug} Trace:{this.Trace}");
            Mod.Log.Info?.Write($"  DefaultDropshipID: {this.DefaultDropship}");

            Mod.Log.Info?.Write("\n  --- DROPSHIPS CONFIG ---");
            foreach (KeyValuePair<string, DropshipConfig> kvp in Dropships)
            {
                Mod.Log.Info?.Write($"\n  DropshipID: {kvp.Key}");
                Mod.Log.Info?.Write("  ---- PREFAB");
                Mod.Log.Info?.Write($" assetBundleID          : {kvp.Value.prefab.AssetBundleId}");
                Mod.Log.Info?.Write($" prefabPath             : {kvp.Value.prefab.AssetBundleId}");
                Mod.Log.Info?.Write($" attachDecal            : {kvp.Value.prefab.AttachDecal}");
                Mod.Log.Info?.Write($" attachEngineGlow       : {kvp.Value.prefab.AttachEngineGlow}");
                Mod.Log.Info?.Write($" attachesEngines        : {(kvp.Value.prefab.AttachesEngines != null ? String.Join(",", kvp.Value.prefab.AttachesEngines) : "None" )}");
                Mod.Log.Info?.Write($" attachesSpotLights     : {(kvp.Value.prefab.AttachesSpotLights != null ? String.Join(",", kvp.Value.prefab.AttachesSpotLights) : "None" )}");
                Mod.Log.Info?.Write($" attachesRunningLights  : {(kvp.Value.prefab.AttachesRunningLights != null ? String.Join(",", kvp.Value.prefab.AttachesRunningLights) : "None" )}");
                Mod.Log.Info?.Write("  ---- COSTS");
                Mod.Log.Info?.Write($" purchase: {kvp.Value.costs.Purchase}  upkeep: {kvp.Value.costs.Upkeep}  drop: {kvp.Value.costs.Drop}");
                Mod.Log.Info?.Write("  ---- REQUIREMENTS");
                Mod.Log.Info?.Write($" factionRep: {kvp.Value.requirements.FactionReputation}  mustBeAllied: {kvp.Value.requirements.MustBeAllied}");
                Mod.Log.Info?.Write($" planetTags       : {String.Join(",", kvp.Value.requirements.PlanetTags)}");
                Mod.Log.Info?.Write("  ---- BAYS");
                Mod.Log.Info?.Write($" mech: {kvp.Value.bays.MechBays}  vehicle: {kvp.Value.bays.VehicleBays}  battleArmor: {kvp.Value.bays.BattleArmorBays}");
                Mod.Log.Info?.Write("  ---- COLORS");
                Mod.Log.Info?.Write($" purchased: {kvp.Value?.colors?.UpgradePurchased}  onHover: {kvp.Value?.colors?.UpgradePurchasedHovered}");
                Mod.Log.Info?.Write($" available: {kvp.Value?.colors?.UpgradeAvailable}  onHover: {kvp.Value?.colors?.UpgradeAvailableHovered}");
                Mod.Log.Info?.Write($" unavailable: {kvp.Value?.colors?.UpgradeUnavailable}  onHover: {kvp.Value?.colors?.UpgradeUnavailableHovered}");
                Mod.Log.Info?.Write("  ---- UPGRADES");
                foreach (var category in kvp.Value.upgrades)
                {
                    Mod.Log.Info?.Write($" -------- categoryID: {category.CategoryId}  headerText: {category.HeaderText}  icon: {category.Icon}");
                    foreach (var system in category.Systems)
                    {
                        Mod.Log.Info?.Write($"          systemId        : {system.SystemId}  headerText: {system.HeaderText}");
                        Mod.Log.Info?.Write($"          innateUpgrades  : {String.Join(",", system.innateUpgrades)}");
                        Mod.Log.Info?.Write($"          optionalUpgrades: {String.Join(",", system.optionalUpgrades)}");
                    }
                }
            }

            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            Mod.Log.Debug?.Write(" == Initializing Configuration");


            Mod.Log.Debug?.Write(" == Configuration Initialized");
        }

    }
}

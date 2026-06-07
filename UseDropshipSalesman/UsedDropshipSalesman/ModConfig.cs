
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UsedDropshipSalesman;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman
{
    public record DropshipConfig
    {
        public String Label;

        public DropshipPrefabConfig prefab;
        public DropshipCosts costs;
        public DropshipRequirements requirements;
        public DropshipBays bays;
        public ColorConfig colors;

        public List<DropshipUpgradeCategory> upgrades;
                // Initialized during config
        public List<String> AllUpgradeIds;
        public List<String> InnateUpgradeIds;
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
        public String eventTag;
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
        public float[] UpgradePurchasedColor;
        public Color UpgradePurchased;

        public float[] UpgradePurchasedHoveredColor;
        public Color UpgradePurchasedHovered;

        public float[] UpgradeAvailableColor;
        public Color UpgradeAvailable;

        public float[] UpgradeAvailableHoveredColor;
        public Color UpgradeAvailableHovered;

        public float[] UpgradeUnavailableColor;
        public Color UpgradeUnavailable;

        public float[] UpgradeUnavailableHoveredColor;
        public Color UpgradeUnavailableHovered;

        public float[] UpgradeInnateColor;
        public Color UpgradeInnate;

        public float[] UpgradeInnateHoveredColor;
        public Color UpgradeInnateHovered;
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

            
            foreach (KeyValuePair<String, DropshipConfig> kvp in this.Dropships)
            {
                Mod.Log.Debug?.Write($"Processing dropship: {kvp.Key}");
                ConvertColor(kvp.Value);
                PopulateUpgrades(kvp.Value);
            }



            Mod.Log.Debug?.Write(" == Configuration Initialized");
        }

        private void PopulateUpgrades(DropshipConfig config)
        {
            Mod.Log.Debug?.Write(" -- Aggregating upgrades.");

            config.AllUpgradeIds = config.upgrades.SelectMany(cats => config.upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades.Union(sys.optionalUpgrades))
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Write($" All upgrades for dropship are: {String.Join(",", config.AllUpgradeIds)}");

            config.InnateUpgradeIds = config.upgrades.SelectMany(cats => config.upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades)
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Write($" Innate upgrades for dropship are: {String.Join(",", config.InnateUpgradeIds)}");
        }

        private void ConvertColor(DropshipConfig config)
        {
            Mod.Log.Debug?.Write(" -- Converting colors.");

            if (config.colors == null)
            {
                config.colors = new ColorConfig()
                {
                    UpgradePurchased = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED,
                    UpgradePurchasedHovered = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER,
                    UpgradeAvailable = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE,
                    UpgradeAvailableHovered = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER,
                    UpgradeUnavailable = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE,
                    UpgradeUnavailableHovered = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER,
                    UpgradeInnate = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE,
                    UpgradeInnateHovered = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE_HOVER
                };

                return;
            }

            if (
                config.colors.UpgradePurchasedColor != null && config.colors.UpgradePurchasedColor.Length == 4)
            {
                config.colors.UpgradePurchased = new Color(
                    config.colors.UpgradePurchasedColor[0],
                    config.colors.UpgradePurchasedColor[1],
                    config.colors.UpgradePurchasedColor[2],
                    config.colors.UpgradePurchasedColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradePurchased set to: {config.colors.UpgradePurchased}");
            }
            else
            {
                config.colors.UpgradePurchased = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED;
                Mod.Log.Debug?.Write($" UpgradePurchased defaulted to: {config.colors.UpgradePurchased}");
            }

            if (config.colors != null && 
                config.colors.UpgradePurchasedHoveredColor != null && config.colors.UpgradePurchasedHoveredColor.Length == 4)
            {
                config.colors.UpgradePurchasedHovered = new Color(
                    config.colors.UpgradePurchasedHoveredColor[0],
                    config.colors.UpgradePurchasedHoveredColor[1],
                    config.colors.UpgradePurchasedHoveredColor[2],
                    config.colors.UpgradePurchasedHoveredColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradePurchasedHovered set to: {config.colors.UpgradePurchased}");
            }
            else
            {
                config.colors.UpgradePurchasedHovered = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER;
                Mod.Log.Debug?.Write($" UpgradePurchasedHovered defaulted to: {config.colors.UpgradePurchasedHovered}");
            }

            if (config.colors != null && 
                config.colors.UpgradeAvailableColor != null && config.colors.UpgradeAvailableColor.Length == 4)
            {
                config.colors.UpgradeAvailable = new Color(
                    config.colors.UpgradeAvailableColor[0],
                    config.colors.UpgradeAvailableColor[1],
                    config.colors.UpgradeAvailableColor[2],
                    config.colors.UpgradeAvailableColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeAvailable set to: {config.colors.UpgradePurchased}");
            }
            else
            {
                config.colors.UpgradeAvailable = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE;
                Mod.Log.Debug?.Write($" UpgradeAvailable defaulted to: {config.colors.UpgradeAvailable}");
            }

            if (config.colors != null && 
                config.colors.UpgradeAvailableHoveredColor != null && config.colors.UpgradeAvailableHoveredColor.Length == 4)
            {
                config.colors.UpgradeAvailableHovered = new Color(
                    config.colors.UpgradeAvailableHoveredColor[0],
                    config.colors.UpgradeAvailableHoveredColor[1],
                    config.colors.UpgradeAvailableHoveredColor[2],
                    config.colors.UpgradeAvailableHoveredColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeAvailableHovered set to: {config.colors.UpgradeAvailableHovered}");
            }
            else
            {
                config.colors.UpgradeAvailableHovered = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER;
                Mod.Log.Debug?.Write($" UpgradeAvailableHovered defaulted to: {config.colors.UpgradeAvailableHovered}");
            }

            if (config.colors != null && 
                config.colors.UpgradeUnavailableColor != null && config.colors.UpgradeUnavailableColor.Length == 4)
            {
                config.colors.UpgradeUnavailable = new Color(
                    config.colors.UpgradeUnavailableColor[0],
                    config.colors.UpgradeUnavailableColor[1],
                    config.colors.UpgradeUnavailableColor[2],
                    config.colors.UpgradeUnavailableColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeUnavailable set to: {config.colors.UpgradeUnavailable}");
            }
            else
            {
                config.colors.UpgradeUnavailable = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE;
                Mod.Log.Debug?.Write($" UpgradeUnavailable defaulted to: {config.colors.UpgradeUnavailable}");
            }

            if (config.colors != null && 
                config.colors.UpgradeUnavailableHoveredColor != null && config.colors.UpgradeUnavailableHoveredColor.Length == 4)
            {
                config.colors.UpgradeUnavailableHovered = new Color(
                    config.colors.UpgradeUnavailableHoveredColor[0],
                    config.colors.UpgradeUnavailableHoveredColor[1],
                    config.colors.UpgradeUnavailableHoveredColor[2],
                    config.colors.UpgradeUnavailableHoveredColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeUnavailableHovered set to: {config.colors.UpgradeUnavailableHovered}");
            }
            else
            {
                config.colors.UpgradeUnavailableHovered = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER;
                Mod.Log.Debug?.Write($" UpgradeUnavailableHovered defaulted to: {config.colors.UpgradeUnavailableHovered}");
            }

            if (config.colors != null && 
                config.colors.UpgradeInnateColor != null && config.colors.UpgradeInnateColor.Length == 4)
            {
                config.colors.UpgradeInnate = new Color(
                    config.colors.UpgradeInnateColor[0],
                    config.colors.UpgradeInnateColor[1],
                    config.colors.UpgradeInnateColor[2],
                    config.colors.UpgradeInnateColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeInnate set to: {config.colors.UpgradeInnate}");
            }
            else
            {
                config.colors.UpgradeInnate = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE;
                Mod.Log.Debug?.Write($" UpgradeInnate defaulted to: {config.colors.UpgradeInnate}");
            }

            if (config.colors != null && 
                config.colors.UpgradeInnateColor != null && config.colors.UpgradeInnateColor.Length == 4)
            {
                config.colors.UpgradeInnateHovered = new Color(
                    config.colors.UpgradeInnateHoveredColor[0],
                    config.colors.UpgradeInnateHoveredColor[1],
                    config.colors.UpgradeInnateHoveredColor[2],
                    config.colors.UpgradeInnateHoveredColor[3]
                    );
                Mod.Log.Debug?.Write($" UpgradeInnateHovered set to: {config.colors.UpgradeInnateHovered}");
            }
            else
            {
                config.colors.UpgradeInnateHovered = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE_HOVER;
                Mod.Log.Debug?.Write($" UpgradeInnateHovered defaulted to: {config.colors.UpgradeInnateHovered}");
            }

        }

    }
}

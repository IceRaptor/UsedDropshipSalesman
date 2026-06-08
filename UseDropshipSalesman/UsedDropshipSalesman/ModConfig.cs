
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
        public DropshipDropBays DropBays;
        public DropshipBays RepairBays;
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

    public record DropshipDropBays
    {
        public String[] Labels;
        public int MaxTonnage;
        public String[][] Slots;
    }

    public record ColorConfig
    {
        public UpgradeColors Upgrades;
    }

    public record UpgradeColors
    {
        public float[][] Purchased;
        public Color PurchasedColor;
        public Color PurchasedHoverColor;

        public float[][] Available;
        public Color AvailableColor;
        public Color AvailableHoverColor;

        public float[][] Unavailable;
        public Color UnavailableColor;
        public Color UnavailableHoverColor;

        public float[][] Innate;
        public Color InnateColor;
        public Color InnateHoverColor;
    }

    public class ModConfig
    {

        // If true, many logs will be printed
        public bool Debug = false;
        // If true, all logs will be printed
        public bool Trace = false;

        public string DefaultDropship;
        public List<String> PersistentUpgrades; // TODO: Doc
        public ColorConfig Colors;

        public Dictionary<String, DropshipConfig> Dropships = new Dictionary<String, DropshipConfig>();

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG:{this.Debug} Trace:{this.Trace}");
            Mod.Log.Info?.Write($"  DefaultDropshipID: {this.DefaultDropship}");

            Mod.Log.Info?.Write("  ---- PERSISTENT UPGRADES");
            foreach (String upgrade in this.PersistentUpgrades)
            {
                Mod.Log.Info?.Write($" id: {upgrade}");
            }
            
            Mod.Log.Info?.Write("  ---- COLORS");
            Mod.Log.Info?.Write($" purchased: {this.Colors?.Upgrades?.PurchasedColor}  onHover: {this.Colors?.Upgrades?.PurchasedHoverColor}");
            Mod.Log.Info?.Write($" available: {this.Colors?.Upgrades?.AvailableColor}  onHover: {this.Colors?.Upgrades?.AvailableHoverColor}");
            Mod.Log.Info?.Write($" unavailable: {this.Colors?.Upgrades?.UnavailableColor}  onHover: {this.Colors?.Upgrades?.UnavailableHoverColor}");

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
                Mod.Log.Info?.Write("  ---- REPAIR BAYS");
                Mod.Log.Info?.Write($" mech: {kvp.Value?.RepairBays?.MechBays}  vehicle: {kvp.Value?.RepairBays?.VehicleBays}  battleArmor: {kvp.Value?.RepairBays?.BattleArmorBays}");
                Mod.Log.Info?.Write("  ---- DROP BAYS");
                Mod.Log.Info?.Write($" maxTonnage: {kvp.Value?.DropBays?.MaxTonnage}");
                for (int i = 0; i < kvp.Value?.DropBays?.Labels?.Length; i++)
                {
                    Mod.Log.Info?.Write($"Lance: '{kvp.Value?.DropBays?.Labels[i]}' => [{String.Join(",", kvp.Value?.DropBays?.Slots[i])}]");
                }

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

                if (kvp.Value.DropBays != null &&
                        kvp.Value.DropBays?.Labels.Length != kvp.Value.DropBays?.Slots.Length)
                {
                    Mod.Log.Error?.Write("Critical error - dropbay labels and slots don't match, cannot continue!");
                }

                ConvertColors();
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

        private void ConvertColors()
        {
            Mod.Log.Debug?.Write(" -- Converting colors.");

            if (this.Colors == null)
            {
                this.Colors = new ColorConfig()
                {
                    Upgrades = new UpgradeColors
                    {
                        PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED,
                        PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER,
                        AvailableColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE,
                        AvailableHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER,
                        UnavailableColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE,
                        UnavailableHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER,
                        InnateColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE,
                        InnateHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE_HOVER
                    }
                };

                return;
            }

            if (this.Colors.Upgrades.Purchased != null && this.Colors.Upgrades.Purchased.Length == 2)
            {
                this.Colors.Upgrades.PurchasedColor = new Color(
                    this.Colors.Upgrades.Purchased[0][0],
                    this.Colors.Upgrades.Purchased[0][1],
                    this.Colors.Upgrades.Purchased[0][2],
                    this.Colors.Upgrades.Purchased[0][3]
                    );
                this.Colors.Upgrades.PurchasedHoverColor = new Color(
                    this.Colors.Upgrades.Purchased[1][0],
                    this.Colors.Upgrades.Purchased[1][1],
                    this.Colors.Upgrades.Purchased[1][2],
                    this.Colors.Upgrades.Purchased[1][3]
                    );
                Mod.Log.Debug?.Write($" Purchased set to: {this.Colors.Upgrades.Purchased} / {this.Colors.Upgrades.PurchasedHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER;
                Mod.Log.Debug?.Write($" Purchased defaulted to: {this.Colors.Upgrades.Purchased} / {this.Colors.Upgrades.PurchasedHoverColor}");
            }

            if (this.Colors.Upgrades.Available != null && this.Colors.Upgrades.Available.Length == 2)
            {
                this.Colors.Upgrades.AvailableColor = new Color(
                    this.Colors.Upgrades.Available[0][0],
                    this.Colors.Upgrades.Available[0][1],
                    this.Colors.Upgrades.Available[0][2],
                    this.Colors.Upgrades.Available[0][3]
                    );
                this.Colors.Upgrades.AvailableHoverColor = new Color(
                    this.Colors.Upgrades.Available[1][0],
                    this.Colors.Upgrades.Available[1][1],
                    this.Colors.Upgrades.Available[1][2],
                    this.Colors.Upgrades.Available[1][3]
                    );
                Mod.Log.Debug?.Write($" Available set to: {this.Colors.Upgrades.AvailableColor} / {this.Colors.Upgrades.AvailableHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER;
                Mod.Log.Debug?.Write($" Available defaulted to: {this.Colors.Upgrades.AvailableColor} / {this.Colors.Upgrades.AvailableHoverColor}");
            }

            if (this.Colors.Upgrades.Unavailable != null && this.Colors.Upgrades.Unavailable.Length == 2)
            {
                this.Colors.Upgrades.UnavailableColor = new Color(
                    this.Colors.Upgrades.Unavailable[0][0],
                    this.Colors.Upgrades.Unavailable[0][1],
                    this.Colors.Upgrades.Unavailable[0][2],
                    this.Colors.Upgrades.Unavailable[0][3]
                    );
                this.Colors.Upgrades.UnavailableHoverColor = new Color(
                    this.Colors.Upgrades.Unavailable[1][0],
                    this.Colors.Upgrades.Unavailable[1][1],
                    this.Colors.Upgrades.Unavailable[1][2],
                    this.Colors.Upgrades.Unavailable[1][3]
                    );
                Mod.Log.Debug?.Write($" Unavailable set to: {this.Colors.Upgrades.UnavailableColor} / {this.Colors.Upgrades.UnavailableHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER;
                Mod.Log.Debug?.Write($" Unavailable defaulted to: {this.Colors.Upgrades.UnavailableColor} / {this.Colors.Upgrades.UnavailableHoverColor}");
            }


            if (this.Colors.Upgrades.Innate != null && this.Colors.Upgrades.Innate.Length == 2)
            {
                this.Colors.Upgrades.InnateColor = new Color(
                    this.Colors.Upgrades.Innate[0][0],
                    this.Colors.Upgrades.Innate[0][1],
                    this.Colors.Upgrades.Innate[0][2],
                    this.Colors.Upgrades.Innate[0][3]
                    );
                this.Colors.Upgrades.InnateHoverColor = new Color(
                    this.Colors.Upgrades.Innate[1][0],
                    this.Colors.Upgrades.Innate[1][1],
                    this.Colors.Upgrades.Innate[1][2],
                    this.Colors.Upgrades.Innate[1][3]
                    );
                Mod.Log.Debug?.Write($" Innate set to: {this.Colors.Upgrades.Innate} / {this.Colors.Upgrades.InnateHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE_HOVER;
                Mod.Log.Debug?.Write($" Innate defaulted to: {this.Colors.Upgrades.Innate} / {this.Colors.Upgrades.InnateHoverColor}");
            }
        }

    }
}

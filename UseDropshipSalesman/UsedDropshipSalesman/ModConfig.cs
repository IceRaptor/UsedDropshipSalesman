
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UsedDropshipSalesman;
using UsedDropshipSalesman.Defs;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman
{
    public record DropshipConfig
    {
        CustomDropshipDef _customDropshipDef;
        public CustomDropshipDef CustomDropship { 
            get => _customDropshipDef;
            set {
                _customDropshipDef = value;
                this.PopulateUpgrades(value);
            }
        }

        // Initialized during config
        public List<String> AllUpgradeIds;
        public List<String> InnateUpgradeIds;

        private void PopulateUpgrades(CustomDropshipDef customDropshipDef)
        {
            Mod.Log.Debug?.Write(" -- Aggregating upgrades.");

            this.AllUpgradeIds = customDropshipDef.Upgrades.SelectMany(cats => customDropshipDef.Upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades.Union(sys.optionalUpgrades))
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Write($" All upgrades for dropship are: {String.Join(",", this.AllUpgradeIds)}");

            this.InnateUpgradeIds = customDropshipDef.Upgrades.SelectMany(cats => customDropshipDef.Upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades)
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Write($" Innate upgrades for dropship are: {String.Join(",", this.InnateUpgradeIds)}");
        }
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
                CustomDropshipDef customDropship = kvp.Value.CustomDropship;
                Mod.Log.Info?.Write($"\n  DropshipID: {kvp.Key}");
                Mod.Log.Info?.Write("  ---- PREFAB");
                Mod.Log.Info?.Write($" assetBundleID          : {customDropship.Visuals.AssetBundleId}");
                Mod.Log.Info?.Write($" prefabPath             : {customDropship.Visuals.AssetBundleId}");
                Mod.Log.Info?.Write($" attachDecal            : {customDropship.Visuals.AttachDecal}");
                Mod.Log.Info?.Write($" attachEngineGlow       : {customDropship.Visuals.AttachEngineGlow}");
                Mod.Log.Info?.Write($" attachesEngines        : {(customDropship.Visuals.AttachesEngines != null ? String.Join(",", customDropship.Visuals.AttachesEngines) : "None" )}");
                Mod.Log.Info?.Write($" attachesSpotLights     : {(customDropship.Visuals.AttachesSpotLights != null ? String.Join(",", customDropship.Visuals.AttachesSpotLights) : "None" )}");
                Mod.Log.Info?.Write($" attachesRunningLights  : {(customDropship.Visuals.AttachesRunningLights != null ? String.Join(",", customDropship.Visuals.AttachesRunningLights) : "None" )}");
                Mod.Log.Info?.Write("  ---- COSTS");
                Mod.Log.Info?.Write($" purchase: {customDropship.Costs.Purchase}  upkeep: {customDropship.Costs.Upkeep}  drop: {customDropship.Costs.Drop}");
                Mod.Log.Info?.Write("  ---- REQUIREMENTS");
                Mod.Log.Info?.Write($" factionRep: {customDropship.Requirements.FactionReputation}  mustBeAllied: {customDropship.Requirements.MustBeAllied}");
                Mod.Log.Info?.Write($" planetTags       : {String.Join(",", customDropship.Requirements.PlanetTags)}");
                Mod.Log.Info?.Write("  ---- REPAIR BAYS");
                foreach (KeyValuePair<string, int> kvpRB in customDropship?.HangarBays)
                {
                    Mod.Log.Info?.Write($"   hangarBay: {kvpRB.Key}  value: {kvpRB.Value}");
                }
                Mod.Log.Info?.Write("  ---- DROP BAYS");
                Mod.Log.Info?.Write($" maxTonnage: {customDropship?.DropBays?.MaxTonnage}");
                for (int i = 0; i < customDropship?.DropBays?.Labels?.Length; i++)
                {
                    Mod.Log.Info?.Write($"Lance: '{customDropship?.DropBays?.Labels[i]}' => [{String.Join(",", customDropship?.DropBays?.Slots[i])}]");
                }

                Mod.Log.Info?.Write("  ---- UPGRADES");
                foreach (var category in customDropship.Upgrades)
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
            
            ConvertColors();

            Mod.Log.Debug?.Write(" == Configuration Initialized");
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

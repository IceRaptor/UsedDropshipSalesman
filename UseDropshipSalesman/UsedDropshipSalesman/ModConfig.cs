
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
            Mod.Log.Debug?.Log(" -- Aggregating upgrades.");

            this.AllUpgradeIds = customDropshipDef.Upgrades.SelectMany(cats => customDropshipDef.Upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades.Union(sys.optionalUpgrades))
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Log($" All upgrades for dropship are: {String.Join(",", this.AllUpgradeIds)}");

            this.InnateUpgradeIds = customDropshipDef.Upgrades.SelectMany(cats => customDropshipDef.Upgrades)
                .SelectMany(cat => cat.Systems)
                .SelectMany(sys => sys.innateUpgrades)
                .Distinct()
                .ToList();
            Mod.Log.Debug?.Log($" Innate upgrades for dropship are: {String.Join(",", this.InnateUpgradeIds)}");
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

        public string FallbackDropship; // Used for an existing career with values; default to HBS_ARGO
        public Dictionary<string, string> CareerStartDropshipByPlanetName;
        public List<String> PersistentUpgrades; // TODO: Doc
        public ColorConfig Colors;

        public Dictionary<String, DropshipConfig> Dropships = new Dictionary<String, DropshipConfig>();

        public void LogConfig()
        {
            Mod.Log.Info?.Log("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Log($"  DEBUG:{this.Debug} Trace:{this.Trace}");

            Mod.Log.Info?.Log("  ---- STARTING DROPSHIPS");
            Mod.Log.Info?.Log($"Fallback dropshipId: {this.FallbackDropship}");
            foreach (KeyValuePair<string, string> kvp in this.CareerStartDropshipByPlanetName)
            {
                Mod.Log.Info?.Log($" planetName: {kvp.Key}  dropshipId: {kvp.Value}");
            }

            Mod.Log.Info?.Log("  ---- PERSISTENT UPGRADES");
            foreach (String upgrade in this.PersistentUpgrades)
            {
                Mod.Log.Info?.Log($" id: {upgrade}");
            }
            
            Mod.Log.Info?.Log("  ---- COLORS");
            Mod.Log.Info?.Log($" purchased: {this.Colors?.Upgrades?.PurchasedColor}  onHover: {this.Colors?.Upgrades?.PurchasedHoverColor}");
            Mod.Log.Info?.Log($" available: {this.Colors?.Upgrades?.AvailableColor}  onHover: {this.Colors?.Upgrades?.AvailableHoverColor}");
            Mod.Log.Info?.Log($" unavailable: {this.Colors?.Upgrades?.UnavailableColor}  onHover: {this.Colors?.Upgrades?.UnavailableHoverColor}");

            Mod.Log.Info?.Log("\n  --- DROPSHIPS CONFIG ---");
            foreach (KeyValuePair<string, DropshipConfig> kvp in Dropships)
            {
                CustomDropshipDef customDropship = kvp.Value.CustomDropship;
                Mod.Log.Info?.Log($"\n  DropshipID: {kvp.Key}");
                Mod.Log.Info?.Log("  ---- PREFAB");
                Mod.Log.Info?.Log($" assetBundleID          : {customDropship.Visuals.AssetBundleId}");
                Mod.Log.Info?.Log($" prefabPath             : {customDropship.Visuals.AssetBundleId}");
                Mod.Log.Info?.Log($" attachDecal            : {customDropship.Visuals.AttachDecal}");
                Mod.Log.Info?.Log($" attachEngineGlow       : {customDropship.Visuals.AttachEngineGlow}");
                Mod.Log.Info?.Log($" attachesEngines        : {(customDropship.Visuals.AttachesEngines != null ? String.Join(",", customDropship.Visuals.AttachesEngines) : "None" )}");
                Mod.Log.Info?.Log($" attachesSpotLights     : {(customDropship.Visuals.AttachesSpotLights != null ? String.Join(",", customDropship.Visuals.AttachesSpotLights) : "None" )}");
                Mod.Log.Info?.Log($" attachesRunningLights  : {(customDropship.Visuals.AttachesRunningLights != null ? String.Join(",", customDropship.Visuals.AttachesRunningLights) : "None" )}");
                Mod.Log.Info?.Log("  ---- COSTS");
                Mod.Log.Info?.Log($" purchase: {customDropship.Costs.Purchase}  upkeep: {customDropship.Costs.Upkeep}  drop: {customDropship.Costs.Drop}");
                Mod.Log.Info?.Log("  ---- REQUIREMENTS");
                Mod.Log.Info?.Log($" factionRep: {customDropship.Requirements.FactionReputation}  mustBeAllied: {customDropship.Requirements.MustBeAllied}");
                Mod.Log.Info?.Log($" planetTags       : {String.Join(",", customDropship.Requirements.PlanetTags)}");
                Mod.Log.Info?.Log("  ---- BERTHS");
                Mod.Log.Info?.Log($" maxPilots: {customDropship.Berths.MaxPilots}");

                Mod.Log.Info?.Log("  ---- HANGAR BAYS");
                foreach (KeyValuePair<string, int> kvpRB in customDropship?.HangarBays)
                {
                    Mod.Log.Info?.Log($"   hangarBay: {kvpRB.Key}  value: {kvpRB.Value}");
                }
                Mod.Log.Info?.Log("  ---- DROP BAYS");
                Mod.Log.Info?.Log($" maxTonnage: {customDropship?.DropBays?.MaxTonnage}");
                for (int i = 0; i < customDropship?.DropBays?.Labels?.Length; i++)
                {
                    Mod.Log.Info?.Log($"Lance: '{customDropship?.DropBays?.Labels[i]}' => [{String.Join(",", customDropship?.DropBays?.Slots[i])}]");
                }

                Mod.Log.Info?.Log("  ---- UPGRADES");
                foreach (var category in customDropship.Upgrades)
                {
                    Mod.Log.Info?.Log($" -------- categoryID: {category.CategoryId}  headerText: {category.HeaderText}  icon: {category.Icon}");
                    foreach (var system in category.Systems)
                    {
                        Mod.Log.Info?.Log($"          systemId        : {system.SystemId}  headerText: {system.HeaderText}");
                        Mod.Log.Info?.Log($"          innateUpgrades  : {String.Join(",", system.innateUpgrades)}");
                        Mod.Log.Info?.Log($"          optionalUpgrades: {String.Join(",", system.optionalUpgrades)}");
                    }
                }
            }

            Mod.Log.Info?.Log("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            Mod.Log.Debug?.Log(" == Initializing Configuration");
            
            ConvertColors();

            Mod.Log.Debug?.Log(" == Configuration Initialized");
        }

        

        private void ConvertColors()
        {
            Mod.Log.Debug?.Log(" -- Converting colors.");

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
                Mod.Log.Debug?.Log($" Purchased set to: {this.Colors.Upgrades.Purchased} / {this.Colors.Upgrades.PurchasedHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER;
                Mod.Log.Debug?.Log($" Purchased defaulted to: {this.Colors.Upgrades.Purchased} / {this.Colors.Upgrades.PurchasedHoverColor}");
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
                Mod.Log.Debug?.Log($" Available set to: {this.Colors.Upgrades.AvailableColor} / {this.Colors.Upgrades.AvailableHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER;
                Mod.Log.Debug?.Log($" Available defaulted to: {this.Colors.Upgrades.AvailableColor} / {this.Colors.Upgrades.AvailableHoverColor}");
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
                Mod.Log.Debug?.Log($" Unavailable set to: {this.Colors.Upgrades.UnavailableColor} / {this.Colors.Upgrades.UnavailableHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER;
                Mod.Log.Debug?.Log($" Unavailable defaulted to: {this.Colors.Upgrades.UnavailableColor} / {this.Colors.Upgrades.UnavailableHoverColor}");
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
                Mod.Log.Debug?.Log($" Innate set to: {this.Colors.Upgrades.Innate} / {this.Colors.Upgrades.InnateHoverColor}");
            }
            else
            {
                this.Colors.Upgrades.PurchasedColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE;
                this.Colors.Upgrades.PurchasedHoverColor = ModConsts.UPGRADE_COLOR_DEFAULT_INNATE_HOVER;
                Mod.Log.Debug?.Log($" Innate defaulted to: {this.Colors.Upgrades.Innate} / {this.Colors.Upgrades.InnateHoverColor}");
            }
        }

    }
}

using JwTweaks.Data;
using JwTweaks.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Defs
{
    [JsonObject]
    public class CustomDropshipDef
    {
        [JsonRequired]
        public DropshipDescription Description { get; set; }
        [JsonRequired]
        public DropshipVisuals Visuals { get; set; }
        public DropshipCosts Costs { get; set; }
        public DropshipRequirements Requirements { get; set; }
        public Dictionary<string, int> HangarBays { get; set; }
        public DropshipDropBays DropBays { get; set; }
        [JsonRequired]
        public List<DropshipUpgradeCategory> Upgrades { get; set; }
        
        public bool Validate()
        {
            bool isValid = true;

            if (this.Description == null || this.Description.Id == null) 
            {
                Mod.Log.Debug?.Write("CustomDropship has no description field or ID!");
                isValid = false; 
            }
            Mod.Log.Debug?.Write($"Validating CustomDropshipDef with id: {this.Description.Id}");

            if (this.Visuals == null)
            {
                Mod.Log.Debug?.Write("Dropship Visuals not defined!");
                isValid = false;
            }
            else if (String.IsNullOrEmpty(this.Visuals.AssetBundleId))
            {
                Mod.Log.Debug?.Write("CustomDropship missing assetBundleID!");
                isValid = false;
            }
            else if (String.IsNullOrEmpty(this.Visuals.PrefabPath) && !this.Visuals.AssetBundleId.StartsWith("HBS_"))
            {
                Mod.Log.Debug?.Write("CustomDropship with custom assetbundle missing prefabPath!");
                isValid = false;
            }

            // TODO: Validate default ID is present
            // TODO: Soft validate expected values


            // Validate dropbays
            if (this.DropBays == null)
            {
                Mod.Log.Debug?.Write("Dropship dropBays not defined!");
            }
            else if (this.DropBays?.Labels?.Length != this.DropBays?.Slots?.Length)
            {
                Mod.Log.Warn?.Write("Dropship dropBays labels and slots do not match, is not valid!");
            }

            if (this.Upgrades == null || this.Upgrades.Count <= 1)
            {
                Mod.Log.Debug?.Write("CustomDropship has no upgrades!");
                isValid = false;
            }

            return isValid;
        }
    }

    public record DropshipDescription
    {
        public string Id;
        public string Name;
        public string Icon;
    }

    public record DropshipVisuals
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

    public record DropshipDropBays
    {
        public String[] Labels;
        public int MaxTonnage;
        public String[][] Slots;
    }

    public record DropshipUpgradeCategory
    {
        public string CategoryId;
        public string HeaderText;
        public string Icon;
        public List<DropshipUpgradeSystem> Systems;
    }

    public record DropshipUpgradeSystem
    {
        public string SystemId;
        public string HeaderText;
        public List<string> innateUpgrades;
        public List<string> optionalUpgrades;

        // Derived from argoUpgradeDefs
        public List<DropshipUpgradeItem> ItemUpgrades;

    }

    public record DropshipUpgradeItem
    {
        public string Name;
        public string Description;
        public string Icon;
    }

}

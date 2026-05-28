
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UsedDropshipSalesman;

namespace UsedDropshipSalesman
{
    public record DropshipConfig
    {
        public DropshipPrefabConfig prefab;
        public DropshipCosts costs;
        public DropshipRequirements requirements;
        public DropshipBays bays;

        public String[] InnateUpgrades;
        public String[] OptionalUpgrades;
    }

    public record DropshipPrefabConfig
    {
        public String AssetBundleId;
        public String PrefabPath;
        public String Attach_EngineGlow;
        public String Attach_Decal;
        public List<String> Attaches_Engines;

        // TODO:TBD
        public List<String> Attaches_SpotLights;
        public List<String> Attaches_RunningLights;
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


            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            Mod.Log.Debug?.Write(" == Initializing Configuration");


            Mod.Log.Debug?.Write(" == Configuration Initialized");
        }

    }
}

using CustomUnits;
using CustomUnits.CustomHangars;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using static CustomUnits.CustomHangars.CustomHangarHelper;

namespace UsedDropshipSalesman
{

    public static class Mod
    {

        public const string HarmonyPackage = "us.frostraptor.UsedDropshipSalesman";
        public const string LogName = "used_dropship_salesman";
        public const string LogLabel = "USEDDROPSHIP";

        public static DeferringLogger Log;
        public static string ModDir;
        public static ModConfig Config;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception settingsE = null;
            try
            {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            }
            catch (Exception e)
            {
                settingsE = e;
                Mod.Config = new ModConfig();
            }

            Log = new DeferringLogger(modDirectory, LogName, LogLabel, Config.Debug, Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.FileVersion}");

            // Initialize the mod settings
            Mod.Config.Init();

            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            if (settingsE != null)
            {
                Log.Info?.Write($"ERROR reading settings file! Error was: {settingsE}");
            }
            else
            {
                Log.Info?.Write($"INFO: No errors reading settings file.");
            }

            // Initialize modules
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }

        public static void FinishedLoading()
        {
            Mod.Log.Trace?.Write("==== ModInit::FinishedLoading invoked.");

            // Setup the dropship size constraints in CU
            Dictionary<string, CustomHangarConstraint> constraints = new Dictionary<string, CustomHangarConstraint>();
            constraints[CustomHangarHelper.HANGAR_ID_BASE] = new CustomHangarConstraint() { MaxUnitsPerPod = 8 };
            constraints["vehicle_bays"] = new CustomHangarConstraint() { MaxUnitsPerPod = 3 };
            constraints["battle_armor_bays"] = new CustomHangarConstraint() { MaxUnitsPerPod = 14 };

            CustomHangarHelper.SetConstraints(constraints, Mod.LogLabel);

           
        }
    }
}

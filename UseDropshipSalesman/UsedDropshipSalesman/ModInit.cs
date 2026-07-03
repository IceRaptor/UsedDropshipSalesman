using CustomUnits.CustomHangars;
using IRBTModUtils.Logging;
using JwTweaks.Data;
using JwTweaks.Features;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UsedDropshipSalesman.Data;
using UsedDropshipSalesman.Defs;
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
        public static UDSSaveData ModSaveData = new UDSSaveData();

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

            // Initialize the custom save block
            JsonSaveBlock<UDSSaveData> udsSaveDataBlock = new()
            {
                Data = Mod.ModSaveData
            };
            SaveSerializationManager.RegisterCustomSaveBlock(udsSaveDataBlock, "UDSSaveDataBlock");

            // Initialize modules
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }

        public static void FinishedLoading(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            Mod.Log.Trace?.Write("==== ModInit::FinishedLoading invoked.");

            // Setup the dropship size constraints in CU
            Dictionary<string, CustomHangarConstraint> constraints = new Dictionary<string, CustomHangarConstraint>();
            constraints[CustomHangarHelper.BASE_HANGAR_ID] = new CustomHangarConstraint() { MaxAvailableUnits = 8 };
            constraints["vehicle_bays"] = new CustomHangarConstraint() { MaxAvailableUnits = 3 };
            constraints["battle_armor_bays"] = new CustomHangarConstraint() { MaxAvailableUnits = 14 };

            CustomHangarHelper.SetConstraints(constraints, Mod.LogLabel);

            // Load the dropship configs
            if (customResources != null && customResources.Count > 0)
            {

                bool hasResources = customResources.TryGetValue(ModConsts.CUSTOM_RESOURCE_DROPSHIP_CONFIG, out Dictionary<string, VersionManifestEntry> dropshipConfigEntries);
                foreach (KeyValuePair<string, VersionManifestEntry> kvp in dropshipConfigEntries)
                {
                    Mod.Log.Debug?.Write($"Loading customDropshipDef: {kvp.Key} from path: {kvp.Value.FilePath}");
                    try
                    { 
                        string fileContent = File.ReadAllText(kvp.Value.FilePath);
                        Mod.Log.Trace?.Write($"Deserializing context to CustomDropshipDef:\n'{fileContent}'");
                        CustomDropshipDef dropshipDef = JsonConvert.DeserializeObject<CustomDropshipDef>(fileContent);

                        bool isValid = dropshipDef.Validate();
                        if (isValid)
                        {
                            Mod.Log.Debug?.Write($"Adding dropshipDef with ID: {dropshipDef.Description.Id} to available dropships.");
                            DropshipConfig newConfig = new DropshipConfig() { CustomDropship = dropshipDef };
                            Mod.Config.Dropships.Add(dropshipDef.Description.Id, newConfig);

                        }
                        else
                        {
                            Mod.Log.Warn?.Write($"Dropship {dropshipDef?.Description?.Id} failed validation, skipping.!");
                            var jsonString = JsonConvert.SerializeObject(dropshipDef, Formatting.Indented, 
                                new JsonConverter[] { new StringEnumConverter() });
                            Mod.Log.Warn?.Write($" -- Generated object: {jsonString}");
                        }

                    }
                    catch (Exception ex)
                    {
                        Mod.Log.Warn?.Write(ex, "Failed read custom resource!");
                    }
                }
            }
        }




    }
}

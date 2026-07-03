using CustomAmmoCategoriesLog;
using CustomUnits.CustomHangars;
using HBS.Logging;
using IRBTModUtils.Logging;
using JwTweaks.Data;
using JwTweaks.Features;
using ModTek.Public;
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

        internal static NullableLogger Log = NullableLogger.GetLogger("UsedDropshipSalesman", NullableLogger.TraceLogLevel);
        internal static string ModDir;
        internal static ModConfig Config;
        public static UDSSaveData ModSaveData = new UDSSaveData();

        //public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception settingsE = null;
            try
            {
                string settingsFile = Path.Combine(modDirectory, "settings.json");
                using StreamReader reader = new(settingsFile);
                string settingsText = reader.ReadToEnd();
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsText);
            }
            catch (Exception e)
            {
                settingsE = e;
                Mod.Config = new ModConfig();
            }

            //Log = new DeferringLogger(modDirectory, LogName, LogLabel, Config.Debug, Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Log($"AssemblyVersion: {fvi.FileVersion}");

            // Initialize the mod settings
            Mod.Config.Init();

            Log.Debug?.Log($"ModDir is:{modDirectory}");
            Log.Debug?.Log($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            if (settingsE != null)
            {
                Log.Info?.Log($"ERROR reading settings file! Error was: {settingsE}");
            }
            else
            {
                Log.Info?.Log($"INFO: No errors reading settings file.");
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
            Mod.Log.Trace?.Log("==== ModInit::FinishedLoading invoked.");

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
                    Mod.Log.Debug?.Log($"Loading customDropshipDef: {kvp.Key} from path: {kvp.Value.FilePath}");
                    try
                    { 
                        string fileContent = File.ReadAllText(kvp.Value.FilePath);
                        Mod.Log.Trace?.Log($"Deserializing context to CustomDropshipDef:\n'{fileContent}'");
                        CustomDropshipDef dropshipDef = JsonConvert.DeserializeObject<CustomDropshipDef>(fileContent);

                        bool isValid = dropshipDef.Validate();
                        if (isValid)
                        {
                            Mod.Log.Debug?.Log($"Adding dropshipDef with ID: {dropshipDef.Description.Id} to available dropships.");
                            DropshipConfig newConfig = new DropshipConfig() { CustomDropship = dropshipDef };
                            Mod.Config.Dropships.Add(dropshipDef.Description.Id, newConfig);

                        }
                        else
                        {
                            Mod.Log.Warning?.Log($"Dropship {dropshipDef?.Description?.Id} failed validation, skipping.!");
                            var jsonString = JsonConvert.SerializeObject(dropshipDef, Formatting.Indented, 
                                new JsonConverter[] { new StringEnumConverter() });
                            Mod.Log.Warning?.Log($" -- Generated object: {jsonString}");
                        }

                    }
                    catch (Exception ex)
                    {
                        Mod.Log.Warning?.Log("Failed read custom resource!", ex);
                    }
                }
            }
        }




    }
}

using BattleTech.Data;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using CustomUnits.CustomHangars;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomUnits.CustomHangars.CustomHangarHelper;

namespace UsedDropshipSalesman.Helper
{

    public static class UpgradeHelper
    {
        // Invoke CU lance control APIs to fixate drop sizes
        public static void UpdateDropConfig(DropshipConfig config)
        {
            int totalUnits = 0;
            List<List<string>> layout = new List<List<string>>();
            foreach (var slot in config.CustomDropship.DropBays.Slots)
            {
                totalUnits += slot.Length;
                layout.Add(slot.ToList());
            }

            var labels = config.CustomDropship.DropBays.Labels.ToList();
            Mod.Log.Info?.Write($"Updating CU dropConfig to support {totalUnits} across {layout.Count} lances.");
            CustomLanceHelper.PushDropLayout(config.CustomDropship.Description.Id, layout, totalUnits, labels);
        }

        public static void UpdateHangarConfig(DropshipConfig config)
        {
            Mod.Log.Info?.Write($"Updating CU hangarConfig to support hangars: ");
            foreach (KeyValuePair<string, int> kvp in config.CustomDropship.HangarBays)
            {
                Mod.Log.Info?.Write($" -- bay: {kvp.Key}  value: {kvp.Value}");
            }

            Dictionary<string, CustomHangarConstraint> constraints;
            constraints = config.CustomDropship.HangarBays.ToDictionary(x => x.Key, y => new CustomHangarConstraint() { MaxAvailableUnits= y.Value });

            CustomHangarHelper.SetConstraints(constraints, Mod.LogName);
        }

        public static void ApplyUpgrades(DropshipConfig newConfig, SimGameState sim)
        {
            if (newConfig == null) { return; }
            if (newConfig.InnateUpgradeIds == null || newConfig.InnateUpgradeIds.Count == 0)
            {
                Mod.Log.Info?.Write($"Dropship {newConfig.CustomDropship.Description.Id} has no innate upgrades.");
            }

            var AllShipUpgrades = sim.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade, false);
            foreach (VersionManifestEntry vme in AllShipUpgrades)
            {
                ShipModuleUpgrade smu = sim.DataManager.ShipUpgradeDefs.Get(vme.Id);
                if (smu == null || smu.Description == null || smu.Description.Id == null || String.IsNullOrEmpty(smu?.Description?.Id))
                {
                    Mod.Log.Debug?.Write($"Could not read shipModuleUpgrade from versionManifest: {vme.Id}");
                    continue;
                }

                if (newConfig.InnateUpgradeIds.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Write($"New config has innate module {vme.Id}, applying changes");
                    sim.AddArgoUpgrade(smu);
                }
            }

        }

        public static void RevertUpgrades(DropshipConfig configToRevert, SimGameState sim)
        {
            if (configToRevert == null) { return; }
            if (configToRevert.AllUpgradeIds.Count == 0)
            {
                Mod.Log.Warn?.Write($"DropshipConfig improperly initialized, no upgrades found in AllUpgradeIds. Skipping!");
                return;
            }

            var AllShipUpgrades = sim.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade, false);
            Mod.Log.Info?.Write($"Iterating over {AllShipUpgrades.Length} ShipModuleUpgrade Defs");
            foreach (VersionManifestEntry vme in AllShipUpgrades)
            {
                ShipModuleUpgrade smu = sim.DataManager.ShipUpgradeDefs.Get(vme.Id);
                if (smu == null || smu.Description == null || smu.Description.Id == null || String.IsNullOrEmpty(smu?.Description?.Id)) 
                {
                    Mod.Log.Debug?.Write($"Could not read shipModuleUpgrade from versionManifest: {vme.Id}");
                    continue;
                }

                // Check for persistent configuration items; skip reverting these
                if (Mod.Config.PersistentUpgrades.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Write($"ShipModule {vme.Id} marked as persistent, not reverting.");
                    continue;
                }

                if (configToRevert.AllUpgradeIds.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Write($"Current ship state has module {vme.Id}, reverting Tag and Stat changes");
                    
                    if (smu.Tags != null && !smu.Tags.IsEmpty)
                    {
                        Mod.Log.Debug?.Write($" -- Removing tags: {String.Join(",", smu.Tags)}");
                        sim.CompanyTags.RemoveRange(smu.Tags);
                    }

                    SimGameStat[] stats = smu.Stats;
                    foreach (SimGameStat companyStat in stats)
                    {
                        Mod.Log.Debug?.Write($" -- Removing statistic: {companyStat.name}");
                        sim.CompanyStats.RemoveStatistic(companyStat.name);
                    }
                }
            }

            foreach (String upgradeId in configToRevert.AllUpgradeIds)
            {

            }
        }
    }

}

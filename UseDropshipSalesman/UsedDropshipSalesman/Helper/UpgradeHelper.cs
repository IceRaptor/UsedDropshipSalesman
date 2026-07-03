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
using TMPro;
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

            // TODO: Pull upgrades from save state instead of current mod config

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
        }

        public static bool CheckForPendingUpgrades(SimGameState sgs)
        {
            bool hasUpgrades = sgs.CurrentUpgradeEntry != null;



            Mod.Log.Debug?.Write("No pending ShipModuleUpgrades detected");
            return hasUpgrades;


        }

        public static bool IsUpgradeBlocked(DropshipConfig newConfig, DropshipConfig oldConfig, SimGameState sgs)
        {
            bool hasBlockingIssues = false;

            List<String> blockedReasons = new List<String>(); 
            // Check for pending upgrades
            if (sgs.CurrentUpgradeEntry != null)
            {
                Mod.Log.Info?.Write($"Active argo upgrade, must cancel upgrade");
                // TODO: LOCALIZE 
                blockedReasons.Add("Pending Ship Upgrade");

                hasBlockingIssues = true;
            }

            // Check for hangarbay storage delta
            Dictionary<string, int> countByHangarId = new Dictionary<string, int>();
            Mod.Log.Debug?.Write($" Counting current active mechs.");
            foreach (KeyValuePair<int, MechDef> kvp in sgs.ActiveMechs)
            {
                // kvp.key is the index of the mechbay; CU will make these in the range of 0-max, 100-max, 200-max, etc
                Mod.Log.Debug?.Write($" Found mech: {kvp.Value?.ChassisID} with count: {kvp.Key}");
                CustomHangarDef customHangarDef = CustomHangarHelper.HangarDef(kvp.Value.Chassis);
                Mod.Log.Debug?.Write($" Found CustomHangarDef: {customHangarDef?.Description?.Id}");
                // If the hangarDef is null, the unit goes into the default bay (typically MechBay)
                string hangarId = customHangarDef?.Description?.Id ?? CustomHangarHelper.BASE_HANGAR_ID;
                Mod.Log.Debug?.Write($"Adding {kvp.Key} active units with chassis: {kvp.Value.ChassisID} to hangerDefId: {hangarId}");

                if (countByHangarId.ContainsKey(hangarId)) { countByHangarId[hangarId] += 1; }
                else { countByHangarId.Add(hangarId, 1); }
            }
            Mod.Log.Debug?.Write($" Counting current readying mechs.");
            foreach (KeyValuePair<int, MechDef> kvp in sgs.ReadyingMechs)
            {
                Mod.Log.Debug?.Write($" Found mech: {kvp.Value?.ChassisID} with count: {kvp.Key}");
                CustomHangarDef customHangarDef = CustomHangarHelper.HangarDef(kvp.Value.Chassis);
                Mod.Log.Debug?.Write($" Found CustomHangarDef: {customHangarDef?.Description?.Id}");
                // If the hangarDef is null, the unit goes into the default bay (typically MechBay)
                string hangarId = customHangarDef?.Description?.Id ?? CustomHangarDef.DEFAULT_VEHICLE_HANGAR_ID;
                Mod.Log.Debug?.Write($"Adding {kvp.Key} readying units with chassis: {kvp.Value.ChassisID} to hangerDefId: {hangarId}");

                if (countByHangarId.ContainsKey(hangarId)) { countByHangarId[hangarId] += 1; }
                else { countByHangarId.Add(hangarId, 1); }
            }

            List<string> allHangars = allHangars = CustomHangarHelper.listHangars.Select(chd => chd.Description.Id).ToList();
            allHangars.Insert(0, CustomHangarHelper.BASE_HANGAR_ID);
            foreach (string hangarId in allHangars)
            {
                Mod.Log.Debug?.Write($"Evaluating count on hangarDef: {hangarId}");
                if (countByHangarId.ContainsKey(hangarId) && newConfig.CustomDropship.HangarBays.ContainsKey(hangarId))
                {
                    int newConstraintSize = newConfig.CustomDropship.HangarBays.ContainsKey(hangarId) ? 
                        newConfig.CustomDropship.HangarBays[hangarId] : 0;
                    if (countByHangarId[hangarId] > newConstraintSize) 
                    {
                        Mod.Log.Info?.Write($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {countByHangarId[hangarId]} units active or readying.");
                        // TODO: LOCALIZE 
                        blockedReasons.Add($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {countByHangarId[hangarId]} units active or readying.");
                        hasBlockingIssues = true;
                    }
                }
            } 

            // Check for mechbay changes
            //if (sgs.MechLabQueue.Count > 0)
            //{
            //    Mod.Log.Info?.Write($"Active mechlab upgrades happening");
            //}

            // Check for medbay changes?

            // Check for pilot limits

            if (hasBlockingIssues)
            {
                // TODO: LOCALIZE 
                string upgradeFailureText = "Your dropship cannot be changed until you resolve the following issues:\n";
                foreach (string reason in blockedReasons)
                {
                    upgradeFailureText += "\n - " + reason;
                }
                upgradeFailureText += "\n\nCancel active ship upgrades or readying mechs in the timeline view from the main screen. " +
                    "Store active units or cancel reading actions to reduce the hangar count to match the new dropship. " +
                    "\nDo not navigate away from the current star system until you resolve these actions. " + 
                    "This dialog will repeat each day until the conditions are cleared.\n<b>You cannot cancel the dropship upgrade.</b>";
                GenericPopup gp = GenericPopupBuilder.Create("Dropship Upgrade Failed", upgradeFailureText)
                        //.AddButton(localButtonAccept, acceptAction, true, null)                        
                        .AddButton("OKAY", null, true, null)
                        .Render();

                    TextMeshProUGUI contentText = gp._contentText;
                    contentText.alignment = TextAlignmentOptions.Left;
            }

            return hasBlockingIssues;
        }

    }

}

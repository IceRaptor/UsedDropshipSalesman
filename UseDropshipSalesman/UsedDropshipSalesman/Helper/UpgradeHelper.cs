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
            Mod.Log.Info?.Log($"Updating CU dropConfig to support {totalUnits} across {layout.Count} lances.");
            CustomLanceHelper.PushDropLayout(config.CustomDropship.Description.Id, layout, totalUnits, labels);
        }

        public static void UpdateHangarConfig(DropshipConfig config)
        {
            Mod.Log.Info?.Log($"Updating CU hangarConfig to support hangars: ");
            foreach (KeyValuePair<string, int> kvp in config.CustomDropship.HangarBays)
            {
                Mod.Log.Info?.Log($" -- bay: {kvp.Key}  value: {kvp.Value}");
            }

            Dictionary<string, CustomHangarConstraint> constraints;
            constraints = config.CustomDropship.HangarBays.ToDictionary(x => x.Key, y => new CustomHangarConstraint() { MaxAvailableUnits= y.Value });

            CustomHangarHelper.SetConstraints(constraints, Mod.LogName);
        }

        private static void DeDupeSGSShipUpgrades(SimGameState sgs)
        {
            // De-duplicate any items in ShipUpgrades
            Dictionary<string, ShipModuleUpgrade> allUpgrades = new();
            foreach (ShipModuleUpgrade smu in sgs.ShipUpgrades)
            {
                if (smu.Description == null || String.IsNullOrEmpty(smu.Description.Name))
                {
                    continue; // Don't want empty values floating around
                }
                if (allUpgrades.ContainsKey(smu.Description.Id))
                {
                    continue;
                }
                allUpgrades.Add(smu.Description.Id, smu);
            }

            sgs.shipUpgrades = allUpgrades.Values.ToList();
        }

        public static void ApplyUpgrades(DropshipConfig newConfig, SimGameState sim)
        {
            if (newConfig == null) { return; }
            if (newConfig.InnateUpgradeIds == null || newConfig.InnateUpgradeIds.Count == 0)
            {
                Mod.Log.Info?.Log($"Dropship {newConfig.CustomDropship.Description.Id} has no innate upgrades.");
            }

            var AllShipUpgrades = sim.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade, false);
            foreach (VersionManifestEntry vme in AllShipUpgrades)
            {
                ShipModuleUpgrade smu = sim.DataManager.ShipUpgradeDefs.Get(vme.Id);
                if (smu == null || smu.Description == null || smu.Description.Id == null || String.IsNullOrEmpty(smu?.Description?.Id))
                {
                    Mod.Log.Debug?.Log($"Could not read shipModuleUpgrade from versionManifest: {vme.Id}");
                    continue;
                }

                if (newConfig.InnateUpgradeIds.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Log($"New config has innate module {vme.Id}, applying changes");
                    sim.AddArgoUpgrade(smu);
                }
                else if (Mod.ModSaveData.PurchasedPersistentUpgrades.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Log($"Purchased persistent module found, reapplying: {smu.Description.Id}");
                    sim.AddArgoUpgrade(smu);
                }
            }

            // TODO: Should be unncessary with changes
            DeDupeSGSShipUpgrades(sim);
        }

        public static void RevertUpgrades(DropshipConfig configToRevert, SimGameState sim)
        {
            if (configToRevert == null) { return; }
            if (configToRevert.AllUpgradeIds.Count == 0)
            {
                Mod.Log.Warning?.Log($"DropshipConfig improperly initialized, no upgrades found in AllUpgradeIds. Skipping!");
                return;
            }

            // TODO: Pull upgrades from save state instead of current mod config

            var AllShipUpgrades = sim.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade, false);
            Mod.Log.Info?.Log($"Reverting upgrades - iterating over {AllShipUpgrades.Length} ShipModuleUpgrades");
            foreach (VersionManifestEntry vme in AllShipUpgrades)
            {
                ShipModuleUpgrade smu = sim.DataManager.ShipUpgradeDefs.Get(vme.Id);
                if (smu == null || smu.Description == null || smu.Description.Id == null || String.IsNullOrEmpty(smu?.Description?.Id)) 
                {
                    Mod.Log.Debug?.Log($"Could not read shipModuleUpgrade from versionManifest: {vme.Id}");
                    continue;
                }

                // Check for persistent configuration items and move them into save-state. ApplyUpgrade should add them
                if (Mod.Config.PersistentUpgrades.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Log($"ShipModule {smu.Description.Id} marked as persistent, marking as a purchased upgrades");
                    if (!Mod.ModSaveData.PurchasedPersistentUpgrades.Contains(smu.Description.Id))
                    {
                        Mod.ModSaveData.PurchasedPersistentUpgrades.Add(smu.Description.Id);
                    }
                }

                if (configToRevert.AllUpgradeIds.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Log($"Current ship state has module {vme.Id}, reverting Tag and Stat changes");
                    
                    if (smu.Tags != null && !smu.Tags.IsEmpty)
                    {
                        Mod.Log.Debug?.Log($" -- Removing tags: {String.Join(",", smu.Tags)}");
                        sim.CompanyTags.RemoveRange(smu.Tags);
                    }

                    SimGameStat[] stats = smu.Stats;
                    foreach (SimGameStat companyStat in stats)
                    {
                        Mod.Log.Debug?.Log($" -- Removing statistic: {companyStat.name}");
                        sim.CompanyStats.RemoveStatistic(companyStat.name);
                    }
                }

                if (ModConsts.BASEGAME_DEFAULT_ARGO_UPGRADES.Contains(smu.Description.Id))
                {
                    Mod.Log.Debug?.Log($"Base game default argo module {vme.Id} found, reverting Tag and Stat changes");

                    if (smu.Tags != null && !smu.Tags.IsEmpty)
                    {
                        Mod.Log.Debug?.Log($" -- Removing tags: {String.Join(",", smu.Tags)}");
                        sim.CompanyTags.RemoveRange(smu.Tags);
                    }

                    SimGameStat[] stats = smu.Stats;
                    foreach (SimGameStat companyStat in stats)
                    {
                        Mod.Log.Debug?.Log($" -- Removing statistic: {companyStat.name}");
                        sim.CompanyStats.RemoveStatistic(companyStat.name);
                    }
                }
            }

            sim.purchasedArgoUpgrades.Clear();
            sim.shipUpgrades.Clear();

            // There should be NO upgrades at this point
            if (sim.ShipUpgrades.Count > 0)
            {
                String upgrades = String.Join(", ", sim.ShipUpgrades.Select(su => su.Description.Id).ToList());
                Mod.Log.Warning?.Log($"Ship still has {sim.ShipUpgrades.Count} upgrades after being reverted. This should not happen!\n" +
                    $"  Upgrades list: {upgrades}");
            }
            if (sim.PurchasedArgoUpgrades.Count >0)
            {
                String upgrades = String.Join(", ", sim.ShipUpgrades.Select(su => su.Description.Id).ToList());
                Mod.Log.Warning?.Log($"Ship still has {sim.ShipUpgrades.Count} purchased upgrades after being reverted. This should not happen!\n" +
                    $"  Upgrades list: {upgrades}");
            }

            // TODO: Should be unnecessary with changes
            DeDupeSGSShipUpgrades(sim);
        }

        public static bool IsUpgradeBlocked(DropshipConfig newConfig, DropshipConfig oldConfig, SimGameState sgs)
        {
            bool hasBlockingIssues = false;

            List<String> blockedReasons = new List<String>(); 
            // Check for pending upgrades
            if (sgs.CurrentUpgradeEntry != null)
            {
                Mod.Log.Info?.Log($"Active argo upgrade, must cancel upgrade");
                // TODO: LOCALIZE 
                blockedReasons.Add("Pending Ship Upgrade");

                hasBlockingIssues = true;
            }

            // Check for hangarbay storage delta
            Dictionary<string, int> countByHangarId = new Dictionary<string, int>();
            Mod.Log.Debug?.Log($" Counting current active mechs.");
            foreach (KeyValuePair<int, MechDef> kvp in sgs.ActiveMechs)
            {
                // kvp.key is the index of the mechbay; CU will make these in the range of 0-max, 100-max, 200-max, etc
                Mod.Log.Debug?.Log($" Found mech: {kvp.Value?.ChassisID} with count: {kvp.Key}");
                CustomHangarDef customHangarDef = CustomHangarHelper.HangarDef(kvp.Value.Chassis);
                Mod.Log.Debug?.Log($" Found CustomHangarDef: {customHangarDef?.Description?.Id}");
                // If the hangarDef is null, the unit goes into the default bay (typically MechBay)
                string hangarId = customHangarDef?.Description?.Id ?? CustomHangarHelper.BASE_HANGAR_ID;
                Mod.Log.Debug?.Log($"Adding {kvp.Key} active units with chassis: {kvp.Value.ChassisID} to hangerDefId: {hangarId}");

                if (countByHangarId.ContainsKey(hangarId)) { countByHangarId[hangarId] += 1; }
                else { countByHangarId.Add(hangarId, 1); }
            }
            Mod.Log.Debug?.Log($" Counting current readying mechs.");
            foreach (KeyValuePair<int, MechDef> kvp in sgs.ReadyingMechs)
            {
                Mod.Log.Debug?.Log($" Found mech: {kvp.Value?.ChassisID} with count: {kvp.Key}");
                CustomHangarDef customHangarDef = CustomHangarHelper.HangarDef(kvp.Value.Chassis);
                Mod.Log.Debug?.Log($" Found CustomHangarDef: {customHangarDef?.Description?.Id}");
                // If the hangarDef is null, the unit goes into the default bay (typically MechBay)
                string hangarId = customHangarDef?.Description?.Id ?? CustomHangarDef.DEFAULT_VEHICLE_HANGAR_ID;
                Mod.Log.Debug?.Log($"Adding {kvp.Key} readying units with chassis: {kvp.Value.ChassisID} to hangerDefId: {hangarId}");

                if (countByHangarId.ContainsKey(hangarId)) { countByHangarId[hangarId] += 1; }
                else { countByHangarId.Add(hangarId, 1); }
            }

            List<string> allHangars = allHangars = CustomHangarHelper.listHangars.Select(chd => chd.Description.Id).ToList();
            allHangars.Insert(0, CustomHangarHelper.BASE_HANGAR_ID);
            foreach (string hangarId in allHangars)
            {
                Mod.Log.Debug?.Log($"Evaluating count on hangarDef: {hangarId}");
                if (countByHangarId.ContainsKey(hangarId) && newConfig.CustomDropship.HangarBays.ContainsKey(hangarId))
                {
                    int newConstraintSize = newConfig.CustomDropship.HangarBays.ContainsKey(hangarId) ? 
                        newConfig.CustomDropship.HangarBays[hangarId] : 0;
                    if (countByHangarId[hangarId] > newConstraintSize) 
                    {
                        Mod.Log.Info?.Log($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {countByHangarId[hangarId]} units active or readying.");
                        // TODO: LOCALIZE 
                        blockedReasons.Add($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {countByHangarId[hangarId]} units active or readying.");
                        hasBlockingIssues = true;
                    }
                }
            }

            // Check for mechbay changes
            //if (sgs.MechLabQueue.Count > 0)
            //{
            //    Mod.Log.Info?.Log($"Active mechlab upgrades happening");
            //}

            // Check for medbay changes?

            // Check for pilot limits
            int currentPilotCount = sgs.PilotRoster.Count;
            Mod.Log.Debug?.Log($"New dropship has {newConfig.CustomDropship.Berths.MaxPilots} berths for {currentPilotCount} pilots");
            if (currentPilotCount > newConfig.CustomDropship.Berths.MaxPilots)
            {
                blockedReasons.Add($"New dropship berth limit is {newConfig.CustomDropship.Berths.MaxPilots}, but there are {currentPilotCount} active pilots.");
                hasBlockingIssues = true;
            }

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
                    "Dismiss pilots that exceed the berth limit on the new dropship. " +
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

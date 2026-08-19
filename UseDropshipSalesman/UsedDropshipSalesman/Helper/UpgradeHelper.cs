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
using UsedDropshipSalesman.Defs;
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

        public static void UpdateHangarConfig(DropshipConfig config, SimGameState sgs)
        {
            Mod.Log.Info?.Log($"Updating CU hangarConfig to support hangars: ");
            Dictionary<string, CustomHangarConstraint> constraints = new();
            foreach (DropshipHangarBay bay in config.CustomDropship.HangarBays)
            {
                Mod.Log.Info?.Log($" -- bay: {bay.bayId}  base: {bay.baseBays}  max: {bay.maxBays}");

                string statName = ModConsts.STAT_ADDITIONAL_HANGARS_PREFIX + bay.bayId;
                int additionalBays = sgs.companyStats.GetValue<int>(statName);
                int currentMax = bay.baseBays + additionalBays;
                if (currentMax > bay.maxBays) { currentMax = bay.maxBays; }
                Mod.Log.Debug?.Log($"CurrentMax for Hangars: {currentMax} => base: {bay.baseBays} + additional: {additionalBays}");

                constraints.Add(bay.bayId, new CustomHangarConstraint() { MaxAvailableUnits = currentMax });
            }

            CustomHangarHelper.SetConstraints(constraints, Mod.ModLabel);
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

            Mod.Log.Debug?.Log($"== Reverting all upgrades for current dropship: {configToRevert.CustomDropship.Description.Id}");

            // TODO: Need to iterate purchasedmodules (and shipModules for non-purchased?) instead of all - screwing with the logic
            Mod.Log.Debug?.Log($"Ship has {sim.PurchasedArgoUpgrades.Count} purchased upgrades: [{String.Join(",", sim.PurchasedArgoUpgrades)}]");
            foreach (String purchasedShipModuleId in sim.PurchasedArgoUpgrades)
            {
                // Check for persistent configuration items and move them into save-state. ApplyUpgrade should add them
                if (Mod.Config.PersistentUpgrades.Contains(purchasedShipModuleId))
                {
                    Mod.Log.Debug?.Log($"Persistent shipModule {purchasedShipModuleId} was purchased, marking it as purchased in SaveData");
                    if (!Mod.ModSaveData.PurchasedPersistentUpgrades.Contains(purchasedShipModuleId))
                    {
                        Mod.ModSaveData.PurchasedPersistentUpgrades.Add(purchasedShipModuleId);
                    }
                }
            }

            String shipUpgrades = String.Join(", ", sim.ShipUpgrades.Select(su => su.Description.Id).ToList());
            Mod.Log.Debug?.Log($"Ship has {sim.ShipUpgrades.Count} upgrades to revert: [{shipUpgrades}]");
            List<ShipModuleUpgrade> baseArgoUpgrades = new();
            foreach (ShipModuleUpgrade smu in sim.ShipUpgrades)
            {
                if (smu == null || smu.Description == null || smu.Description.Id == null || String.IsNullOrEmpty(smu?.Description?.Id))
                {
                    Mod.Log.Warning?.Log($"Ship module is null somehow? Skipping.");
                    continue;
                }

                // DO NOT REVERT the default argo upgrades, as these are tied to core Statistic values (like MechTechSkill, MedBayPods, etc). 
                //  If we nuke them, they aren't save-safe.
                if (ModConsts.BASEGAME_DEFAULT_ARGO_UPGRADES.Contains(smu.Description.Id))
                {
                    Mod.Log.Trace?.Log($"Default argo upgrade: {smu.Description.Id} found, skipping revert.");
                    baseArgoUpgrades.Add(smu);
                    continue;
                }

                Mod.Log.Debug?.Log($"Reverting ShipModuleUpgrade: {smu.Description.Id}");
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
            
            // TODO: Pull upgrades from save state instead of current mod config
            sim.purchasedArgoUpgrades.Clear();
            sim.purchasedArgoUpgrades.AddRange(baseArgoUpgrades.Select(smu => smu.Description.Id).ToList());
            sim.shipUpgrades.Clear();
            sim.shipUpgrades.AddRange(baseArgoUpgrades);

            // There should be NO upgrades at this point
            if (sim.ShipUpgrades.Count > 0)
            {
                String upgrades = String.Join(", ", sim.ShipUpgrades.Select(su => su.Description.Id).ToList());
                Mod.Log.Warning?.Log($"Ship still has {sim.ShipUpgrades.Count} upgrades after being reverted. This should not happen!\n" +
                    $"  Upgrades list: {upgrades}");
            }
            if (sim.PurchasedArgoUpgrades.Count >0)
            {
                Mod.Log.Warning?.Log($"Ship still has {sim.PurchasedArgoUpgrades.Count} purchased upgrades after being reverted. This should not happen!\n" +
                    $"  Upgrades list: [{String.Join(",", sim.PurchasedArgoUpgrades)}]");
            }

            Mod.Log.Debug?.Log($"== DONE");
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
            Dictionary<string, int> currentUnitCountPerHangar = new Dictionary<string, int>();
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

                if (currentUnitCountPerHangar.ContainsKey(hangarId)) { currentUnitCountPerHangar[hangarId] += 1; }
                else { currentUnitCountPerHangar.Add(hangarId, 1); }
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

                if (currentUnitCountPerHangar.ContainsKey(hangarId)) { currentUnitCountPerHangar[hangarId] += 1; }
                else { currentUnitCountPerHangar.Add(hangarId, 1); }
            }

            List<string> allHangars = allHangars = CustomHangarHelper.listHangars.Select(chd => chd.Description.Id).ToList();
            allHangars.Insert(0, CustomHangarHelper.BASE_HANGAR_ID);
            Dictionary<string, DropshipHangarBay> newHangarConfigs = newConfig.CustomDropship.HangarBays.ToList()
                .Distinct().ToDictionary(bayCfg => bayCfg.bayId);
            foreach (string hangarId in allHangars)
            {
                Mod.Log.Debug?.Log($"Evaluating count on hangarDef: {hangarId}");
                if (currentUnitCountPerHangar.ContainsKey(hangarId) && newHangarConfigs.ContainsKey(hangarId))
                {
                    DropshipHangarBay dropshipHangarBay = newHangarConfigs[hangarId];
                    string additionalHangarsStatName = $"{ModConsts.STAT_ADDITIONAL_HANGARS_PREFIX}{hangarId}";
                    int additionalHangars = sgs.companyStats.GetValue<int>(additionalHangarsStatName);
                    int newConstraintSize = dropshipHangarBay.baseBays + additionalHangars;
                    if (newConstraintSize > dropshipHangarBay.maxBays) { newConstraintSize = dropshipHangarBay.maxBays; }
                    Mod.Log.Debug?.Log($"New constraint size for hangar: {hangarId} is {newConstraintSize} => " +
                        $"base: {newHangarConfigs[hangarId].baseBays} + additional: {additionalHangars}");

                    if (currentUnitCountPerHangar[hangarId] > newConstraintSize) 
                    {
                        Mod.Log.Info?.Log($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {currentUnitCountPerHangar[hangarId]} units active or readying.");
                        // TODO: LOCALIZE 
                        blockedReasons.Add($"New dropship hangar {CustomHangarHelper.GetHangarLabel(hangarId)} limit is {newConstraintSize}, there are {currentUnitCountPerHangar[hangarId]} units active or readying.");
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
            Mod.Log.Debug?.Log($"New dropship has {sgs.GetMaxMechWarriors()} berths for {currentPilotCount} pilots");
            if (currentPilotCount > sgs.GetMaxMechWarriors())
            {
                blockedReasons.Add($"New dropship berth limit is {sgs.GetMaxMechWarriors()}, but there are {currentPilotCount} active pilots.");
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

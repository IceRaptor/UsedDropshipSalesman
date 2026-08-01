using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using CustomUnits.CustomHangars;
using HBS.Collections;
using HBS.Extensions;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Defs;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStats")]
    static class SimGameState_InitCompanyStats
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_InitCompanyStats - entered.");
            __instance.companyStats.AddStatistic<String>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);

            // Force there to be 3 full mechbays for all ships, and let CU constraints handle the rest
            __instance.companyStats.Set<int>(__instance.Constants.Story.MechBayPodsID, 3);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    static class SimGameState_Rehydrate
    {
        static void Postfix(GameInstanceSave gameInstanceSave, SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_Rehydrate - entered.");
            if (!__instance.CompanyStats.ContainsStatistic(ModConsts.STAT_CURRENT_DROPSHIP))
            {
                Mod.Log.Debug?.Log($"Game without UDS stats loaded, initializing to default dropship: {Mod.Config.FallbackDropship}");
                __instance.CompanyStats.AddStatistic<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);
                __instance.CompanyStats.Set<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);

                // Save the dropship state
                Mod.ModSaveData.CurrentDropshipId = Mod.Config.FallbackDropship;

                Mod.Log.Debug?.Log($"Current dropship value is: {__instance.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP)}");
            }
            else
            {
                Mod.Log.Debug?.Log($"Current dropship is: {__instance.CompanyStats.GetValue<String>(ModConsts.STAT_CURRENT_DROPSHIP)}");
                Mod.Log.Debug?.Log($"SaveState is: {Mod.ModSaveData}");
            }

            // Force there to be 3 full mechbays for all ships, and let CU constraints handle the rest
            __instance.companyStats.Set<int>(__instance.Constants.Story.MechBayPodsID, 3);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(int timeLapse, SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_OnDayPassed - entered.");

            if (__instance == null) return;


            // Check for a difference in ships
            string currDropshipId = __instance.CompanyStats.GetValue<String>(ModConsts.STAT_CURRENT_DROPSHIP);
            Mod.Log.Debug?.Log($"Current dropship stat is: {currDropshipId}, SaveState is: {Mod.ModSaveData}");
            if (!String.Equals(currDropshipId, Mod.ModSaveData.CurrentDropshipId, StringComparison.InvariantCultureIgnoreCase))
            {
                Mod.Log.Info?.Log($"Current dropship stat: {currDropshipId} does not match SaveState: {Mod.ModSaveData.CurrentDropshipId}, changing dropship.");

                // TODO: Add dialog when traveling 
                if (__instance.TravelState != SimGameTravelStatus.IN_SYSTEM) { return; } // Only process dropship upgrades at a planet

                Mod.Config.Dropships.TryGetValue(Mod.ModSaveData.CurrentDropshipId, out DropshipConfig oldConfig);
                Mod.Config.Dropships.TryGetValue(currDropshipId, out DropshipConfig newConfig);
                bool upgradeIsBlocked = UpgradeHelper.IsUpgradeBlocked(newConfig, oldConfig, __instance);
                if (upgradeIsBlocked) {
                    __instance.RoomManager.ShipRoom.TimePlayPause.ToggleTime();
                    return; 
                } // Dialog will fire and try again tomorrow

                Mod.Log.Info?.Log($"Reverting dropship: {oldConfig.CustomDropship.Description.Id}");
                // THIS IS WHAT BREAKS MECHBAY
                UpgradeHelper.RevertUpgrades(oldConfig, SimGameState_Debug.sim);

                Mod.Log.Info?.Log($"Applying upgrades for new dropship: {newConfig.CustomDropship.Description.Id}");
                UpgradeHelper.ApplyUpgrades(newConfig, SimGameState_Debug.sim);
                EngineeringScreenUIHelper.RefreshUpgradeIcons(__instance.RoomManager.EngineeringRoom.engineeringScreen, newConfig);

                UpgradeHelper.UpdateDropConfig(newConfig);
                UpgradeHelper.UpdateHangarConfig(newConfig);
                UIHelper.UpdateHangerConfig(newConfig, __instance);

                Mod.ModSaveData.CurrentDropshipId = currDropshipId;

                // This should update the visuals
                __instance.SpaceController.SetShip(DropshipType.Argo);

                // Refresh rooms - WHY AM I DOING THIS?
                //__instance.RoomManager.RefreshDisplay();

                // Pause after the upgrade
                __instance.RoomManager.ShipRoom.TimePlayPause.ToggleTime();
            }
            else
            {
                Mod.Log.Trace?.Log($"Current dropship stat: {currDropshipId} matches saved dropship: {Mod.ModSaveData.CurrentDropshipId}, skipping.");
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "SetSimShip")]
    static class SimGameState_SetSimShip
    {
        static void Postfix(DropshipType dropship, SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_SetSimShip - entered.");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "CompleteArgoUpgrade")]
    static class SimGameState_CompleteArgoUpgrade
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_CompleteArgoUpgrade - entered.");
            if (__instance == null || __instance?.RoomManager?.EngineeringRoom?.engineeringScreen == null) return; // Nothing to do

            // Refresh argo upgrade colors
            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }
            EngineeringScreenUIHelper.RefreshUpgradeIcons(__instance.RoomManager.EngineeringRoom.engineeringScreen, config);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "AddArgoUpgrade")]
    static class SimGameState_AddArgoUpgrade
    {
        static void Prefix(bool __runOriginal, ShipModuleUpgrade upgrade, SimGameState __instance, out DropshipType __state)
        {
            Mod.Log.Trace?.Log("==== SimGameState_AddArgoUpgrade-PREFIX- entered");
            // Skip processing if spacecontroller isn't valid; prevents event generated argoUpgrade error
            if (__instance == null || __instance.SpaceController == null) {
                __state = DropshipType.Argo;
                return; 
            }

            // Force the type so all upgrade logic applies
            __state = __instance.CurDropship;
            __instance.CurDropship = DropshipType.Argo;
            Mod.Log.Trace?.Log($"state: {__state} sgs.currentDropship: {__instance.CurDropship}");

        }

        static void Postfix(ShipModuleUpgrade upgrade, SimGameState __instance, DropshipType __state)
        {
            Mod.Log.Trace?.Log("==== SimGameState_AddArgoUpgrade-POSTFIX - entered");
            // Skip processing if spacecontroller isn't valid; prevents event generated argoUpgrade error
            if (__instance == null || __instance.SpaceController == null) { return; }

            __instance.CurDropship = __state;
            Mod.Log.Trace?.Log($"state: {__state} sgs.currentDropship: {__instance.CurDropship}");


        }
    }

    [HarmonyPatch(typeof(SimGameState), "QueueArgoUpgrade")]
    static class SimGameState_QueueArgoUpgrade
    {
        static void Postfix(ShipModuleUpgrade requestedUpgrade, SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_QueueArgoUpgrade - entered");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "UpdateArgoUpgrades")]
    static class SimGameState_UpdateArgoUpgrades
    {
        static void Postfix(bool passDay, SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_UpdateArgoUpgrades - entered");
        }
    }

    // Invoked on F7
    [HarmonyPatch(typeof(SimGameState_Debug), "SimDebug_ToggleCurrentShipType")]
    static class SimGameState_Debug_SimDebug_ToggleCurrentShipType
    {
        static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Log("==== SimGameState_Debug_SimDebug_ToggleCurrentShipType - entered");

            if (!__runOriginal) return;

            if (!SimGameState_Debug.isSimAvailable) return;

            __runOriginal = false;

            var gpb = GenericPopupBuilder.Create("Choose Dropship", "Choose a dropship from the buttons below.");
            foreach (KeyValuePair<string, DropshipConfig> kvp in Mod.Config.Dropships)
            {
                gpb.AddButton($"{kvp.Value.CustomDropship.Description.Name}",
                    delegate () { SetCurrentDropship(kvp.Value.CustomDropship.Description.Id); }
                    );
            }
            gpb.Render();
        }
        private static void SetCurrentDropship(string dropshipId)
        {
            if (!SimGameState_Debug.isSimAvailable) return;

            SimGameState_Debug.sim.CompanyStats.Set(ModConsts.STAT_CURRENT_DROPSHIP, dropshipId);
        }
    }

    // Invoked at the end of character creation
    [HarmonyPatch(typeof(SimGameState), "InitStartingPlanet_TEMP")]
    static class SimGameState_InitStartingPlanet_TEMP
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_InitStartingPlanet_TEMP - entered");

            // CurrSystem should be set to the start position of the selected life path
            Mod.Log.Debug?.Log($"Starting system is: {__instance.CurSystem?.Name}");

            var found = Mod.Config.CareerStartDropshipByPlanetName.TryGetValue(__instance.CurSystem.Name, out string startingDropshipId);
            if (!found)
            {
                startingDropshipId = Mod.Config.FallbackDropship;
            }

            __instance.CompanyStats.Set<String>(ModConsts.STAT_CURRENT_DROPSHIP, startingDropshipId);
            Mod.ModSaveData.Reset();
            Mod.ModSaveData.CurrentDropshipId = startingDropshipId;

            // Setup values at career start
            Mod.Config.Dropships.TryGetValue(startingDropshipId, out DropshipConfig newConfig);
            UpgradeHelper.ApplyUpgrades(newConfig, SimGameState_Debug.sim);
            UpgradeHelper.UpdateDropConfig(newConfig);
            UpgradeHelper.UpdateHangarConfig(newConfig);

            Mod.Log.Debug?.Log($"Current dropship is: {__instance.CompanyStats.GetValue<String>(ModConsts.STAT_CURRENT_DROPSHIP)}");
            Mod.Log.Debug?.Log($"SaveState is: {Mod.ModSaveData}");
        }
    }

    // Invoked at the end of character creation
    [HarmonyPatch(typeof(SimGameState), "InitFromSave")]
    static class SimGameState_InitFromSave
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_InitFromSave - entered");

            // Update CAC settings on save load
            Mod.Config.Dropships.TryGetValue(Mod.ModSaveData.CurrentDropshipId, out DropshipConfig newConfig);
            UpgradeHelper.UpdateDropConfig(newConfig);
            UpgradeHelper.UpdateHangarConfig(newConfig);
        }
    }

    // Limits the count of mechwarriors available
    [HarmonyPatch(typeof(SimGameState), "GetMaxMechWarriors")]
    static class SimGameState_GetMaxMechWarriors
    {
        static void Postfix(SimGameState __instance, ref int __result)
        {
            Mod.Log.Trace?.Log("==== SimGameState_GetMaxMechWarriors - entered");

            if (__instance == null) { return; }
            if (Mod.ModSaveData == null) { return; } // This can be invoked before the save is hydrated, so short-circuit

            Mod.Log.Trace?.Log($"Resolving MaxMechwarriors for dropshipId: {Mod.ModSaveData?.CurrentDropshipId}");
            var hasValue = Mod.Config.Dropships.TryGetValue(Mod.ModSaveData.CurrentDropshipId, out DropshipConfig dropshipConfig);
            if (!hasValue) 
            {
                Mod.Log.Warning?.Log($"Failed to lookup dropship config for: {Mod.ModSaveData.CurrentDropshipId}, this should not happen! TELL FROST");
                return;
            }

            Mod.Log.Debug?.Log($"Returning {dropshipConfig.CustomDropship.Berths.MaxPilots} for max pilots.");
            __result = dropshipConfig.CustomDropship.Berths.MaxPilots;
        }
    }

    // Total maintenance cost for the ship
    [HarmonyPatch(typeof(SimGameState), "GetShipBaseMaintenanceCost")]
    static class SimGameState_GetShipBaseMaintenanceCost
    {
        static void Postfix(SimGameState __instance, ref int __result)
        {
            Mod.Log.Trace?.Log("==== SimGameState_GetShipBaseMaintenanceCost - entered");

            if (__instance == null) { return; }
            if (Mod.ModSaveData == null) { return; } // This can be invoked before the save is hydrated, so short-circuit

            var hasValue = Mod.Config.Dropships.TryGetValue(Mod.ModSaveData.CurrentDropshipId, out DropshipConfig dropshipConfig);
            if (!hasValue)
            {
                Mod.Log.Warning?.Log($"Failed to lookup dropship config for: {Mod.ModSaveData.CurrentDropshipId}, this should not happen! TELL FROST");
                return;
            }

            Mod.Log.Debug?.Log($"Returning {dropshipConfig.CustomDropship.Costs.Upkeep} for monthly dropship cost.");
            __result = dropshipConfig.CustomDropship.Costs.Upkeep;
        }
    }

    //// Total maintenance cost for the ship
    //[HarmonyPatch(typeof(SimGameState), "HasShipUpgrade")]
    //[HarmonyPatch(new Type[] { typeof(TagSet), typeof(List<string>)})]
    //static class SimGameState_HasShipUpgrade_TagSet
    //{
    //    static void Postfix(SimGameState __instance, bool __result, TagSet idList, List<string> upgradesToCheck = null)
    //    {
    //        if (__instance == null) { return; }
    //        if (Mod.ModSaveData == null) { return; } // This can be invoked before the save is hydrated, so short-circuit

    //        Mod.Log.Trace?.Log("==== SimGameState_HasShipUpgrade(TagSet, List)- entered");
    //        Mod.Log.Debug?.Log($"SimGameState_HasShipUpgrade(TagSet, List): {__result} for idList: [{idList}] and upgradesToCheck: [{String.Join(",", upgradesToCheck)}]");
    //    }
    //}

}

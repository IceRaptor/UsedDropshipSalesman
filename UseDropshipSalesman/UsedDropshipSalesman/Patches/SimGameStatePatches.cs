using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using CustomUnits;
using CustomUnits.CustomHangars;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using us.frostraptor.modUtils;
using UsedDropshipSalesman.Defs;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStats")]
    static class SimGameState_InitCompanyStats
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_InitCompanyStats - entered.");
            __instance.companyStats.AddStatistic<String>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);
            Mod.ModSaveState = new Data.UDSSaveData();
        }
    }

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    static class SimGameState_Rehydrate
    {
        static void Postfix(GameInstanceSave gameInstanceSave, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_Rehydrate - entered.");
            if (!__instance.CompanyStats.ContainsStatistic(ModConsts.STAT_CURRENT_DROPSHIP))
            {
                Mod.Log.Debug?.Write($"Game without UDS stats loaded, initializing to default dropship: {Mod.Config.FallbackDropship}");
                __instance.CompanyStats.AddStatistic<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);
                __instance.CompanyStats.Set<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.FallbackDropship);

                // Save the dropship state
                Mod.ModSaveState.CurrentDropshipId = Mod.Config.FallbackDropship;

                Mod.Log.Debug?.Write($"Current dropship value is: {__instance.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP)}");
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(int timeLapse, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_OnDayPassed - entered.");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "SetSimShip")]
    static class SimGameState_SetSimShip
    {
        static void Postfix(DropshipType dropship, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_SetSimShip - entered.");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ApplyArgoUpgrades")]
    static class SimGameState_ApplyArgoUpgrades
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_ApplyArgoUpgrades - entered.");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "AddArgoUpgrade")]
    static class SimGameState_AddArgoUpgrade
    {
        static void Prefix(bool __runOriginal, ShipModuleUpgrade upgrade, SimGameState __instance, out DropshipType __state)
        {
            Mod.Log.Trace?.Write("==== SimGameState_AddArgoUpgrade-PREFIX- entered");
            // Skip processing if spacecontroller isn't valid; prevents event generated argoUpgrade error
            if (__instance == null || __instance.SpaceController == null) {
                __state = DropshipType.Argo;
                return; 
            }

            // Force the type so all upgrade logic applies
            __state = __instance.CurDropship;
            __instance.CurDropship = DropshipType.Argo;
            Mod.Log.Trace?.Write($"state: {__state} sgs.currentDropship: {__instance.CurDropship}");

        }

        static void Postfix(ShipModuleUpgrade upgrade, SimGameState __instance, DropshipType __state)
        {
            Mod.Log.Trace?.Write("==== SimGameState_AddArgoUpgrade-POSTFIX - entered");
            // Skip processing if spacecontroller isn't valid; prevents event generated argoUpgrade error
            if (__instance == null || __instance.SpaceController == null) { return; }

            __instance.CurDropship = __state;
            Mod.Log.Trace?.Write($"state: {__state} sgs.currentDropship: {__instance.CurDropship}");


        }
    }

    [HarmonyPatch(typeof(SimGameState), "QueueArgoUpgrade")]
    static class SimGameState_QueueArgoUpgrade
    {
        static void Postfix(ShipModuleUpgrade requestedUpgrade, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_QueueArgoUpgrade - entered");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "UpdateArgoUpgrades")]
    static class SimGameState_UpdateArgoUpgrades
    {
        static void Postfix(bool passDay, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_UpdateArgoUpgrades - entered");
        }
    }



    [HarmonyPatch(typeof(SimGameState_Debug), "SimDebug_ToggleCurrentShipType")]
    static class SimGameState_Debug_SimDebug_ToggleCurrentShipType
    {
        static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write("==== SimGameState_Debug_SimDebug_ToggleCurrentShipType - entered");

            var currentDropshipId = SimGameState_Debug.sim.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            Mod.Log.Info?.Write($"Current dropship is: '{currentDropshipId}'.");

            int nextDropshipIdx = -1;
            var dropshipIds = Mod.Config.Dropships.Keys.ToArray();
            for (int i = 0; i < dropshipIds.Length; i++)
            {
                string dropshipId = dropshipIds[i];
                Mod.Log.Trace?.Write($"Evaluating dropshipId: {dropshipId} with idx: {i}");
                if (currentDropshipId.Equals(dropshipId, StringComparison.InvariantCultureIgnoreCase))
                {
                    nextDropshipIdx = i+1;
                }
            }
            if (nextDropshipIdx == dropshipIds.Length) { nextDropshipIdx = 0; }
            string nextDropshipId = dropshipIds[nextDropshipIdx];

            Mod.Log.Info?.Write($"Next dropship is: '{nextDropshipId}' with idx: {nextDropshipIdx}.");
            SimGameState_Debug.sim.SpaceController.SetShip(DropshipType.Leopard);

            // Simulate an upgrade flow
            SimGameState_Debug.sim.CompanyStats.Set<string>(ModConsts.STAT_CURRENT_DROPSHIP, nextDropshipId); // mod sets sim-state different, has been changed
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig oldConfig);
            Mod.Config.Dropships.TryGetValue(nextDropshipId, out DropshipConfig newConfig);
            UpgradeHelper.RevertUpgrades(oldConfig, SimGameState_Debug.sim);
            UpgradeHelper.ApplyUpgrades(newConfig, SimGameState_Debug.sim);

            __runOriginal = false;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "InitStartingPlanet_TEMP")]
    static class SimGameState_InitStartingPlanet_TEMP
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_InitStartingPlanet_TEMP - entered");

            // CurrSystem should be set to the start position of the selected life path
            Mod.Log.Debug?.Write($"Starting system is: {__instance.CurSystem?.Name}");

            var found = Mod.Config.CareerStartDropshipByPlanetName.TryGetValue(__instance.CurSystem.Name, out string startingDropshipId);
            if (!found)
            {
                startingDropshipId = Mod.Config.FallbackDropship;
            }

            __instance.CompanyStats.Set<String>(ModConsts.STAT_CURRENT_DROPSHIP, startingDropshipId);
            Mod.ModSaveState = new Data.UDSSaveData()
            {
                CurrentDropshipId = startingDropshipId,
            };

        }
    }


    //[HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    //static class SimGameState__OnAttachUXComplete
    //{
    //    static void Postfix(SimGameState __instance)
    //    {
    //        Mod.Log.Trace?.Write("==== SimGameState__OnAttachUXComplete - entered");



    //    }
    //}
}

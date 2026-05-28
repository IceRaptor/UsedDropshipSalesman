using BattleTech.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using us.frostraptor.modUtils;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStats")]
    static class SimGameState_InitCompanyStats
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_InitCompanyStats - entered.");
            __instance.companyStats.AddStatistic<String>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.DefaultDropship);

        }
    }

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    static class SimGameState_Rehydrate
    {
        static void Postfix(GameInstanceSave gameInstanceSave)
        {
            Mod.Log.Trace?.Write("==== SimGameState_Rehydrate - entered.");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "InitFromSave")]
    static class SimGameState_InitFromSave
    {
        static void Postfix(GameInstance game, GameInstanceSave gameInstanceSave, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_InitFromSave - entered.");
            if (! __instance.CompanyStats.ContainsStatistic(ModConsts.STAT_CURRENT_DROPSHIP))
            {
                Mod.Log.Debug?.Write($"Game without UDS stats loaded, initializing to default: {Mod.Config.DefaultDropship}");
                __instance.CompanyStats.AddStatistic<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.DefaultDropship);
                __instance.CompanyStats.Set<string>(ModConsts.STAT_CURRENT_DROPSHIP, Mod.Config.DefaultDropship);

                Mod.Log.Debug?.Write($"Current dropship value is: {__instance.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP)}");
            }

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
        static void Postfix(ShipModuleUpgrade upgrade, SimGameState __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_AddArgoUpgrade - entered");
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
        static void Prefix(bool __runOriginal)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write("==== SimGameState_Debug_SimDebug_ToggleCurrentShipType - entered");
        }
    }

}

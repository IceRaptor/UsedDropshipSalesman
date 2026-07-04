using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using CustomUnits.CustomHangars;
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
                UpgradeHelper.RevertUpgrades(oldConfig, SimGameState_Debug.sim);
                
                Mod.Log.Info?.Log($"Applying upgrades for new dropship: {newConfig.CustomDropship.Description.Id}");
                UpgradeHelper.ApplyUpgrades(newConfig, SimGameState_Debug.sim);

                Mod.ModSaveData.CurrentDropshipId = currDropshipId;

                // This should force and update of the visuals
                __instance.SpaceController.SetShip(DropshipType.Argo);
               
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

    [HarmonyPatch(typeof(SimGameState), "ApplyArgoUpgrades")]
    static class SimGameState_ApplyArgoUpgrades
    {
        static void Postfix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_ApplyArgoUpgrades - entered.");
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



    [HarmonyPatch(typeof(SimGameState_Debug), "SimDebug_ToggleCurrentShipType")]
    static class SimGameState_Debug_SimDebug_ToggleCurrentShipType
    {
        static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Log("==== SimGameState_Debug_SimDebug_ToggleCurrentShipType - entered");

            var currentDropshipId = SimGameState_Debug.sim.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            Mod.Log.Info?.Log($"Current dropship is: '{currentDropshipId}'.");

            int nextDropshipIdx = -1;
            var dropshipIds = Mod.Config.Dropships.Keys.ToArray();
            for (int i = 0; i < dropshipIds.Length; i++)
            {
                string dropshipId = dropshipIds[i];
                Mod.Log.Trace?.Log($"Evaluating dropshipId: {dropshipId} with idx: {i}");
                if (currentDropshipId.Equals(dropshipId, StringComparison.InvariantCultureIgnoreCase))
                {
                    nextDropshipIdx = i+1;
                }
            }
            if (nextDropshipIdx == dropshipIds.Length) { nextDropshipIdx = 0; }
            string nextDropshipId = dropshipIds[nextDropshipIdx];

            Mod.Log.Info?.Log($"Next dropship is: '{nextDropshipId}' with idx: {nextDropshipIdx}.");
            SimGameState_Debug.sim.SpaceController.SetShip(DropshipType.Leopard);

            // Simulate an upgrade flow
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig oldConfig);
            Mod.Config.Dropships.TryGetValue(nextDropshipId, out DropshipConfig newConfig);
            UpgradeHelper.RevertUpgrades(oldConfig, SimGameState_Debug.sim);
            UpgradeHelper.ApplyUpgrades(newConfig, SimGameState_Debug.sim);

            SimGameState_Debug.sim.CompanyStats.Set<string>(ModConsts.STAT_CURRENT_DROPSHIP, nextDropshipId); // mod sets sim-state different, has been changed
            Mod.ModSaveData.CurrentDropshipId = newConfig.CustomDropship.Description.Id;
            __runOriginal = false;
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

            Mod.Log.Debug?.Log($"Current dropship is: {__instance.CompanyStats.GetValue<String>(ModConsts.STAT_CURRENT_DROPSHIP)}");
            Mod.Log.Debug?.Log($"SaveState is: {Mod.ModSaveData}");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    static class SimGameState__OnAttachUXComplete
    {
        static void Postfix(SimGameState __instance)
        {

            //if (ModState.SneakyLadGO == null)
            //{
            //    ModState.SneakyLadGO = new("UDS_SNEAKY_LAD");
            //    UnityEngine.Object.DontDestroyOnLoad(ModState.SneakyLadGO);
            //}

            //Mod.Log.Trace?.Log("==== SimGameState__OnAttachUXComplete - entered");

            //var parentGO = ModState.SneakyLadGO.transform?.parent?.gameObject;
            //Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: parent GO: {parentGO?.name} is null ? {parentGO == null}");
            //foreach (Transform t in parentGO.transform)
            //{
            //    Mod.Log.Debug?.Log($" -- childGO: {t?.gameObject?.name}");
            //}

            //var parentGO = __instance?.SpaceController?.gameObject;
            //var grandparentGO = parentGO?.transform?.parent?.gameObject;
            //var rootGO = parentGO?.transform?.root?.gameObject;
            //Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: parent GO: {parentGO?.name} is null ? {parentGO == null}");
            //Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: grandparent GO: {grandparentGO?.name} is null ? {grandparentGO == null}");
            //Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: parent root GO: {rootGO?.name} is null ? {rootGO == null}");
            //if (parentGO != null)
            //{
            //    var spaceLoadingGO = parentGO.FindFirstChildNamed("SpaceLoading");
            //    Mod.Log.Debug?.Log($"SpaceLoading == null ? {spaceLoadingGO == null}");

            //    var newGOChild = parentGO.FindFirstChildNamed("SpaceLoading");
            //    Mod.Log.Debug?.Log($"newGOChild == null ? {newGOChild == null}");
            //}

            var spaceLoadingGO = GameObject.Find("envPrfVhcl_leopard");
            var realSpaceGO = GameObject.Find("Real Space");
            Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: spaceLoadingGO: '{spaceLoadingGO?.name}' is null ? {spaceLoadingGO == null}");
            Mod.Log.Debug?.Log($"SimGameState__OnAttachUXComplete: realSpaceGO: '{realSpaceGO?.name}' is null ? {realSpaceGO == null}");


        }
    }

    [HarmonyPatch(typeof(SimGameState), "StartContract")]
    static class SimGameState_StartContract
    {
        static void Prefix(SimGameState __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_StartContract - entered");
            //var parentGO = __instance?.SpaceController?.gameObject;
            //var grandparentGO = parentGO?.transform?.parent?.gameObject;
            //var rootGO = parentGO?.transform?.root?.gameObject;
            //Mod.Log.Debug?.Log($"SimGameState_StartContract: parent GO: {parentGO?.name} is null ? {parentGO == null}");
            //Mod.Log.Debug?.Log($"SimGameState_StartContract: grandparent GO: {grandparentGO?.name} is null ? {grandparentGO == null}");
            //Mod.Log.Debug?.Log($"SimGameState_StartContract: parent root GO: {rootGO?.name} is null ? {rootGO == null}");
            //if (parentGO != null)
            //{
            //    var spaceLoadingGO = parentGO.FindFirstChildNamed("SpaceLoading");
            //    Mod.Log.Debug?.Log($"SpaceLoading == null ? {spaceLoadingGO == null}");
            //}

            var spaceLoadingGO = GameObject.Find("envPrfVhcl_leopard");
            var realSpaceGO = GameObject.Find("Real Space");
            Mod.Log.Debug?.Log($"SimGameState_StartContract: spaceLoadingGO: '{spaceLoadingGO?.name}' is null ? {spaceLoadingGO == null}");
            Mod.Log.Debug?.Log($"SimGameState_StartContract: realSpaceGO: '{realSpaceGO?.name}' is null ? {realSpaceGO == null}");
            if (spaceLoadingGO != null) { spaceLoadingGO.SetActive(false); }
        }
    }

    [HarmonyPatch(typeof(GameInstance), "LaunchContract", new Type[] { typeof(Contract)})]
    static class GameInstance_LaunchContract
    {
        static void Prefix(GameInstance __instance)
        {
            Mod.Log.Trace?.Log("==== GameInstance_LaunchContract - entered");
            //var parentGO = __instance?.Simulation?.SpaceController?.gameObject;
            //var grandparentGO = parentGO?.transform?.parent?.gameObject;
            //var rootGO = parentGO?.transform?.root?.gameObject;
            //Mod.Log.Debug?.Log($"GameInstance_LaunchContract: parent GO: {parentGO?.name} is null ? {parentGO == null}");
            //Mod.Log.Debug?.Log($"GameInstance_LaunchContract: grandparent GO: {grandparentGO?.name} is null ? {grandparentGO == null}");
            //Mod.Log.Debug?.Log($"GameInstance_LaunchContract: parent root GO: {rootGO?.name} is null ? {rootGO == null}");

            //if (parentGO != null)
            //{
            //    var spaceLoadingGO = parentGO.FindFirstChildNamed("SpaceLoading");
            //    Mod.Log.Debug?.Log($"SpaceLoading == null ? {spaceLoadingGO == null}");
            //}

            var spaceLoadingGO = GameObject.Find("envPrfVhcl_leopard");
            var realSpaceGO = GameObject.Find("Real Space");
            Mod.Log.Debug?.Log($"GameInstance_LaunchContract: spaceLoadingGO: '{spaceLoadingGO?.name}' is null ? {spaceLoadingGO == null}");
            Mod.Log.Debug?.Log($"GameInstance_LaunchContract: realSpaceGO: '{realSpaceGO?.name}' is null ? {realSpaceGO == null}");
            if (spaceLoadingGO != null) { spaceLoadingGO.SetActive(false); }

        }
    }

    [HarmonyPatch(typeof(LevelLoader), "LoadScene", new Type[] { typeof(string), typeof(string) })]
    static class LevelLoader_LoadScene
    {
        static void Prefix(LevelLoader __instance, string scene, string loadingInterstitialScene)
        {
            Mod.Log.Trace?.Log("==== LevelLoader_LoadScene - entered");
            Mod.Log.Debug?.Log($"==== LevelLoader_LoadScene - scene: {scene}  loadingInterstitialScene: {loadingInterstitialScene}");


        }
    }



    // UnityEngine.Object.Instantiate
    //[HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object)} )]
    //static class UnityEngine_Object_Instantiate
    //{
    //    static void Prefix(UnityEngine.Object original)
    //    {
    //        Mod.Log.Trace?.Log("==== UnityEngine_Object_Instantiate:Object - entered");
    //        if (original as GameObject != null)
    //        {
    //            Mod.Log.Debug?.Log($"==== UnityEngine_Object_Instantiate:Object - {original?.name} ");
    //        }

    //    }
    //}
}

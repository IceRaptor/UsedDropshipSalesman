using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Save.SaveGameStructure;
using CustomAmmoCategoriesPatches;
using ErosionBrushPlugin;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using HBS.Animation;
using HBS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(SimGameSpaceController), "Init")]
    static class SimGameSpaceController_Init
    {
        static void Postfix(SimGameState simGame, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_Init - entered");

            // Initilate the leopard references
            DropshipHelper.BuildSimLeopardState(__instance);

            // Make sure we have a datamanager reference
            if (ModState.DataManagerUnityInstance == null)
            {
                GameObject dataManagerGO = GameObject.Find("DataManager");
                ModState.DataManagerUnityInstance = dataManagerGO?.GetComponent<DataManagerUnityInstance>();
                if (ModState.DataManagerUnityInstance == null) { Mod.Log.Warning?.Log("Failed to find DataManagerUnityInstance!"); }
            }
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "Dock")]
    static class SimGameSpaceController_Dock
    {
        static void Postfix(SimGameTravelStatus status, bool force, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_Dock - entered");

            Mod.Log.Trace?.Log($"Entry speed is: {__instance.argoAnimator.speed}");
            //__instance.argoAnimator.speed = 0.5f;
        }
    }


    [HarmonyPatch(typeof(SimGameSpaceController), "LateUpdate")]
    static class SimGameSpaceController_LateUpdate
    {
        // LateUpdate is very performance sensitive - avoid any logging
        static void Postfix(SimGameSpaceController __instance)
        {

            // This is a fix for a bug where loading a save while traveling always reverts back to the Argo. I think it's because
            //  the argo animator forces the argo to be active, and we have to side-step that.
            if (__instance.argo.gameObject.activeSelf && Mod.ModSaveData.prefabDonorIsLeopard)
            {
                // We have a mismatch; flip the prefab back to the leopard
                __instance.argo.gameObject.SetActive(false);
                __instance.leopard.gameObject.SetActive(true);
                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);
            }

            if (__instance.leopard.gameObject.activeSelf && !Mod.ModSaveData.prefabDonorIsLeopard)
            {
                // We have a mismatch; flip the prefab back to the argo
                __instance.argo.gameObject.SetActive(true);
                __instance.leopard.gameObject.SetActive(false);
                __instance.argoAnimator.SetTrigger("setargo");
                __instance.argoAnimator.SetBool("argo", value: true);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "SetShipTransitStates")]
    static class SimGameSpaceController_SetShipTransitStates
    {
        static void Prefix(ref bool inTransit, SimGameSpaceController __instance, ref bool __runOriginal)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_SetShipTransitStates:PRE - entered");

            Mod.Log.Trace?.Log($"  argoGO activeSelf? {__instance.argo.gameObject.activeSelf}  activeInHierarchy? {__instance.argo.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  leopardGO activeSelf? {__instance.leopard.gameObject.activeSelf}  activeInHierarchy? {__instance.leopard.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  sim travelState: {__instance.sim.TravelState}");

            __runOriginal = false;
        }

        static void Postfix(ref bool inTransit, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_SetShipTransitStates:POST - entered");

            Mod.Log.Trace?.Log($"  argoGO activeSelf? {__instance.argo.gameObject.activeSelf}  activeInHierarchy? {__instance.argo.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  leopardGO activeSelf? {__instance.leopard.gameObject.activeSelf}  activeInHierarchy? {__instance.leopard.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  sim travelState: {__instance.sim.TravelState}");
        }

    }

    [HarmonyPatch(typeof(SimGameSpaceController), "CurrentShipController", MethodType.Getter)]
    [HarmonyPatch()]
    static class SimGameSpaceController_CurrentShipController
    {
        static void Postfix(ref ArgoController __result, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_CurrentShipController - entered");

            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            if (config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_ARGO)
            {
                Mod.Log.Debug?.Log(" Returning argo ArgoController instance");
                __result = __instance.argo;
            }
            else
            {
                Mod.Log.Debug?.Log(" Returning leopard ArgoController instance");
                __result = __instance.leopard;
            }
            Mod.Log.Debug?.Log($" EngineController id: {__result.name}  parent.id: {__result?.gameObject?.transform?.parent?.gameObject?.name}  " +
                $"currentState: {__result.currentState}  currentEngineState: {__result.currentEngineState}  dropshipType: {__result.dropshipType}  " +
                $"currShip: {__instance.currentShip}  simCurrShip: {__instance.sim.CurDropship}");

            Mod.Log.Trace?.Log($"  argoGO activeSelf? {__instance.argo.gameObject.activeSelf}  activeInHierarchy? {__instance.argo.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  leopardGO activeSelf? {__instance.leopard.gameObject.activeSelf}  activeInHierarchy? {__instance.leopard.gameObject.activeInHierarchy}");
            Mod.Log.Trace?.Log($"  sim travelState: {__instance.sim.TravelState}");
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "SetShip")]
    static class SimGameSpaceController_SetShip
    {
        static void Postfix(DropshipType ship, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameSpaceController_SetShip - entered");
            Mod.Log.Debug?.Log($"SetShip invoked with: {ship}");

            ModState.SimGameSpaceController ??= __instance;

            // TODO: Need to handle a call to set the argo
            // TODO: Need to handle a default new career by disabling argo
            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Log.Info?.Log($"Current dropship is: '{currentDropshipId}', overlaying meshes.");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            // Always force the argo to make the upgrades visible
            __instance.currentShip = DropshipType.Argo;
            __instance.sim.CurDropship = DropshipType.Argo;
            __instance.sim.HasSimShipBeenSet = true;

            if (ModState.SimGameLeopardState == null || ModState.SimGameLeopardState.ParentGO == null)
            {
                Mod.Log.Debug?.Log($"SimLeopardState or parentGO was null, rebuidling.");
                DropshipHelper.BuildSimLeopardState(__instance);
            }

            __instance.argo.gameObject.SetActive(false);
            __instance.leopard.gameObject.SetActive(false);
            
           if (config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_LEOPARD)
           {
                Mod.Log.Debug?.Log($"Setting visuals to HBS_LEOPARD");
                // Use the default Leopard meshes with custom upgrades
                DropshipHelper.ToggleSimLeopardVisiblity(true);
                __instance.leopard.gameObject.SetActive(true);

                Mod.ModSaveData.prefabDonorIsLeopard = true;

               __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                EngineeringScreenUIHelper.OverlayCustomUpgrades(config.CustomDropship.Upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else if (config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_ARGO)
           {
                Mod.Log.Debug?.Log($"Setting visuals to HBS_ARGO");
                // Use the default Argo ship with custom upgrades
                DropshipHelper.ToggleSimLeopardVisiblity(false);
                __instance.argo.gameObject.SetActive(true);

                Mod.ModSaveData.prefabDonorIsLeopard = false;

                __instance.argoAnimator.SetTrigger("setargo");
                __instance.argoAnimator.SetBool("argo", value: true);

                EngineeringScreenUIHelper.ResetUpgradePanel(__instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else
            {
                Mod.Log.Debug?.Log($"Setting visuals to CUSTOM");
                // Use a custom mesh with custom upgrades
                DropshipHelper.ToggleSimLeopardVisiblity(false);
                __instance.leopard.gameObject.SetActive(true);

                Mod.ModSaveData.prefabDonorIsLeopard = true;

                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                if (ModState.CustomAnimator != null)
                {
                    Mod.Log.Trace?.Log($"Setting customAnimator params inOrbit: {ModConsts.MOD_ANIMATOR_PARAM_IN_ORBIT}");
                    ModState.CustomAnimator.SetBool(ModConsts.MOD_ANIMATOR_PARAM_IN_ORBIT, true);
                }

                Mod.Log.Debug?.Log($"Before overlaying meshes");
                DropshipHelper.OverlaySimGameDropshipMeshes(currentDropshipId, config);

                Mod.Log.Debug?.Log($"Before overlaying upgrades");
                EngineeringScreenUIHelper.OverlayCustomUpgrades(config.CustomDropship.Upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }

        }

    }
}

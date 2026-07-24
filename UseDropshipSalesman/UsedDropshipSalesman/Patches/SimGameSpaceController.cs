using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Save.SaveGameStructure;
using CustomAmmoCategoriesPatches;
using ErosionBrushPlugin;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
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
            __instance.sim.RoomManager.RefreshDisplay();

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

                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                UpgradeUIHelper.OverlayCustomUpgrades(config.CustomDropship.Upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else if (config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_ARGO)
           {
                Mod.Log.Debug?.Log($"Setting visuals to HBS_ARGO");
                // Use the default Argo ship with custom upgrades
                DropshipHelper.ToggleSimLeopardVisiblity(false);
                __instance.argo.gameObject.SetActive(true);

                __instance.argoAnimator.SetTrigger("setargo");
                __instance.argoAnimator.SetBool("argo", value: true);

                UpgradeUIHelper.ResetUpgradePanel(__instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else
            {
                Mod.Log.Debug?.Log($"Setting visuals to CUSTOM");
                // Use a custom mesh with custom upgrades
                __instance.leopard.gameObject.SetActive(true);
                DropshipHelper.ToggleSimLeopardVisiblity(false);

                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                Mod.Log.Debug?.Log($"Before overlaying meshes");
                DropshipHelper.OverlaySimGameDropshipMeshes(currentDropshipId, config);

                Mod.Log.Debug?.Log($"Before overlaying upgrades");
                UpgradeUIHelper.OverlayCustomUpgrades(config.CustomDropship.Upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }

            UpgradeUIHelper.RefreshUpgradeIcons(__instance.sim.RoomManager.EngineeringRoom.engineeringScreen, config);
            UpgradeHelper.UpdateDropConfig(config);
            UpgradeHelper.UpdateHangarConfig(config);
            UIHelper.UpdateHangerConfig(config, __instance.sim);

        }


    }
}

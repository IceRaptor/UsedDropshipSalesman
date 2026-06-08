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
            Mod.Log.Trace?.Write("==== SimGameSpaceController_Init - entered");

            Mod.Log.Info?.Write("  Caching HBS Leopard GOs");
            ModState.SGLeopardState = DropshipHelper.BuildSGLeopardState(__instance);

            Mod.Log.Info?.Write("Identifying prefabs to load");
            var prefabsToLoad = new Dictionary<string, string>();
            foreach (KeyValuePair<String, DropshipConfig> kvp in Mod.Config.Dropships)
            {
                DropshipPrefabConfig prefabConfig = kvp.Value.prefab;
                Mod.Log.Info?.Write($" Loading dropship: {kvp.Key} assetBundle: {prefabConfig.AssetBundleId} " +
                    $"prefabPath:{prefabConfig.PrefabPath}");

                if (prefabConfig.AssetBundleId.Equals(ModConsts.HBS_PREFAB_LEOPARD, StringComparison.InvariantCultureIgnoreCase) ||
                    prefabConfig.AssetBundleId.Equals(ModConsts.HBS_PREFAB_ARGO, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Info?.Write($"  Dropship configured to use HBS assets, skipping load.");
                    continue;
                }

                if (!prefabsToLoad.ContainsKey(prefabConfig.AssetBundleId))
                {
                    prefabsToLoad.Add(prefabConfig.AssetBundleId, prefabConfig.PrefabPath);
                }

            }

            foreach (KeyValuePair<string, string> kvp in prefabsToLoad)
            {
                var abm = simGame.DataManager.AssetBundleManager;
                abm.RequestBundle(kvp.Key, delegate
                {
                    Mod.Log.Debug?.Write($" -- Loaded assetBundleId: {kvp.Key}");

                    var assetBundle = abm.GetLoadedAssetBundle(kvp.Key);
                    Mod.Log.Trace?.Write($" -- All assets in bundle: {kvp.Key}");
                    foreach (string n in assetBundle.GetAllAssetNames())
                    {
                        Mod.Log.Trace?.Write($"  ---- {n}");
                    }

                var prefabGO = abm.GetAssetFromBundle<GameObject>(kvp.Value, kvp.Key);
                    Mod.Log.Debug?.Write($"  Prefab is not null? {prefabGO != null}");

                    if (prefabGO != null)
                    {
                        if (!ModState.DropshipPrefabs.ContainsKey(kvp.Key))
                        {
                            ModState.DropshipPrefabs.Add(kvp.Key, prefabGO);
                            Mod.Log.Debug?.Write($"  Loaded prefab for dropship? {prefabGO != null}");
                        }

                    }
                });

            }
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "Dock")]
    static class SimGameSpaceController_Dock
    {
        static void Postfix(SimGameTravelStatus status, bool force, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameSpaceController_Dock - entered");

            Mod.Log.Trace?.Write($"Entry speed is: {__instance.argoAnimator.speed}");
            //__instance.argoAnimator.speed = 0.5f;
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "SetShip")]
    static class SimGameSpaceController_SetShip
    {
        static void Postfix(DropshipType ship, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameSpaceController_SetShip - entered");

            ModState.SimGameSpaceController ??= __instance;

            // TODO: Need to handle a call to set the argo
            // TODO: Need to handle a default new career by disabling argo
            var currentDropshipId = __instance.sim.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            Mod.Log.Info?.Write($"Current dropship is: '{currentDropshipId}', overlaying meshes.");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Write($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

           if (config.prefab.AssetBundleId == ModConsts.HBS_PREFAB_LEOPARD)
           {
                // Use the default Leopard meshes with custom upgrades
                DropshipHelper.ToggleLeopardVisibility(true);
                __instance.argo.gameObject.SetActive(false);
                __instance.leopard.gameObject.SetActive(true);
                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                ModState.DropshipInstances.Values.ForEach(go => go.SetActive(false));
                UpgradeUIHelper.OverlayCustomUpgrades(config.upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else if (config.prefab.AssetBundleId == ModConsts.HBS_PREFAB_ARGO)
           {
                // Use the default Argo ship with custom upgrades
                DropshipHelper.ToggleLeopardVisibility(true);
                __instance.argo.gameObject.SetActive(true);
                __instance.leopard.gameObject.SetActive(false);
                __instance.argoAnimator.SetTrigger("setArgo");
                __instance.argoAnimator.SetBool("argo", value: true);

                ModState.DropshipInstances.Values.ForEach(go => go.SetActive(false));
                UpgradeUIHelper.ResetUpgradePanel(__instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            else
            {
                // Use a custom mesh with custom upgrades
                DropshipHelper.ToggleLeopardVisibility(false);
                __instance.argo.gameObject.SetActive(false);
                __instance.leopard.gameObject.SetActive(true);
                __instance.argoAnimator.SetTrigger("setleopard");
                __instance.argoAnimator.SetBool("argo", value: false);

                ModState.DropshipInstances.Values.ForEach(go => go.SetActive(false));
                DropshipHelper.OverlayDropshipMeshes(currentDropshipId, config);
                UpgradeUIHelper.OverlayCustomUpgrades(config.upgrades, __instance.sim.RoomManager.EngineeringRoom.engineeringScreen);
            }
            UpgradeUIHelper.RefreshUpgradeIcons(__instance.sim.RoomManager.EngineeringRoom.engineeringScreen, config);
            UpgradeHelper.UpdateDropConfig(config);

           // Always force the argo to make the upgrades visible
            __instance.currentShip = DropshipType.Argo;
            __instance.sim.RoomManager.RefreshDisplay();
            __instance.sim.CurDropship = DropshipType.Argo;
            __instance.sim.HasSimShipBeenSet = true;
        }

        //static void RewriteMeshesOntoArgo(GameObject dropshipPrefab)
        //{
        //    //sgsc.argoAnimator.SetBool("argo", false);
        //    //sgsc.argo.gameObject.SetActive(false); // Does nothing?

        //    var argoParent = sgsc.argo.gameObject.transform.parent;
        //    var argoAttach = argoParent.gameObject.FindFirstChildNamed("envPrfArgo_argo");

        //    Mod.Log.Trace?.Write("Instantiating prefab");
        //    var dropship_go = UnityEngine.Object.Instantiate(dropshipPrefab, argoAttach.transform);
        //    dropship_go.SetActive(true);

        //    // Adjust to the center of the first jump point
        //    Mod.Log.Trace?.Write("Adjusting dropship position");
        //    ModState.DropshipGO = dropship_go;
        //    DropshipHelper.AlignSpheriod(dropship_go);

        //    // TODO: Note in docs you must set layer = 20 for visibility
        //    Mod.Log.Trace?.Write("Setting layer = 20 for all GameObjects");
        //    dropship_go.gameObject.layer = 20;
        //    var children = dropship_go.GetComponentsInChildren<GameObject>();
        //    foreach (GameObject child in children)
        //    {
        //        child.gameObject.layer = 20;
        //    }

        //    // Disable argo components
        //    Mod.Log.Trace?.Write("Disabling standard argo meshes");
        //    var leopard = sgsc.argo.gameObject.FindFirstChildNamed("chrPrfVhcl_leopard");
        //    leopard.SetActive(false);

        //    var argoCenter = argoParent.gameObject.FindFirstChildNamed("Center");
        //    argoCenter.SetActive(false);

        //    // Update the mesh to use the battletech shader
        //    Mod.Log.Trace?.Write("Updating shader to BTS shader");
        //    var leopard_mat = leopard.GetComponent<MeshRenderer>().material;
        //    var dropship_mats = dropship_go.GetComponentsInChildren<MeshRenderer>();
        //    foreach (MeshRenderer childMeshRenderer in dropship_mats)
        //    {
        //        Mod.Log.Trace?.Write($"Setting shader to BT shader for render: {childMeshRenderer.gameObject.name}");
        //        childMeshRenderer.material.shader = leopard_mat.shader;
        //    }

        //    Mod.Log.Trace?.Write("Done!");

        //}
    }
}

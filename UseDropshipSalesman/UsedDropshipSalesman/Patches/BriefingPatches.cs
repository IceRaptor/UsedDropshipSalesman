using BattleTech.UI;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(Briefing), "Init", new Type[] { })]
    static class Briefing_Init
    {
        static void Postfix(Briefing __instance)
        {
            Mod.Log.Trace?.Log("==== Briefing_Init - entered");
            Mod.Log.Debug?.Log($"==== Briefing_Init");

            ArgoController argoController = __instance?.loadCam?.leopard;
            GameObject leopardGO = argoController.gameObject;
            GameObject parentGO = leopardGO?.transform?.parent?.gameObject; // Should be 'SpaceLoading'
            Mod.Log.Debug?.Log($"  Briefing Leopard GO: {argoController?.gameObject?.name} with parent: {parentGO?.name}");
            if (argoController != null && String.Equals(parentGO?.name, "SpaceLoading"))
            {
                if (ModState.BriefingLeopardState == null || ModState.BriefingLeopardState.ParentGO == null)
                {
                    DropshipHelper.BuildBriefingLeopardState(argoController);
                }

                // Make sure we have a datamanager reference
                if (ModState.DataManagerUnityInstance == null) 
                {
                    GameObject dataManagerGO = GameObject.Find("DataManager");
                    ModState.DataManagerUnityInstance = dataManagerGO?.GetComponent<DataManagerUnityInstance>();
                    if (ModState.DataManagerUnityInstance == null) { Mod.Log.Warning?.Log("Failed to find DataManagerUnityInstance!"); }
                }

                var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
                Mod.Log.Info?.Log($"Current dropship is: '{currentDropshipId}', overlaying meshes.");
                Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
                if (config == null)
                {
                    Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                    return;
                }

                // TODO: For now, use the Leopard for the argo's dropship
                if (config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_LEOPARD ||
                    config.CustomDropship.Visuals.AssetBundleId == ModConsts.HBS_PREFAB_ARGO)
                {
                    // Use the default Leopard meshes with custom upgrades
                    DropshipHelper.ToggleBriefingLeopardVisbility(true);
                    //leopardGO.gameObject.SetActive(true);

                    //argoAnimator.SetTrigger("setleopard");
                    //argoAnimator.SetBool("argo", value: false);

                    //ModState.BriefingDropshipInstances.Values.ForEach(go => go.SetActive(false));
                }
                else
                {
                    // Use a custom mesh with custom upgrades
                    DropshipHelper.ToggleBriefingLeopardVisbility(false);
                    //leopardGO.gameObject.SetActive(true);

                    //__instance.argoAnimator.SetTrigger("setleopard");
                    ////__instance.argoAnimator.SetBool("argo", value: false);

                    //ModState.BriefingDropshipInstances.Values.ForEach(go => go.SetActive(false));
                    DropshipHelper.OverlayBriefingDropshipMeshes(currentDropshipId, config);
                }

                // SimGame is torn down during combat transition
                ModState.SimGameDropshipInstances.Clear();

            }
            else
            {
                Mod.Log.Debug?.Log($"  LeopardArgoController null, skipping.");
            }

        }
    }
}

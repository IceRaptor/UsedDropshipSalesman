using BattleTech.Data;
using BattleTech.Save.SaveGameStructure;
using CustomAmmoCategoriesPatches;
using ErosionBrushPlugin;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(SimGameSpaceController), "SetShip")]
    static class SimGameSpaceController_SetShip
    {
        private static SimGameSpaceController sgsc;

        static void Postfix(DropshipType ship, SimGameSpaceController __instance)
        {
            Mod.Log.Info?.Write("Updating ship graphics");

            //LoadRequest loadRequest = __instance.sim.DataManager.CreateLoadRequest();
            //loadRequest.AddLoadRequest<AssetBundle>(BattleTechResourceType.AssetBundle, "chrprfvhcl_union", delegate
            //{
            //    Mod.Log.Info?.Write("Loaded union bundle");
            //    SimGameSpaceController_SetShip.sgsc = __instance;
            //    SimGameSpaceController_SetShip.RewriteMeshes();
            //});
            //loadRequest.ProcessRequests();

            __instance.sim.DataManager.AssetBundleManager.RequestBundle("chrprfvhcl_uds_union", delegate
            {
                Mod.Log.Info?.Write("Loaded union bundle");
                SimGameSpaceController_SetShip.sgsc = __instance;
                SimGameSpaceController_SetShip.RewriteMeshes();
            });

        }

        static void RewriteMeshes()
        {
            sgsc.argoAnimator.SetBool("argo", false);
            sgsc.argo.gameObject.SetActive(false); // Does nothing?

            var argoParent = sgsc.argo.gameObject.transform.parent;
            var argoAttach = argoParent.gameObject.FindFirstChildNamed("envPrfArgo_argo");

            //Mod.Log.Info?.Write($"Doing pooled instantiate");
            //var pooled_GO = sgsc.sim.DataManager.PooledInstantiate("assets/character/vehicle/prefabs/uds/uds_union/chrprfvhcl_uds_union.prefab", BattleTechResourceType.Prefab, argoCenter.transform.position, argoCenter.transform.rotation, argoCenter.gameObject.transform);
            //Mod.Log.Info?.Write($" -- done. pooled_GO == null? {pooled_GO == null}");

            var unionBundleLoaded = sgsc.sim.DataManager.AssetBundleManager.IsBundleLoaded("chrprfvhcl_uds_union");
            Mod.Log.Info?.Write($"chrprfvhcl_uds_union assetbundle loaded? {unionBundleLoaded}");

            var unionBundle = sgsc.sim.DataManager.AssetBundleManager.GetLoadedAssetBundle("chrprfvhcl_uds_union");
            var assetNames = unionBundle.GetAllAssetNames();
            Mod.Log.Info?.Write("All assets in bundle:");
            foreach (string n in assetNames)
            {
                Mod.Log.Trace?.Write($"  -- {n}");
            }

            var unionPrefab = sgsc.sim.DataManager.AssetBundleManager.GetAssetFromBundle<GameObject>("assets/character/vehicle/prefabs/uds/uds_union/chrprfvhcl_uds_union.prefab", "chrprfvhcl_uds_union");
            Mod.Log.Info?.Write($"Union prefab !null? {unionPrefab != null}");

            Mod.Log.Trace?.Write("Instantiating prefab");
            var dropship_go = UnityEngine.Object.Instantiate(unionPrefab, argoAttach.transform);
            dropship_go.SetActive(true);

            // Adjust to the center of the first jump point
            Mod.Log.Trace?.Write("Adjusting dropship position");
            //dropship_go.gameObject.transform.localPosition += new Vector3(-40.0f, -115.0f, -185.0f);
            dropship_go.gameObject.transform.localPosition += new Vector3(12.0f, 0.0f, 7.0f);
            dropship_go.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            // TODO: Note in docs you must set layer = 20 for visibility
            Mod.Log.Trace?.Write("Setting layer = 20 for all GameObjects");
            dropship_go.gameObject.layer = 20;
            var children = dropship_go.GetComponentsInChildren<GameObject>();
            foreach (GameObject child in children)
            {
                child.gameObject.layer = 20;
            }

            // Disable argo components
            Mod.Log.Trace?.Write("Disabling standard argo meshes");
            var leopard = sgsc.argo.gameObject.FindFirstChildNamed("chrPrfVhcl_leopard");
            leopard.SetActive(false);

            var argoCenter = argoParent.gameObject.FindFirstChildNamed("Center");
            argoCenter.SetActive(false);

            // Update the mesh to use the battletech shader
            Mod.Log.Trace?.Write("Updating shader to BTS shader");
            var leopard_mat = leopard.GetComponent<MeshRenderer>().material;
            var dropship_mats = dropship_go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRenderer in dropship_mats)
            {
                Mod.Log.Trace?.Write($"Setting shader to BT shader for render: {childMeshRenderer.gameObject.name}");
                childMeshRenderer.material.shader = leopard_mat.shader;
            }

            Mod.Log.Trace?.Write("Done!");

        }
    }
}

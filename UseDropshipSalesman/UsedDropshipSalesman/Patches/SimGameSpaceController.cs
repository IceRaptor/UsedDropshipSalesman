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
    [HarmonyPatch(typeof(SimGameSpaceController), "Dock")]
    static class SimGameSpaceController_Dock
    {
        static void Postfix(SimGameTravelStatus status, bool force, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameSpaceController_Dock - entered");

            Mod.Log.Trace?.Write($"Entry speed is: {__instance.argoAnimator.speed}");
            __instance.argoAnimator.speed = 0.5f;
        }
    }

    [HarmonyPatch(typeof(SimGameSpaceController), "SetShip")]
    static class SimGameSpaceController_SetShip
    {
        private static SimGameSpaceController sgsc;

        static void Postfix(DropshipType ship, SimGameSpaceController __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameSpaceController_SetShip - entered");

            __instance.sim.DataManager.AssetBundleManager.RequestBundle("chrprfvhcl_uds_union", delegate
            {
                Mod.Log.Info?.Write("Loaded union bundle");
                SimGameSpaceController_SetShip.sgsc = __instance;

                var unionBundleLoaded = sgsc.sim.DataManager.AssetBundleManager.IsBundleLoaded("chrprfvhcl_uds_union");
                Mod.Log.Info?.Write($"chrprfvhcl_uds_union assetbundle loaded? {unionBundleLoaded}");

                var unionBundle = sgsc.sim.DataManager.AssetBundleManager.GetLoadedAssetBundle("chrprfvhcl_uds_union");
                Mod.Log.Info?.Write($"All assets in bundle: chrprfvhcl_uds_union");
                foreach (string n in unionBundle.GetAllAssetNames())
                {
                    Mod.Log.Trace?.Write($"  -- {n}");
                }

                var unionPrefab = sgsc.sim.DataManager.AssetBundleManager.GetAssetFromBundle<GameObject>("assets/character/vehicle/prefabs/uds/chrprfvhcl_uds_union.prefab", "chrprfvhcl_uds_union");
                Mod.Log.Info?.Write($"Union prefab !null? {unionPrefab != null}");

                //SimGameSpaceController_SetShip.RewriteMeshesOntoLeopard(unionPrefab);
            });

            __instance.sim.DataManager.AssetBundleManager.RequestBundle("chrprfvhcl_uds_overlord", delegate
            {
                Mod.Log.Info?.Write("Loaded overlord bundle");
                SimGameSpaceController_SetShip.sgsc = __instance;

                var overlordBundleLoaded = sgsc.sim.DataManager.AssetBundleManager.IsBundleLoaded("chrprfvhcl_uds_overlord");
                Mod.Log.Info?.Write($"chrprfvhcl_uds_overlord assetbundle loaded? {overlordBundleLoaded}");

                var overlordBundle = sgsc.sim.DataManager.AssetBundleManager.GetLoadedAssetBundle("chrprfvhcl_uds_overlord");
                Mod.Log.Info?.Write($"All assets in bundle: chrprfvhcl_uds_overlord");
                foreach (string n in overlordBundle.GetAllAssetNames())
                {
                    Mod.Log.Trace?.Write($"  -- {n}");
                }

                var overlordPrefab = sgsc.sim.DataManager.AssetBundleManager.GetAssetFromBundle<GameObject>("assets/character/vehicle/prefabs/uds/chrprfvhcl_uds_overlord.prefab", "chrprfvhcl_uds_overlord");
                Mod.Log.Info?.Write($"Overlord prefab !null? {overlordPrefab != null}");

                SimGameSpaceController_SetShip.RewriteMeshesOntoLeopard(overlordPrefab);
            });

        }

        static void RewriteMeshesOntoLeopard(GameObject dropshipPrefab)
        {
            var parent = sgsc.leopard.gameObject;
            var leopardAttach = parent.gameObject.FindFirstChildNamed("envPrfVhcl_leopard");

            Mod.Log.Trace?.Write("Instantiating prefab");
            var dropship_go = UnityEngine.Object.Instantiate(dropshipPrefab, leopardAttach.transform);
            dropship_go.SetActive(true);

            // TODO: Note in docs you must set layer = 20 for visibility
            Mod.Log.Trace?.Write("Setting layer = 20 for all GameObjects");
            dropship_go.gameObject.layer = 20;
            var children = dropship_go.GetComponentsInChildren<GameObject>();
            foreach (GameObject child in children)
            {
                child.gameObject.layer = 20;
            }

            // Update the mesh to use the battletech shader
            Mod.Log.Trace?.Write("Updating shader to BTS shader");
            var leopard_mat = leopardAttach.GetComponent<MeshRenderer>().material;
            var dropship_mats = dropship_go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRenderer in dropship_mats)
            {
                Mod.Log.Trace?.Write($"Setting shader to BT shader for render: {childMeshRenderer.gameObject.name}");
                childMeshRenderer.material.shader = leopard_mat.shader;
            }

            GameObject leopardEngineGlow = null;
            GameObject leopardEngineFlare = null;
            GameObject leopardEngineJet1 = null;
            ArgoMainEngine leopardEngine = null;
            GameObject leopardRunningLights = null;
            GameObject leopardDecal = null;
            foreach (Transform childT in leopardAttach.transform)
            {
                if (childT.name.Equals("engineSpread (1)", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (leopardEngineGlow == null) { leopardEngineGlow = childT.gameObject; }
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.StartsWith("Point Light", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Lights for the engine
                    if (leopardEngineFlare == null) { leopardEngineFlare = childT.gameObject; }
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.StartsWith("jetFlames", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the jet plumes of engines
                    if (leopardEngineJet1 == null) { leopardEngineJet1 = childT.gameObject; }
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.Equals("LeopardEngine", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Component that controls engines
                    if (leopardEngine == null) { leopardEngine = childT.gameObject.GetComponent<ArgoMainEngine>(); }
                }
                else if (childT.name.Equals("GameObject", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the running lights
                    if (leopardRunningLights == null) { leopardRunningLights = childT.gameObject; }
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.Equals("BattleTech Decal (1)", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the decal attachment
                    if (leopardDecal == null) { leopardDecal = childT.gameObject; }
                }
            }

            // Prep the engine component for the new additions
            //leopardEngine.enabled = false;
            
            // Cores point to the jets (particle systems)
            leopardEngine.engineCores = Array.Empty<ParticleSystem>();

            // Engine spread points to the engine spread (particle system)
            leopardEngine.engineSpread = null;

            // Engine lights points to the point lights, flares to the BTComponent on the same GO
            leopardEngine.engineLights = Array.Empty<Light>();
            leopardEngine.engineFlares = Array.Empty<BTFlare>();

            // Create instances for engine jets
            // TODO: Get this from configuration
            // TODO: Rename prefab attaches to engine_points?
            var attaches_engine_jets = new HashSet<String> { "ap_engine_jets_1", "ap_engine_jets_2", "ap_engine_jets_3", "ap_engine_jets_4" };
            foreach (String ap_name in attaches_engine_jets)
            {
                var attach_point = dropship_go.FindFirstChildNamed(ap_name);
                if (attach_point == null)
                {
                    Mod.Log.Warn?.Write($"Configuration error - engine_jet attach_point: {ap_name} could not be found in the prefab!");
                    continue;
                }

                // Create a new engine jet
                var newEngineJet = UnityEngine.Object.Instantiate(leopardEngineJet1);
                newEngineJet.name = $"engine_jet_{ap_name}";
                newEngineJet.transform.parent = attach_point.transform;
                newEngineJet.transform.position = attach_point.transform.position;
                newEngineJet.transform.rotation = leopardEngineJet1.transform.rotation;
                newEngineJet.transform.localPosition = Vector3.zero;
                newEngineJet.transform.localScale = Vector3.one;
                newEngineJet.SetActive(true);
                leopardEngine.engineCores.Add<ParticleSystem>(newEngineJet.GetComponent<ParticleSystem>());

                // Create a new point flare
                var newEngineFlare = UnityEngine.Object.Instantiate(leopardEngineFlare);
                newEngineFlare.name = $"engine_flare_{ap_name}";
                newEngineFlare.transform.parent = attach_point.transform;
                newEngineFlare.transform.position = attach_point.transform.position;
                newEngineFlare.transform.rotation = leopardEngineFlare.transform.rotation;
                newEngineFlare.transform.localPosition = Vector3.zero;
                newEngineFlare.transform.localScale = Vector3.one;
                newEngineFlare.SetActive(true);
                leopardEngine.engineLights.Add(newEngineFlare.GetComponent<Light>());
                leopardEngine.engineFlares.Add(newEngineFlare.GetComponent<BTFlare>());

                Mod.Log.Trace?.Write($"Instantiated duplicate engine_jet {newEngineJet.name} at {attach_point.name} with position: {attach_point.transform.position}");
            }

            // Create instances for engine glow
            // TODO: Get this from configuration
            // TODO: Rename to 'engine_diffuse_glow' - must be singlar to work with argoEngine
            String attachPointEngineGlow = "ap_engine_lights_1";
            var ap_engineGlow = dropship_go.FindFirstChildNamed(attachPointEngineGlow);
            if (ap_engineGlow != null)
            {
                var newEngineGlow = UnityEngine.Object.Instantiate(leopardEngineGlow);
                newEngineGlow.name = $"engine_glow_{attachPointEngineGlow}";
                newEngineGlow.transform.parent = ap_engineGlow.transform;
                newEngineGlow.transform.position = ap_engineGlow.transform.position;
                newEngineGlow.transform.rotation = leopardEngineGlow.transform.rotation;
                newEngineGlow.transform.localPosition = Vector3.zero;
                newEngineGlow.transform.localScale = Vector3.one;
                newEngineGlow.SetActive(true);
                leopardEngine.engineSpread = newEngineGlow.GetComponent<ParticleSystem>();
            }
            else
            {
                Mod.Log.Warn?.Write($"Configuration error - engine_glow attach_point: {attachPointEngineGlow} could not be found in the prefab!");
            }

            // Create instances for spot lights
            // TODO: Get this from configuration
            var attaches_spot_lights = new HashSet<String> { "ap_lights_spot_1" };

            // Create instances for running lights
            // TODO: Get this from configuration
            var attaches_running_lights = new HashSet<String> { "ap_light_running_top_red_1" };

            // Move the decal
            // TODO: Get this from configuration
            var dropshipDecalAttach = dropship_go.FindFirstChildNamed("ap_decal");
            leopardDecal.gameObject.transform.position = dropshipDecalAttach.transform.position;

            // Disable parent components
            Mod.Log.Trace?.Write("Disabling parent components!");
            var leopardMR = leopardAttach.GetComponent<MeshRenderer>();
            leopardMR.enabled = false;

            Mod.Log.Trace?.Write("Done!");
        }

        static void RewriteMeshesOntoArgo(GameObject dropshipPrefab)
        {
            //sgsc.argoAnimator.SetBool("argo", false);
            //sgsc.argo.gameObject.SetActive(false); // Does nothing?

            var argoParent = sgsc.argo.gameObject.transform.parent;
            var argoAttach = argoParent.gameObject.FindFirstChildNamed("envPrfArgo_argo");

            Mod.Log.Trace?.Write("Instantiating prefab");
            var dropship_go = UnityEngine.Object.Instantiate(dropshipPrefab, argoAttach.transform);
            dropship_go.SetActive(true);

            // Adjust to the center of the first jump point
            Mod.Log.Trace?.Write("Adjusting dropship position");
            ModState.DropshipGO = dropship_go;
            AlignmentHelper.AlignSpheriod(dropship_go);

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

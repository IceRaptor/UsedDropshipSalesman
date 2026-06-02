using BattleTech.Rendering;
using HBS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FSM.StringStateMachine;
using static RootMotion.FinalIK.RagdollUtility;

namespace UsedDropshipSalesman.Helper
{
    internal record SimGameLeopardState
    {
        // Non-mutated references 
        public GameObject RootGO;
        public GameObject EngineGlowGO;
        public GameObject EngineFlare1GO;
        public GameObject EngineFlare2GO;
        public GameObject EngineJet1GO;
        public GameObject EngineJet2GO;
        public GameObject RunningLightsRootGO;
        public GameObject DecalGO;

        public ArgoMainEngine ArgoEngineComp;
        public MeshRenderer BodyMRComp;
        public Material BodyMat;

        // Mutated state references 
        public ParticleSystem[] DefaultAMECores = Array.Empty<ParticleSystem>();
        public Light[] DefaultAMELights = Array.Empty<Light>();
        public BTFlare[] DefaultAMEFlares = Array.Empty<BTFlare>();
    }

    public static class DropshipHelper
    {
        //public static void AlignSpheriod(GameObject dropshipGO)
        //{
        //    if (dropshipGO == null)
        //    {
        //        Mod.Log.Warn?.Write("Invoked without a gameobject!");
        //        return;
        //    }

        //    if (ModState.CurrentTravelStatus == SimGameTravelStatus.WARMING_ENGINES)
        //    {
        //        Mod.Log.Info?.Write("Aligning spheriod dropship docked to jumpship");
        //        // Align docked downward
        //        // Align towards direction of travel
        //        dropshipGO.gameObject.transform.localPosition = new Vector3(12.0f, 0.0f, 7.0f);
        //        dropshipGO.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        //        dropshipGO.gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        //    }
        //    else
        //    {
        //        Mod.Log.Info?.Write("Aligning spheriod dropship for travel");
        //        // Align towards direction of travel
        //        dropshipGO.gameObject.transform.localPosition = new Vector3(12.0f, 0.0f, 7.0f);
        //        dropshipGO.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //        dropshipGO.gameObject.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
        //    }

        //}

        // Mutates the HBS SimGame Leopard heirarchy to host the instantiated prefab. Skips creation if 
        //  the target GO already exists
        public static void OverlayDropshipMeshes(string dropshipId, DropshipConfig config)
        {
            // Check for an existing instance of the prefab already attached to the HBS leopard 
            string dropshipRootName = ModConsts.DROPSHIP_GO_PREFIX + config.prefab.AssetBundleId;

            bool alreadyCreated = ModState.DropshipInstances.TryGetValue(dropshipRootName, out GameObject cachedDropshipRootGO);
            if (alreadyCreated)
            {
                Mod.Log.Debug?.Write($"Dropship {dropshipRootName} GO already created, setting active.");
                cachedDropshipRootGO.SetActive(true);
                return;
            }

            Mod.Log.Info?.Write($"Overlaying prefab: {config.prefab.PrefabPath} onto the leopard");
            ModState.DropshipPrefabs.TryGetValue(config.prefab.AssetBundleId, out GameObject dropshipPrefab);

            Mod.Log.Debug?.Write($"Instantiating prefab: {config.prefab.PrefabPath}");
            GameObject dropshipRootGO = new GameObject(dropshipRootName);
            dropshipRootGO.transform.parent = ModState.SGLeopardState.RootGO.transform;
            dropshipRootGO.transform.position = ModState.SGLeopardState.RootGO.transform.position;
            dropshipRootGO.transform.rotation = ModState.SGLeopardState.RootGO.transform.rotation;
            dropshipRootGO.transform.localScale = Vector3.one;
            var dropshipGO = UnityEngine.Object.Instantiate(dropshipPrefab, dropshipRootGO.transform);

            // HBS scenes expect layer = 20 for these to be visible. Force the issue.
            // TODO: Note in docs you should set layer = 20 for visibility
            Mod.Log.Debug?.Write("Setting layer = 20 for all GameObjects");
            dropshipGO.gameObject.layer = ModConsts.HBS_SIMGAME_DROPSHIP_LAYER;
            var children = dropshipGO.GetComponentsInChildren<GameObject>();
            foreach (GameObject child in children)
            {
                child.gameObject.layer = ModConsts.HBS_SIMGAME_DROPSHIP_LAYER; ;
            }

            // Update the mesh to use the battletech shader
            Mod.Log.Debug?.Write("Updating shader for materials to BTS shader");
            var dropship_mats = dropshipGO.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRenderer in dropship_mats)
            {
                Mod.Log.Trace?.Write($"Setting shader to BT shader for render: {childMeshRenderer.gameObject.name}");
                childMeshRenderer.material.shader = ModState.SGLeopardState.BodyMat.shader;
            }

            // Empty the ArgoMainEngine controller values that we're going to mutate
            ModState.SGLeopardState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
            ModState.SGLeopardState.ArgoEngineComp.engineLights = Array.Empty<Light>();
            ModState.SGLeopardState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();

            // Instance the engine jets and flares 
            // TODO: Get this from configuration
            // TODO: Rename prefab attaches to engine_points?
            foreach (String ap_name in config.prefab.AttachesEngines)
            {
                var attach_point = dropshipGO.FindFirstChildNamed(ap_name);
                if (attach_point == null)
                {
                    Mod.Log.Warn?.Write($"Configuration error - engine_jet attach_point: {ap_name} could not be found in the prefab!");
                    continue;
                }

                // Create a new engine jet
                var newEngineJet = UnityEngine.Object.Instantiate(ModState.SGLeopardState.EngineJet1GO);
                newEngineJet.name = $"engine_jet_{ap_name}";
                newEngineJet.transform.parent = attach_point.transform;
                newEngineJet.transform.position = attach_point.transform.position;
                newEngineJet.transform.rotation = ModState.SGLeopardState.EngineJet1GO.transform.rotation;
                newEngineJet.transform.localPosition = Vector3.zero;
                newEngineJet.transform.localScale = Vector3.one;
                newEngineJet.SetActive(true);
                ModState.SGLeopardState.ArgoEngineComp.engineCores.AddItem<ParticleSystem>(newEngineJet.GetComponent<ParticleSystem>());

                // Create a new point flare
                var newEngineFlare = UnityEngine.Object.Instantiate(ModState.SGLeopardState.EngineFlare1GO);
                newEngineFlare.name = $"engine_flare_{ap_name}";
                newEngineFlare.transform.parent = attach_point.transform;
                newEngineFlare.transform.position = attach_point.transform.position;
                newEngineFlare.transform.rotation = ModState.SGLeopardState.EngineFlare1GO.transform.rotation;
                newEngineFlare.transform.localPosition = Vector3.zero;
                newEngineFlare.transform.localScale = Vector3.one;
                newEngineFlare.SetActive(true);
                ModState.SGLeopardState.ArgoEngineComp.engineLights.AddItem(newEngineFlare.GetComponent<Light>());
                ModState.SGLeopardState.ArgoEngineComp.engineFlares.AddItem(newEngineFlare.GetComponent<BTFlare>());

                Mod.Log.Trace?.Write($"Instantiated duplicate engine_jet {newEngineJet.name} at {attach_point.name} with position: {attach_point.transform.position}");
            }

            // For spot lights, instantiate them
            foreach (String attach_name in config.prefab.AttachesSpotLights)
            {
                var attach_GO = dropshipRootGO.FindFirstChildNamed(attach_name);
                Mod.Log.Debug?.Write($"I should be instantiating a spotlight at attach: {attach_name} with GO != null? {attach_GO != null}");
            }

            // For running lights, instantiate them
            foreach (String attach_name in config.prefab.AttachesRunningLights)
            {
                var attach_GO = dropshipRootGO.FindFirstChildNamed(attach_name);
                Mod.Log.Debug?.Write($"I should be instantiating a running light at attach: {attach_name} with GO != null? {attach_GO != null}");

            }

            // Move the engine glow
            var ap_engineGlow = dropshipGO.FindFirstChildNamed(config.prefab.AttachEngineGlow);
            if (ap_engineGlow != null)
            {
                var newGlow = UnityEngine.Object.Instantiate(ModState.SGLeopardState.EngineGlowGO);
                newGlow.name = $"engine_glow_{config.prefab.AttachEngineGlow}";
                newGlow.transform.parent = ap_engineGlow.transform;
                newGlow.transform.position = ap_engineGlow.transform.position;
                newGlow.transform.rotation = ModState.SGLeopardState.EngineGlowGO.transform.rotation;
                newGlow.transform.localPosition = Vector3.zero;
                newGlow.transform.localScale = Vector3.one;
                newGlow.SetActive(true);
                ModState.SGLeopardState.ArgoEngineComp.engineSpread = newGlow.GetComponent<ParticleSystem>();
            }
            else
            {
                Mod.Log.Warn?.Write($"Configuration error - engine_glow attach_point: {config.prefab.AttachEngineGlow} could not be found in the prefab!");
            }

            // Move the decal
            var ap_decal = dropshipGO.FindFirstChildNamed(config.prefab.AttachDecal);
            if (ap_decal != null)
            {
                var newDecal = UnityEngine.Object.Instantiate(ModState.SGLeopardState.DecalGO);
                newDecal.name = $"decal_{config.prefab.AttachDecal}";
                newDecal.transform.parent = ap_decal.transform;
                newDecal.transform.position = ap_decal.transform.position;
                newDecal.transform.rotation = ModState.SGLeopardState.DecalGO.transform.rotation;
                newDecal.transform.localPosition = Vector3.zero;
                newDecal.transform.localScale = Vector3.one;
                newDecal.SetActive(true);
            }
            else
            {
                Mod.Log.Warn?.Write($"Configuration error - attach_decal attach_point: {config.prefab.AttachDecal} could not be found in the prefab!");
            }

            // Finally set the dropship active and record it as an active instance
            ModState.DropshipInstances.Add(dropshipRootName, dropshipRootGO);
            dropshipRootGO.SetActive(true);
        }

        public static void ToggleLeopardVisibility(bool show = false)
        {
            if (ModState.SimGameSpaceController == null)
            {
                Mod.Log.Error?.Write("Unable to ref SimGameSpaceController, this should never happen!");
            }
            if (ModState.SGLeopardState == null)
            {
                Mod.Log.Error?.Write("Unable to ref SimGameLeopardState, this should never happen!");
            }

            Mod.Log.Debug?.Write($"Updating HBS Leopard mesh to be visible: {show}");

            // Hide the body
            ModState.SGLeopardState.BodyMRComp.enabled = show;
            // TODO: Why am I disabling the singular glow?
            ModState.SGLeopardState.EngineGlowGO.SetActive(show);
            ModState.SGLeopardState.EngineFlare1GO.SetActive(show);
            ModState.SGLeopardState.EngineFlare2GO.SetActive(show);
            ModState.SGLeopardState.EngineJet1GO.SetActive(show);
            ModState.SGLeopardState.EngineJet2GO.SetActive(show);
            ModState.SGLeopardState.RunningLightsRootGO.SetActive(show);
            ModState.SGLeopardState.DecalGO.SetActive(show);

            if (show)
            {
                // Reset the argoEngineState to default values
                ModState.SGLeopardState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
                ModState.SGLeopardState.ArgoEngineComp.engineCores = ModState.SGLeopardState.DefaultAMECores;

                ModState.SGLeopardState.ArgoEngineComp.engineLights = Array.Empty<Light>();
                ModState.SGLeopardState.ArgoEngineComp.engineLights = ModState.SGLeopardState.DefaultAMELights;

                ModState.SGLeopardState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();
                ModState.SGLeopardState.ArgoEngineComp.engineFlares = ModState.SGLeopardState.DefaultAMEFlares;
            }
            else
            {
                ModState.SGLeopardState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
                ModState.SGLeopardState.ArgoEngineComp.engineLights = Array.Empty<Light>();
                ModState.SGLeopardState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();
            }
        }

        public static void ToggleArgoVisibility()
        {

        }

        // Called during mod startup to populate the SinGame leopard state
        internal static SimGameLeopardState BuildSGLeopardState(SimGameSpaceController sgsc)
        {
            var state = new SimGameLeopardState
            {
                RootGO = sgsc.leopard.gameObject
            };
            var leopardAttach = state.RootGO.gameObject.FindFirstChildNamed("envPrfVhcl_leopard");

            state.BodyMRComp = leopardAttach.GetComponent<MeshRenderer>();
            state.BodyMat = state.BodyMRComp.material;

            foreach (Transform childT in leopardAttach.transform)
            {
                if (childT.name.Equals("engineSpread (1)", StringComparison.InvariantCultureIgnoreCase))
                {
                    state.EngineGlowGO ??= childT.gameObject;
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.StartsWith("Point Light", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Lights for the engine
                    state.EngineFlare1GO ??= childT.gameObject;
                    state.EngineFlare2GO ??= childT.gameObject;
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.StartsWith("jetFlames", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the jet plumes of engines
                    state.EngineJet1GO ??= childT.gameObject;
                    state.EngineJet2GO ??= childT.gameObject;
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.Equals("LeopardEngine", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Component that controls engines
                    state.ArgoEngineComp ??= childT.gameObject.GetComponent<ArgoMainEngine>();
                }
                else if (childT.name.Equals("GameObject", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the running lights
                    state.RunningLightsRootGO ??= childT.gameObject;
                    childT.gameObject.SetActive(false);
                }
                else if (childT.name.Equals("BattleTech Decal (1)", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Should be the decal attachment
                    state.DecalGO ??= childT.gameObject;
                }
            }

            if (state.ArgoEngineComp != null)
            {
                state.DefaultAMECores = state.ArgoEngineComp.engineCores.ToArray<ParticleSystem>();
                state.DefaultAMELights = state.ArgoEngineComp.engineLights.ToArray<Light>();
                state.DefaultAMEFlares = state.ArgoEngineComp.engineFlares.ToArray<BTFlare>();
            }

            return state;
        }
    }
}

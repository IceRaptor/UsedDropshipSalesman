using BattleTech.Rendering;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using HBS.Extensions;
using MonoMod.Core.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Defs;
using static FSM.StringStateMachine;
using static RootMotion.FinalIK.RagdollUtility;

namespace UsedDropshipSalesman.Helper
{
    internal record LeopardPrefabState
    {
        // Non-mutated references 
        public GameObject ParentGO;
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
        public SimpleCustomization CamoComp;

        // Mutated state references 
        public ParticleSystem[] DefaultAMECores = Array.Empty<ParticleSystem>();
        public Light[] DefaultAMELights = Array.Empty<Light>();
        public BTFlare[] DefaultAMEFlares = Array.Empty<BTFlare>();

        public override string ToString()
        {
            return 
                $"ParentGO.name: {(ParentGO == null ? "NULL" : ParentGO?.name)}  " +
                $"EngineGlowGO.name: {(EngineGlowGO == null ? "NULL" : EngineGlowGO?.name)}  " +
                $"EngineFlare1GO.name: {(EngineFlare1GO == null ? "NULL" : EngineFlare1GO?.name)}  " +
                $"EngineFlare2GO.name: {(EngineFlare2GO == null ? "NULL" : EngineFlare2GO?.name)}  " +
                $"EngineJet1GO.name: {(EngineJet1GO == null ? "NULL" : EngineFlare1GO.name)}  " +
                $"EngineFlare2GO.name: {(EngineFlare2GO == null ? "NULL" : EngineFlare2GO?.name)}  " +
                $"RunningLightsRootGO.name: {(RunningLightsRootGO == null ? "NULL" : RunningLightsRootGO?.name)}  " +
                $"DecalGO.name: {(DecalGO == null ? "NULL" : DecalGO?.name)}  " +
                $"ArgoEngineComp is null? {ArgoEngineComp == null}  BodyMRComp is null? {BodyMRComp == null} " +
                $"BodyMat is null? {BodyMat == null}  CamoComp is null? {CamoComp == null} " +
                $"DefaultAMECores == null? {DefaultAMECores == null}  DefaultAMECores.Size: {(DefaultAMECores != null ? DefaultAMECores?.Length : 0)} " +
                $"DefaultAMELights == null? {DefaultAMELights == null}  DefaultAMELights.Size: {(DefaultAMELights != null ? DefaultAMELights?.Length : 0)} " +
                $"DefaultAMEFlares == null? {DefaultAMEFlares == null}  DefaultAMEFlares.Size: {(DefaultAMEFlares != null ? DefaultAMEFlares?.Length : 0)} ";
        }
    }

    public static class DropshipHelper
    {

        internal static void OverlaySimGameDropshipMeshes(string dropshipId, DropshipConfig config)
        {
            OverlayDropshipMeshes(dropshipId, config, true);
        }

        internal static void OverlayBriefingDropshipMeshes(string dropshipId, DropshipConfig config)
        {
            OverlayDropshipMeshes(dropshipId, config, false);
        }

        // Mutates the HBS SimGame Leopard heirarchy to host the instantiated prefab. Skips creation if 
        //  the target GO already exists
        private static void OverlayDropshipMeshes(string dropshipId, DropshipConfig config, bool isSimGame)
        {
            // Check for an existing instance of the prefab already attached to the HBS leopard 
            string dropshipRootName = ModConsts.DROPSHIP_GO_PREFIX + config.CustomDropship.Visuals.AssetBundleId;

            GameObject cachedDropshipRootGO;
            bool alreadyCreated = isSimGame ? 
                ModState.SimGameDropshipInstances.TryGetValue(dropshipRootName, out cachedDropshipRootGO) :
                ModState.BriefingDropshipInstances.TryGetValue(dropshipRootName, out cachedDropshipRootGO);
            
            if (alreadyCreated)
            {
                Mod.Log.Debug?.Log($"Dropship {dropshipRootName} GO already created, setting active.");
                cachedDropshipRootGO.SetActive(true);
                return;
            }

            string scene = isSimGame ? "simGame" : "briefing";
            Mod.Log.Info?.Log($"Overlaying prefab: {config.CustomDropship.Visuals.PrefabPath} onto the { scene } leopard");

            // Fetch the prefab from the assetBundle that's already been loaded.
            var abm = ModState.DataManagerUnityInstance.DataManager.AssetBundleManager;
            var assetBundle = abm.GetLoadedAssetBundle(config.CustomDropship.Visuals.AssetBundleId);
            if (assetBundle == null)
            {
                Mod.Log.Info?.Log("Dropships not loaded, loading assetbundles and short-circuiting");
                if (isSimGame) { DropshipHelper.LoadAssetBundle(config, OverlaySimGameMeshes); }
                else { DropshipHelper.LoadAssetBundle(config, OverlayBriefingMeshes); }
                return;
            }
            else
            {
                if (isSimGame) { DropshipHelper.LoadAssetBundle(config, OverlaySimGameMeshes); }
                else { DropshipHelper.LoadAssetBundle(config, OverlayBriefingMeshes); }
            }
        }

        private static void OverlaySimGameMeshes(DropshipConfig config)
        {
            OverlayMeshes(config, true);
        }

        private static void OverlayBriefingMeshes(DropshipConfig config)
        {
            OverlayMeshes(config, false);
        }

        private static void OverlayMeshes(DropshipConfig config, bool isSimGame)
        {

            string dropshipRootName = ModConsts.DROPSHIP_GO_PREFIX + config.CustomDropship.Visuals.AssetBundleId;
            var abm = ModState.DataManagerUnityInstance.DataManager.AssetBundleManager;

            var prefabGO = abm.GetAssetFromBundle<GameObject>(config.CustomDropship.Visuals.PrefabPath, config.CustomDropship.Visuals.AssetBundleId);
            Mod.Log.Debug?.Log($"  AssetBundleId: {config.CustomDropship.Visuals.AssetBundleId}  prefabPath: {config.CustomDropship.Visuals.PrefabPath}");
            Mod.Log.Warning?.Log($"PREFAB_GO == null? {prefabGO == null}");

            Mod.Log.Debug?.Log($"Instantiating prefab: {config.CustomDropship.Visuals.PrefabPath}");
            LeopardPrefabState leopardPrefabState = isSimGame ? ModState.SimGameLeopardState : ModState.BriefingLeopardState;
            GameObject dropshipRootGO = new GameObject(dropshipRootName);
            dropshipRootGO.transform.parent = leopardPrefabState.ParentGO.transform;
            dropshipRootGO.transform.position = leopardPrefabState.ParentGO.transform.position;
            dropshipRootGO.transform.rotation = leopardPrefabState.ParentGO.transform.rotation;
            dropshipRootGO.transform.localScale = Vector3.one;
            var dropshipGO = UnityEngine.Object.Instantiate(prefabGO, dropshipRootGO.transform);

            // HBS scenes expect layer = 20 for these to be visible. Force the issue.
            // TODO: Note in docs you should set layer = 20 for visibility
            Mod.Log.Debug?.Log("Setting layer = 20 for all GameObjects");
            dropshipGO.gameObject.layer = ModConsts.HBS_LEOPARD_PREFAB_LAYER;
            var children = dropshipGO.GetComponentsInChildren<GameObject>();
            foreach (GameObject child in children)
            {
                child.gameObject.layer = ModConsts.HBS_LEOPARD_PREFAB_LAYER;
            }

            // Update the mesh to use the battletech shader
            Mod.Log.Debug?.Log("Updating shader for materials to BTS shader");
            var dropship_mats = dropshipGO.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRenderer in dropship_mats)
            {
                Mod.Log.Trace?.Log($"Setting shader to BT shader for render: {childMeshRenderer.gameObject.name}");
                childMeshRenderer.material.shader = leopardPrefabState.BodyMat.shader;
                childMeshRenderer.gameObject.layer = ModConsts.HBS_LEOPARD_PREFAB_LAYER;
            }

            // Transfer the camo pattern texture
            Mod.Log.Debug?.Log("Updating camo holder texture."); 
            GameObject camoholderGO = dropshipGO.FindFirstChildNamed("camoholder");
            MeshRenderer camoMeshRenderer = camoholderGO.GetComponent<MeshRenderer>();
            leopardPrefabState.CamoComp.paintSchemeTex = (Texture2D)camoMeshRenderer.material.mainTexture;
            leopardPrefabState.CamoComp.UpdateHeraldry();

            // Empty the ArgoMainEngine controller values that we're going to mutate
            leopardPrefabState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
            leopardPrefabState.ArgoEngineComp.engineLights = Array.Empty<Light>();
            leopardPrefabState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();

            // Instance the engine jets and flares 
            // TODO: Get this from configuration
            // TODO: Rename prefab attaches to engine_points?
            Mod.Log.Debug?.Log("Updating engine attaches");
            foreach (String ap_name in config.CustomDropship.Visuals.AttachesEngines)
            {
                var attach_point = dropshipGO.FindFirstChildNamed(ap_name);
                if (attach_point == null)
                {
                    Mod.Log.Warning?.Log($"Configuration error - engine_jet attach_point: {ap_name} could not be found in the prefab!");
                    continue;
                }

                // Create a new engine jet
                var newEngineJet = UnityEngine.Object.Instantiate(leopardPrefabState.EngineJet1GO);
                newEngineJet.name = $"engine_jet_{ap_name}";
                newEngineJet.transform.parent = attach_point.transform;
                newEngineJet.transform.position = attach_point.transform.position;
                newEngineJet.transform.rotation = leopardPrefabState.EngineJet1GO.transform.rotation;
                newEngineJet.transform.localPosition = Vector3.zero;
                newEngineJet.transform.localScale = Vector3.one;
                Mod.Log.Trace?.Log($"  Created newEngineJet: {newEngineJet.name}");
                newEngineJet.SetActive(true);
                leopardPrefabState.ArgoEngineComp.engineCores.AddItem<ParticleSystem>(newEngineJet.GetComponent<ParticleSystem>());

                // Create a new point flare
                var newEngineFlare = UnityEngine.Object.Instantiate(leopardPrefabState.EngineFlare1GO);
                newEngineFlare.name = $"engine_flare_{ap_name}";
                newEngineFlare.transform.parent = attach_point.transform;
                newEngineFlare.transform.position = attach_point.transform.position;
                newEngineFlare.transform.rotation = leopardPrefabState.EngineFlare1GO.transform.rotation;
                newEngineFlare.transform.localPosition = Vector3.zero;
                newEngineFlare.transform.localScale = Vector3.one;
                newEngineFlare.SetActive(true);
                Mod.Log.Trace?.Log($"  Created newEngineFlare: {newEngineFlare.name}");
                leopardPrefabState.ArgoEngineComp.engineLights.AddItem(newEngineFlare.GetComponent<Light>());
                leopardPrefabState.ArgoEngineComp.engineFlares.AddItem(newEngineFlare.GetComponent<BTFlare>());

                Mod.Log.Trace?.Log($"Instantiated duplicate engine_jet {newEngineJet.name} at {attach_point.name} with position: {attach_point.transform.position}");
            }

            // For spot lights, instantiate them
            Mod.Log.Debug?.Log("Updating spot lights");
            foreach (String attach_name in config.CustomDropship.Visuals.AttachesSpotLights)
            {
                var attach_GO = dropshipRootGO.FindFirstChildNamed(attach_name);
                Mod.Log.Debug?.Log($"I should be instantiating a spotlight at attach: {attach_name} with GO != null? {attach_GO != null}");
            }

            // For running lights, instantiate them
            Mod.Log.Debug?.Log("Updating running lights");
            foreach (String attach_name in config.CustomDropship.Visuals.AttachesRunningLights)
            {
                var attach_GO = dropshipRootGO.FindFirstChildNamed(attach_name);
                Mod.Log.Debug?.Log($"I should be instantiating a running light at attach: {attach_name} with GO != null? {attach_GO != null}");

            }

            // Move the engine glow
            Mod.Log.Debug?.Log("Updating engine glow");
            var ap_engineGlow = dropshipGO.FindFirstChildNamed(config.CustomDropship.Visuals.AttachEngineGlow);
            if (ap_engineGlow != null)
            {
                var newGlow = UnityEngine.Object.Instantiate(leopardPrefabState.EngineGlowGO);
                newGlow.name = $"engine_glow_{config.CustomDropship.Visuals.AttachEngineGlow}";
                newGlow.transform.parent = ap_engineGlow.transform;
                newGlow.transform.position = ap_engineGlow.transform.position;
                newGlow.transform.rotation = leopardPrefabState.EngineGlowGO.transform.rotation;
                newGlow.transform.localPosition = Vector3.zero;
                newGlow.transform.localScale = Vector3.one;
                newGlow.SetActive(true);
                leopardPrefabState.ArgoEngineComp.engineSpread = newGlow.GetComponent<ParticleSystem>();
            }
            else
            {
                Mod.Log.Warning?.Log($"Configuration error - engine_glow attach_point: {config.CustomDropship.Visuals.AttachEngineGlow} could not be found in the prefab!");
            }

            // Move the decal
            Mod.Log.Debug?.Log("Updating decal");
            var ap_decal = dropshipGO.FindFirstChildNamed(config.CustomDropship.Visuals.AttachDecal);
            if (ap_decal != null)
            {
                var newDecal = UnityEngine.Object.Instantiate(leopardPrefabState.DecalGO);
                newDecal.name = $"decal_{config.CustomDropship.Visuals.AttachDecal}";
                newDecal.transform.parent = ap_decal.transform;
                newDecal.transform.position = ap_decal.transform.position;
                newDecal.transform.rotation = leopardPrefabState.DecalGO.transform.rotation;
                newDecal.transform.localPosition = Vector3.zero;
                newDecal.transform.localScale = Vector3.one;
                newDecal.SetActive(true);
            }
            else
            {
                Mod.Log.Warning?.Log($"Configuration error - attach_decal attach_point: {config.CustomDropship.Visuals.AttachDecal} could not be found in the prefab!");
            }

            // Finally set the dropship active and record it as an active instance
            if (isSimGame) { ModState.SimGameDropshipInstances.Add(dropshipRootName, dropshipRootGO); }
            //else { ModState.BriefingDropshipInstances.Add(dropshipRootName, dropshipRootGO); }
            
            dropshipRootGO.SetActive(true);
        }

        internal static void ToggleSimLeopardVisiblity(bool show = false)
        {
            if (ModState.SimGameLeopardState == null)
            {
                Mod.Log.Error?.Log("Unable to ref SimGameLeopardState, this should never happen!");
                return;
            }

            DropshipHelper.ToggleLeopardVisibility(true, show);
        }
        internal static void ToggleBriefingLeopardVisbility(bool show = false)
        {
            if (ModState.BriefingLeopardState == null)
            {
                Mod.Log.Error?.Log("Unable to ref BriefingLeopardState, this should never happen!");
                return;
            }

            DropshipHelper.ToggleLeopardVisibility(false, show);
        }

        private static void ToggleLeopardVisibility(bool isSimGame, bool show = false)
        {

            Mod.Log.Debug?.Log($"Updating HBS Leopard mesh to be visible: {show}  isSimGame: {isSimGame}");
            LeopardPrefabState prefabState = isSimGame ? ModState.SimGameLeopardState : ModState.BriefingLeopardState;
            if (prefabState == null)
            {
                Mod.Log.Warning?.Log($"Failed to find a prefabState to manipulate, fast-failing!");
                return;
            }
            Mod.Log.Debug?.Log($"PrefabState = {prefabState}");

            // Hide the body
            prefabState.BodyMRComp.enabled = show;
            // TODO: Why am I disabling the singular glow?
            prefabState.EngineGlowGO.SetActive(show);
            prefabState.EngineFlare1GO.SetActive(show);
            prefabState.EngineFlare2GO.SetActive(show);
            prefabState.EngineJet1GO.SetActive(show);
            prefabState.EngineJet2GO.SetActive(show);
            prefabState.RunningLightsRootGO.SetActive(show);
            prefabState.DecalGO.SetActive(show);

            Mod.Log.Debug?.Log($"Updating engine lights");
            if (show)
            {
                // Reset the argoEngineState to default values
                prefabState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
                prefabState.ArgoEngineComp.engineCores = prefabState.DefaultAMECores;

                prefabState.ArgoEngineComp.engineLights = Array.Empty<Light>();
                prefabState.ArgoEngineComp.engineLights = prefabState.DefaultAMELights;

                prefabState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();
                prefabState.ArgoEngineComp.engineFlares = prefabState.DefaultAMEFlares;
            }
            else
            {
                prefabState.ArgoEngineComp.engineCores = Array.Empty<ParticleSystem>();
                prefabState.ArgoEngineComp.engineLights = Array.Empty<Light>();
                prefabState.ArgoEngineComp.engineFlares = Array.Empty<BTFlare>();
            }

            Mod.Log.Debug?.Log($"Done updating leopard visibility");
        }

        internal static void BuildSimLeopardState(SimGameSpaceController sgsc)
        {
            GameObject leopardPrefabGO = sgsc.leopard.gameObject;
            Mod.Log.Debug?.Log($"Building leopardState for SimGame scene with leopardGO: '{leopardPrefabGO?.name}'");
            ModState.SimGameLeopardState = BuildLeopardState(leopardPrefabGO);
            if (ModState.SimGameLeopardState != null) { Mod.Log.Debug?.Log($"SimGameLeopardState is: {ModState.BriefingLeopardState}"); }
            else { Mod.Log.Debug?.Log($"  SimGameLeopardState == null!"); }
        }
        internal static void BuildBriefingLeopardState(ArgoController leopardArgoController)
        {
            GameObject leopardPrefabGO = leopardArgoController?.gameObject;
            Mod.Log.Debug?.Log($"Building leopardState for Briefing scene with leopardGO: '{leopardPrefabGO?.name}'");
            ModState.BriefingLeopardState = BuildLeopardState(leopardPrefabGO);
            if (ModState.BriefingLeopardState != null) { Mod.Log.Debug?.Log($"BriefingLeopardState is: {ModState.BriefingLeopardState}");  }
            else { Mod.Log.Debug?.Log($"  BriefingLeopardState == null!");  }
        }

        // Called during mod startup to populate the SinGame leopard state
        private static LeopardPrefabState BuildLeopardState(GameObject leopardPrefabGO)
        {
            LeopardPrefabState state = new()
            {
                ParentGO = leopardPrefabGO,
                BodyMRComp = leopardPrefabGO.GetComponent<MeshRenderer>(),
                CamoComp = leopardPrefabGO.GetComponent<SimpleCustomization>()
            };
            state.BodyMat = state.BodyMRComp.material;

            foreach (Transform childT in leopardPrefabGO.transform)
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

        public static void LoadAssetBundle(DropshipConfig config, Action<DropshipConfig> callback)
        {
            Mod.Log.Info?.Log($"Loading assetBundle for dropship: {config.CustomDropship.Description.Id}");
            var abm = ModState.DataManagerUnityInstance.DataManager.AssetBundleManager;
            var onLoaded = delegate (AssetBundle ab)
            {
                Mod.Log.Debug?.Log($" -- Loaded assetBundleId: {ab.name}");

                var assetBundle = abm.GetLoadedAssetBundle(ab.name);
                Mod.Log.Trace?.Log($" -- All assets in bundle: {ab.name}");
                foreach (string n in assetBundle.GetAllAssetNames())
                {
                    Mod.Log.Trace?.Log($"  ---- {n}");
                }

                callback(config);
            };
            abm.RequestBundle(config.CustomDropship.Visuals.AssetBundleId, onLoaded);
        }

        //public static void LoadAllAssetBundles(SimGameState sgs)
        //{
        //    Mod.Log.Info?.Log("Identifying prefabs to load");
        //    var prefabsToLoad = new Dictionary<string, string>();
        //    foreach (KeyValuePair<String, DropshipConfig> kvp in Mod.Config.Dropships)
        //    {
        //        DropshipVisuals prefabConfig = kvp.Value.CustomDropship.Visuals;
        //        Mod.Log.Info?.Log($" Loading dropship: {kvp.Key} assetBundle: {prefabConfig.AssetBundleId} " +
        //            $"prefabPath:{prefabConfig.PrefabPath}");

        //        if (prefabConfig.AssetBundleId.Equals(ModConsts.HBS_PREFAB_LEOPARD, StringComparison.InvariantCultureIgnoreCase) ||
        //            prefabConfig.AssetBundleId.Equals(ModConsts.HBS_PREFAB_ARGO, StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            Mod.Log.Info?.Log($"  Dropship configured to use HBS assets, skipping load.");
        //            continue;
        //        }

        //        if (!prefabsToLoad.ContainsKey(prefabConfig.AssetBundleId))
        //        {
        //            prefabsToLoad.Add(prefabConfig.AssetBundleId, prefabConfig.PrefabPath);
        //        }
        //    }

        //    List<Action<AssetBundle>> callbacks = new List<Action<AssetBundle>>();
        //    foreach (KeyValuePair<string, string> kvp in prefabsToLoad)
        //    {
        //        var abm = sgs.DataManager.AssetBundleManager;
        //        var onLoaded = delegate(AssetBundle ab)
        //        {
        //            Mod.Log.Debug?.Log($" -- Loaded assetBundleId: {ab.name}");

        //            var assetBundle = abm.GetLoadedAssetBundle(ab.name);
        //            Mod.Log.Trace?.Log($" -- All assets in bundle: {ab.name}");
        //            foreach (string n in assetBundle.GetAllAssetNames())
        //            {
        //                Mod.Log.Trace?.Log($"  ---- {n}");
        //            }
        //        };
        //        callbacks.Add(onLoaded);
        //        abm.RequestBundle(kvp.Key, onLoaded);
        //    }
        //}
    }
}

using BattleTech.Data;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using FluffyUnderware.DevTools.Extensions;
using HBS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RootMotion.FinalIK.RagdollUtility;

namespace UsedDropshipSalesman.Helper
{
    public static class UIHelper
    {
        public static void BuildDropshipActionButtons(GameObject parentGO, GameObject prefabGO, CombatGameState combat, CombatHUD hud, AbstractActor actor)
        {
            if (parentGO == null || prefabGO == null || combat == null || hud == null) return;

            Mod.Log.Debug?.Log($"Creating new Dropship action buttons for actor: {actor?.DisplayName}-{actor?.GetPilot()?.Name}");
            try
            {
                GameObject newButtonGO_activeProbe = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_activeProbe.transform.localPosition = new Vector3(0, 0, 0);
                newButtonGO_activeProbe.name = $"UDS_DROPSHIP_CMD_BTN_ACTIVE_PROBE";
                InitializeButtons(newButtonGO_activeProbe, "AbilityDefCMD_UDS_ActiveProbe_Ping", combat, hud, actor);

                GameObject newButtonGO_artThumperAP = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_artThumperAP.transform.localPosition = new Vector3(55, 0, 0);
                newButtonGO_artThumperAP.name = $"UDS_DROPSHIP_CMD_BTN_ARTILLERY_AP";
                InitializeButtons(newButtonGO_artThumperAP, "AbilityDefCMD_UDS_ArtThumperAP", combat, hud, null);

                GameObject newButtonGO_artThumperAE = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_artThumperAE.transform.localPosition = new Vector3(110, 0, 0);
                newButtonGO_artThumperAE.name = $"UDS_DROPSHIP_CMD_BTN_ARTILLERY_HE";
                InitializeButtons(newButtonGO_artThumperAE, "AbilityDefCMD_UDS_ArtThumperHE", combat, hud, null);

                GameObject newButtonGO_Strafe = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_Strafe.transform.localPosition = new Vector3(165, 0, 0);
                newButtonGO_Strafe.name = $"UDS_DROPSHIP_CMD_BTN_STRAFE";
                InitializeButtons(newButtonGO_Strafe, "AbilityDefCMD_UDS_Strafe", combat, hud, null);
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Log("Failed to initialize all dropship action buttons!", e);
            }
        }

        public static void InitializeButtons(GameObject commandButtonGO, string abilityId, CombatGameState combat, CombatHUD hud, AbstractActor actor)
        {
            Mod.Log.Debug?.Log($"Initializing buttonGO: {commandButtonGO.name}  with ability: {abilityId}");

            commandButtonGO.SetActive(true);

            bool had_key = combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");
            Ability ability = new(abilityDef);

            CombatHUDActionButton button1_CHUDAB = commandButtonGO.GetComponent<CombatHUDActionButton>();
            Mod.Log.Debug?.Log($"button1_CHUDAB is null? {button1_CHUDAB == null}  name: {button1_CHUDAB?.name}");

            //button1_CHUDAB.Init(combat, hud, BTInput.Instance.Combat_CommandAbility());
            button1_CHUDAB.Init(combat, hud, BTInput.Instance.Key_None());

            SelectionType abilitySelectionType = CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false);
            button1_CHUDAB.InitButton(abilitySelectionType, ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, actor);
            Mod.Log.Debug?.Log($"Initialized button with ability: {ability?.Def?.Description?.Id}:{ability?.Def?.Description?.Name} with" +
                $"selectionType: {abilitySelectionType}  activationCooldown: {ability?.Def?.ActivationCooldown}  activationETA: {ability?.Def?.ActivationETA}");

            button1_CHUDAB.RefreshActive();

            Mod.Log.Debug?.Log($"Initialized command button for ability: {abilityId}");
        }

        public static void BuildDropshipCommandButtons(GameObject parentGO, GameObject prefabGO, CombatGameState combat, CombatHUD hud, AbstractActor actor)
        {
            if (parentGO == null || prefabGO == null || combat == null || hud == null) return;

            Mod.Log.Debug?.Log($"Creating new Dropship Command buttons for actor: {actor?.DisplayName}-{actor?.GetPilot()?.Name}");
            try
            {
                GameObject newButtonGO_activeProbe = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_activeProbe.transform.localPosition = new Vector3(0, 0, 0);
                newButtonGO_activeProbe.name = $"UDS_DROPSHIP_CMD_BTN_ACTIVE_PROBE";
                InitializeCommandButton(newButtonGO_activeProbe, "AbilityDefCMD_UDS_ActiveProbe_Ping", combat, hud, actor);

                GameObject newButtonGO_artThumperAP = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_artThumperAP.transform.localPosition = new Vector3(100, 0, 0);
                newButtonGO_artThumperAP.name = $"UDS_DROPSHIP_CMD_BTN_ARTILLERY_AP";
                InitializeCommandButton(newButtonGO_artThumperAP, "AbilityDefCMD_UDS_ArtThumperAP", combat, hud, null);

                GameObject newButtonGO_artThumperAE = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_artThumperAE.transform.localPosition = new Vector3(200, 0, 0);
                newButtonGO_artThumperAE.name = $"UDS_DROPSHIP_CMD_BTN_ARTILLERY_HE";
                InitializeCommandButton(newButtonGO_artThumperAE, "AbilityDefCMD_UDS_ArtThumperHE", combat, hud, null);

                GameObject newButtonGO_Strafe = UnityEngine.Object.Instantiate(prefabGO, parentGO.transform);
                newButtonGO_Strafe.transform.localPosition = new Vector3(300, 0, 0);
                newButtonGO_Strafe.name = $"UDS_DROPSHIP_CMD_BTN_STRAFE";
                InitializeCommandButton(newButtonGO_Strafe, "AbilityDefCMD_UDS_Strafe", combat, hud, null);
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Log("Failed to initialize all dropship command buttons!", e);
            }

        }

        public static void InitializeCommandButton(GameObject commandButtonGO, string abilityId, CombatGameState combat, CombatHUD hud, AbstractActor actor)
        {
            Mod.Log.Debug?.Log($"Initializing buttonGO: {commandButtonGO.name}  with ability: {abilityId}");

            commandButtonGO.SetActive(true);

            bool had_key = combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");
            Ability ability = new(abilityDef);

            CombatHUDActionButton button1_CHUDAB = commandButtonGO.GetComponent<CombatHUDActionButton>();
            Mod.Log.Debug?.Log($"button1_CHUDAB is null? {button1_CHUDAB == null}  name: {button1_CHUDAB?.name}");

            button1_CHUDAB.Init(combat, hud, BTInput.Instance.Combat_CommandAbility());

            SelectionType abilitySelectionType = CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false);
            button1_CHUDAB.InitButton(abilitySelectionType,ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, actor);
            Mod.Log.Debug?.Log($"Initialized button with ability: {ability?.Def?.Description?.Id}:{ability?.Def?.Description?.Name} with" +
                $"selectionType: {abilitySelectionType}  activationCooldown: {ability?.Def?.ActivationCooldown}  activationETA: {ability?.Def?.ActivationETA}");

            button1_CHUDAB.RefreshActive();

            Mod.Log.Debug?.Log($"Initialized command button for ability: {abilityId}");
        }

        public static void UpdateHangerConfig(DropshipConfig config, SimGameState sgs)
        {
            Mod.Log.Trace?.Log("==== UpgradeHelper_UpdateHangerConfig - entered.");

            MechBayPanel mbp = sgs.RoomManager.MechBayRoom.mechBay;
            CustomBaysUICaster baysUI = mbp.gameObject.GetComponentInChildren<CustomBaysUICaster>(true);
            if (baysUI == null)
            {
                Mod.Log.Info?.Log("BaysUI is still null!");
                return;
            }
            Mod.Log.Info?.Log($"MBP: {mbp.name}  BayUI: {baysUI.name}  currentBay: {baysUI.currentBay?.name}");

            // uixPrfPanl_SIM_mechBayNav-Widget(Clone)
            // uixPrfPanl_SIM_mechBayNav-Widget(Clone) / Representation / layout_tabs / uixPrfBttn_BASE_TabMedium-tab-bays / bays

            GameObject mechBarNavGO = baysUI.transform.parent.transform.parent.gameObject;
            Mod.Log.Debug?.Log($"MechBarNav parent != null? {mechBarNavGO != null}  name: {mechBarNavGO?.name}");

            // Buttons - buttons to toggle mechbay view
            // uixPrfBttn_BASE_TabMedium-bays0 - mechbay, has CustomBayShower, CustomBaysButton comps
            // uixPrfBttn_BASE_TabMedium-bays1 - vech_bay, has CustomHangerInfo, CustomBaysButton comps
            // uixPrfBttn_BASE_TabMedium-bays2 - ba_bay, , has CustomHangerInfo, CustomBaysButton comps


            // uixPrfPanl_SIM_mechBayNav-Widget(Clone) / Representation / layout_content / obj_list

            // Bays - has CustomHangerInfo component
            //  uixPrfPanl_SIM_mechBays-Widget-MANAGED 
            //  uixPrfPanl_SIM_mechStorage-Widget-MANAGED
            //  uixPrfPanl_inventory-Widget-MANAGED

            GameObject mechBayPanelGO = mechBarNavGO.FindFirstChildNamed("uixPrfPanl_SIM_mechBays-Widget-MANAGED");
            Mod.Log.Debug?.Log($"MechBayPanel != null? {mechBayPanelGO != null}  name: {mechBayPanelGO?.name}");
            //   / uixPrfPanl_SIM_mechBays-Widget-MANAGED / Representation / layout_baysScroller / 
            //     / layout_baysScroller / viewport_storage / content_storage

            // Rows
            // uixPrfPanl_SIM_mechBay_bay-Element-MANAGED-prime 
            // uixPrfPanl_SIM_mechBay_bay-Element-MANAGED
            // uixPrfPanl_SIM_mechBay_bay-Element-MANAGED

            GameObject dropSlotsGO = mechBayPanelGO.FindFirstChildNamed("DropSlots");
            Mod.Log.Debug?.Log($"dropSlotsGO != null? {dropSlotsGO != null}  name: {dropSlotsGO?.name}");

            List<GameObject> dropSlotBayGO = new();
            foreach (Transform childT in dropSlotsGO.transform)
            {
                dropSlotBayGO.Add(childT.gameObject);
            }
            //dropSlotBayGO[4].SetActive(false);
            //dropSlotBayGO[5].SetActive(false);

            // uixPrfPanl_SIM_mechBay_bay-Element-MANAGED-prime / Representation / bg_fill / DropSlots
            //  uixPrfPanl_MechBayDropSlot-MANAGED
            //  uixPrfPanl_MechBayDropSlot-MANAGED
            //  uixPrfPanl_MechBayDropSlot-MANAGED
            //  uixPrfPanl_MechBayDropSlot-MANAGED
            //  uixPrfPanl_MechBayDropSlot-MANAGED
            //  uixPrfPanl_MechBayDropSlot-MANAGED

            // Bays
            // uixPrfPanl_SIM_mechBay_bay-Element-MANAGED-prime  / Representation / bg_fill / DropSlots

        }
    }
}

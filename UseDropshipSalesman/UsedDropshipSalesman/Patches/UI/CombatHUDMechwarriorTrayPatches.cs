using BattleTech.Data;
using BattleTech.UI;
using BestHTTP.SignalR.Hubs;
using CustomUnits;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;
using static Utilities;

namespace UsedDropshipSalesman.Patches.UI
{

    [HarmonyPatch(typeof(CombatHUDMechwarriorTray), "Init")]
    static class CombatHUDMechwarriorTray_Init
    {

        static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD)
        {
            if (__instance == null || Combat == null || HUD == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDMechwarriorTray_Init - entered.");

            if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return; // Nothing to do



        }
    }

    //[HarmonyPatch(typeof(CombatHUDRetreatEscMenu), "OnCombatGameDestroyed")]
    //static class CombatHUDRetreatEscMenu_OnCombatGameDestroyed
    //{
    //    static void Postfix(CombatHUDRetreatEscMenu __instance)
    //    {
    //        Mod.Log.Trace?.Log("==== CombatHUDRetreatEscMenu_OnCombatGameDestroyed - entered.");
    //    }
    //}


    [HarmonyPatch(typeof(CombatHUDMechwarriorTray), "InitAbilityButtons")]
    static class CombatHUDMechwarriorTray_InitAbilityButtons
    {
        static bool IsInitialized = false;

        static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor)
        {
            if (__instance == null) return; // nothing to do
            if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return; // Nothing to do

            if (IsInitialized) return;

            //Mod.Log.Trace?.Log("==== CombatHUDMechwarriorTray_InitAbilityButtons - entered.");

            //// Looking for: UIRoot / uixPrfPanl_HUD(Clone) / Representation / BottomHUD_LayoutGroup / MechWarriorTray
            //Mod.Log.Debug?.Log("CREATING CombatHUDRetreatEscMenu - new layout group");
            //GameObject UIRootGO = __instance.gameObject.transform.parent.parent.parent.parent.gameObject;
            //Mod.Log.Debug?.Log($"UIRootGO is null? {UIRootGO == null}  name: {UIRootGO?.name}");
            //GameObject EscMenuGO = UIRootGO.FindFirstChildNamed("uixPrfPanl_COM_RetreatEscMenu(Clone)");
            //Mod.Log.Debug?.Log($"EscMenuGO is null? {EscMenuGO == null}  name: {EscMenuGO?.name}");
            //GameObject layoutGroupGO = EscMenuGO.FindFirstChildNamed("LayoutGroup");
            //Mod.Log.Debug?.Log($"layoutGroupGO is null? {layoutGroupGO == null}  name: {layoutGroupGO?.name}");
            //GameObject newLayoutGroupGO = UnityEngine.Object.Instantiate(layoutGroupGO, layoutGroupGO.transform.parent);
            //newLayoutGroupGO.transform.localPosition = new Vector3(-120, -80, 0);

            //// Remove the buttons that were added during the clone operation
            ////   - RetreatButton, EscButton, MenuButton, HelpButton
            //foreach (Transform child in newLayoutGroupGO.transform)
            //{
            //    child.gameObject.SetActive(false);
            //}

            //// Create the root GO to hang everything under
            //Mod.Log.Debug?.Log("CREATING creating new GO");
            //GameObject udsRootGO = new()
            //{
            //    name = "UDS_DROPSHIP_ROOT"
            //};
            //udsRootGO.transform.parent = newLayoutGroupGO.transform;
            //udsRootGO.transform.position = newLayoutGroupGO.transform.position;
            //udsRootGO.transform.localPosition = Vector3.zero;

            //UIHelper.BuildDropshipActionButtons(udsRootGO, __instance.MoveButton.gameObject, __instance.Combat, __instance.HUD, actor);
            ////UIHelper.BuildDropshipCommandButtons(udsRootGO, __instance.CommandButton.gameObject, __instance.Combat, __instance.HUD, actor);

            //IsInitialized = true;

            // UIRoot / uixPrfPanl_HUD(Clone) / Representation / BottomHUD_LayoutGroup / MechWarriorTray /
            //     mwt_ActionButtonsLayout / ActionTray2 / actionButton_Holder2 / uixPrfBttn_actionButton-MANAGED

            //Mod.Log.Debug?.Log("ADDED RectTransform");
            //RectTransform button1_RT = newButtonGO.AddComponent<RectTransform>();
            //button1_RT.sizeDelta = new Vector2(48, 48);
            //Mod.Log.Debug?.Log("ADDED CHUDActionButton");
            //CombatHUDActionButton button1_CHUDAB = newButtonGO.AddComponent<CombatHUDActionButton>();
            ////button1_CHUDAB.Icon = new SVGImage();
            //button1_CHUDAB.Init(__instance.Combat, __instance.HUD, BTInput.Instance.Combat_CommandAbility());

            //string abilityId = "AbilityDef_ActiveProbe_Ping";
            //bool had_key = __instance.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            //Mod.Log.Debug?.Log($"ABILITY_DEF: {abilityId} was found: {had_key}");

            //Ability ability = new Ability(abilityDef);
            //Mod.Log.Debug?.Log($"Hydrated ability: {ability.Def.Description.Id}  name: {ability.Def.Description.Name}  " +
            //    $"selectionType: {ability.Def.Targeting}  icon: {ability.Def.AbilityIcon.name}  ");
            ////button1_CHUDAB.InitButton(SelectionType.CommandBase, ability, abilityDef.AbilityIcon, "05198c11-52aa-4bdc-af44-3f62c8c8ff7a", "TEST TEST", null);

            //SVGAsset rawIcon = __instance.Combat.DataManager.SVGCache.GetAsset(ability.Def.AbilityIcon.name);
            //Mod.Log.Debug?.Log($"rawIcon: {rawIcon.name}  materials: {rawIcon.materials}  scale: {rawIcon.scale}");
            //SVGAsset loadedIcon = ability.Def.AbilityIcon;
            //Mod.Log.Debug?.Log($"loadIcon: {loadedIcon.name}  materials: {loadedIcon.materials}  scale: {loadedIcon.scale}");

            //// JANK FIX
            //button1_CHUDAB.Icon = new SVGImage();
            //button1_CHUDAB.gameObject.SetActive(true);

            //button1_CHUDAB.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false),
            //    ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, null);
            ////CommandButton.InitButton(GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false), ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, null);
            //Mod.Log.Debug?.Log($"Initialized button");

        }
    }
}

using BattleTech.UI;
using BestHTTP.SignalR.Hubs;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches.UI
{

    [HarmonyPatch(typeof(CombatHUDRetreatEscMenu), "Init")]
    static class CombatHUDRetreatEscMenu_Init
    {
        static void Postfix(CombatHUDRetreatEscMenu __instance, CombatGameState Combat, CombatHUD HUD)
        {
            if (__instance == null || Combat == null || HUD == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDRetreatEscMenu_Init - entered.");

            if (__instance.Combat == null || __instance.Combat.ActiveContract == null) return; // nothing to do
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


    [HarmonyPatch(typeof(CombatHUDRetreatEscMenu), "Update")]
    static class CombatHUDRetreatEscMenu_Update
    {
        //static GameObject newLayoutGroupGO = null;

        static void Postfix(CombatHUDRetreatEscMenu __instance)
        {
            if (__instance == null || __instance.isArena) return; // nothing to do
            
            //Mod.Log.Trace?.Log("==== CombatHUDRetreatEscMenu_Update - entered.");

            //if (newLayoutGroupGO == null)
            //{
            //    Mod.Log.Debug?.Log("CREATING CombatHUDRetreatEscMenu - new layout group");
            //    GameObject layoutGroupGO = __instance.gameObject.FindFirstChildNamed("LayoutGroup");
            //    newLayoutGroupGO = UnityEngine.Object.Instantiate(layoutGroupGO, layoutGroupGO.transform.parent);
            //    newLayoutGroupGO.transform.position = new Vector3(
            //        newLayoutGroupGO.transform.position.x,
            //        newLayoutGroupGO.transform.position.y - 80,
            //        newLayoutGroupGO.transform.position.z);

            //    Mod.Log.Debug?.Log("CREATING creating new GO");
            //    GameObject newButtonGO = new GameObject();
            //    newButtonGO.name = "UDS_COMMAND_ABIL_1";
            //    newButtonGO.transform.parent = newLayoutGroupGO.transform;
            //    Mod.Log.Debug?.Log("ADDED RectTransform");
            //    RectTransform button1_RT = newButtonGO.AddComponent<RectTransform>();
            //    button1_RT.sizeDelta = new Vector2(48, 48);
            //    Mod.Log.Debug?.Log("ADDED CHUDActionButton");
            //    CombatHUDActionButton button1_CHUDAB = newButtonGO.AddComponent<CombatHUDActionButton>();
            //    button1_CHUDAB.Icon = new SVGImage();
            //    button1_CHUDAB.Init(__instance.Combat, __instance.HUD, BTInput.Instance.Combat_CommandAbility());

            //    string abilityId = "AbilityDef_ActiveProbe_Ping";
            //    bool had_key = __instance.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            //    Mod.Log.Debug?.Log($"ABILITY_DEF: {abilityId} was found: {had_key}");

            //    Ability ability = new Ability(abilityDef);
            //    Mod.Log.Debug?.Log($"Hydrated ability: {ability.Def.Description.Id}  name: {ability.Def.Description.Name}  " +
            //        $"selectionType: {ability.Def.Targeting}  icon: {ability.Def.AbilityIcon.name}  ");
            //    //button1_CHUDAB.InitButton(SelectionType.CommandBase, ability, abilityDef.AbilityIcon, "05198c11-52aa-4bdc-af44-3f62c8c8ff7a", "TEST TEST", null);
            //    button1_CHUDAB.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false),
            //        ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, null);
            //    //CommandButton.InitButton(GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false), ability, ability.Def.AbilityIcon, ability.Def.Description.Id, ability.Def.Description.Name, null);
            //    Mod.Log.Debug?.Log($"Initialized button");

            //}
        }
    }
}

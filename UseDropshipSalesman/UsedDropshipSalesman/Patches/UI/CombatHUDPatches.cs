using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SignalR.Hubs;
using HBS.Extensions;
using MonoMod.Core.Utils;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UsedDropshipSalesman.Helper;
using UsedDropshipSalesman.UI;

namespace UsedDropshipSalesman.Patches.UI
{

    [HarmonyPatch(typeof(CombatHUD), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    static class CombatHUD_Init
    {
        static void Postfix(CombatHUD __instance, CombatGameState Combat)
        {
            Mod.Log.Trace?.Log("==== CombatHUD_Init:POSTFIX- entered.");

            if (__instance == null) return; // nothing to do
            if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return; // Nothing to do
            if (ModState.UDSCombatFrame != null) return; // already created

            // Looking for: UIRoot / uixPrfPanl_HUD(Clone) / Representation / BottomHUD_LayoutGroup / MechWarriorTray
            //GameObject EscMenuGO = __instance.RetreatEscMenu.gameObject;
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

            // Root needs RectTransform, HorizontalLayoutGroup, ContentSizeFilter

            // ROOT OFF PHASE TRACKER
            // uixPrfPanl_phaseTrack(Clone) / Representation / turnIndicators / playerTurn / playerT_BG (1)

            // Create the root GO to hang everything under
            Mod.Log.Trace?.Log("CREATING UDS_DROPSHIP_BTN_ROOT");
            GameObject udsRootGO = new("UDS_DROPSHIP_BTN_ROOT");
            udsRootGO.transform.parent = __instance.PhaseTrack.transform;
            udsRootGO.SetActive(false);
            UDSDropshipCombatFrame combatFrame = udsRootGO.AddComponent<UDSDropshipCombatFrame>();
            combatFrame.Init(Combat, __instance, __instance.PhaseTrack);
        }
    }

    [HarmonyPatch(typeof(CombatHUD), "OnCombatGameDestroyed")]
    [HarmonyPatch(new Type[] { })]
    static class CombatHUD_OnCombatGameDestroyed
    {
        static void Postfix(CombatHUD __instance)
        {
            Mod.Log.Trace?.Log("==== CombatHUD_OnCombatGameDestroyed:POSTFIX- entered.");
            if (__instance == null) return; // nothing to do
            if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return; // Nothing to do
            if (ModState.UDSCombatFrame == null) return; // already created

            ModState.UDSCombatFrame.OnCombatGameDestroyed();
        }
    }
}

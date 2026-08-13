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
            if (ModState.UDSButtonTray != null) return; // already created

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
            Mod.Log.Debug?.Log("CREATING creating new GO");
            GameObject udsRootGO = new()
            {
                name = "UDS_DROPSHIP_BTN_ROOT"
            };
            udsRootGO.transform.parent = __instance.PhaseTrack.transform;
            udsRootGO.transform.parent.position = __instance.PhaseTrack.transform.position;
            udsRootGO.transform.localPosition = new Vector3(-550, -50, 0);
            udsRootGO.SetActive(false);

            GameObject trayLabelGO = new()
            {
                name = "UDS_DROPSHIP_BTN_LABEL_ROW"
            };
            trayLabelGO.transform.parent = udsRootGO.transform;
            trayLabelGO.transform.position = udsRootGO.transform.position;

            // Build the label + background image. Must be first to allow text to overlay
            Mod.Log.Trace?.Log("Creating UDS_DROPSHIP_BTN_LABEL_TEXT");
            GameObject trayImagePrefab = __instance.PhaseTrack.gameObject.FindFirstChildNamed("playerT_BG (1)");
            Mod.Log.Trace?.Log($" trayImagePrefab == null {trayImagePrefab == null}");
            GameObject trayImageGO = UnityEngine.Object.Instantiate(trayImagePrefab, trayLabelGO.transform);
            trayImageGO.name = "UDS_DROPSHIP_BTN_LABEL_IMG";
            SVGImage labelImg = trayImageGO.GetComponent<SVGImage>(); // "uixSvgLine_hor3pt";
            labelImg.color = new Color(1f, 0.635f, 0, 1f);
            RectTransform rt2 = trayImageGO.GetComponent<RectTransform>();
            rt2.sizeDelta = new Vector2(220, 30);

            // Build the text next
            Mod.Log.Trace?.Log("Creating UDS_DROPSHIP_BTN_LABEL_TEXT");
            GameObject trayTextPrefab = __instance.PhaseTrack.gameObject.FindFirstChildNamed("playerT_Text (1)");
            Mod.Log.Trace?.Log($" trayTextPrefab == null {trayTextPrefab == null}");
            GameObject trayTextGO = UnityEngine.Object.Instantiate(trayTextPrefab, trayLabelGO.transform);
            trayTextGO.name = "UDS_DROPSHIP_BTN_LABEL_TEXT";
            LocalizableText lt = trayTextGO.GetComponent<LocalizableText>();
            lt.fontSize = 18;
            lt.text = "Dropship Command";
            
            // Build the tray for buttons
            GameObject buttonTrayGO = new()
            {
                name = "UDS_DROPSHIP_BTN_TRAY"
            };
            buttonTrayGO.transform.parent = udsRootGO.transform;
            buttonTrayGO.transform.position = udsRootGO.transform.position + new Vector3(0, -60, 0);

            RectTransform rt = buttonTrayGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(128, 128);
            HorizontalLayoutGroup hlg = buttonTrayGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            ContentSizeFitter csf = buttonTrayGO.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ModState.UDSButtonTray = buttonTrayGO.AddComponent<UDSButtonTray>();
            ModState.UDSButtonTray.Init(Combat, __instance);
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
            if (ModState.UDSButtonTray == null) return; // already created

            ModState.UDSButtonTray.OnCombatGameDestroyed();
        }
    }
}

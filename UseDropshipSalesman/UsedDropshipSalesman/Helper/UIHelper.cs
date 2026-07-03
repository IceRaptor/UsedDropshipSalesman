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

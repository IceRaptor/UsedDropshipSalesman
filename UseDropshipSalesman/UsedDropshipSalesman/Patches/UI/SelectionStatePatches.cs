using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(SelectionStateCommandTargetSinglePoint), "ProcessLeftClick")]
    static class SelectionStateCommandTargetSinglePoint_ProcessLeftClick
    {
        static void Prefix(SelectionStateCommandTargetSinglePoint __instance, ref bool __result, Vector3 worldPos)
        {
            Mod.Log.Trace?.Log("==== SelectionStateCommandTargetSinglePoint_ProcessLeftClick:PREFIX- entered.");

            Mod.Log.Debug?.Log($"SSCT-SinglePoint:ProcessLeftClick invoked with worldPos: {worldPos}  numPositionsLocked: {__instance.NumPositionsLocked}");
        }
    }

    [HarmonyPatch(typeof(SelectionStateCommandTargetSinglePoint), "ProcessPressedButton")]
    static class SelectionStateCommandTargetSinglePoint_ProcessPressedButton
    {
        static bool ActivateAbility = false;
        static void Prefix(SelectionStateCommandTargetSinglePoint __instance, ref bool __result, ref string button)
        {
            Mod.Log.Trace?.Log("==== SelectionStateCommandTargetSinglePoint_ProcessPressedButton:PREFIX- entered.");

            Mod.Log.Debug?.Log($"SSCT-SinglePoint:ProcessPressedButton invoked with button: {button}  numPositionsLocked: {__instance.NumPositionsLocked}");

            if (__instance.FromButton.Ability != null && 
                !String.IsNullOrEmpty(__instance.FromButton.Ability?.Def?.Description?.Id) &&
                __instance.FromButton.Ability.Def.Description.Id.Contains("_UDS_", StringComparison.InvariantCultureIgnoreCase))
            {
                // UDS button, go while
                Mod.Log.Debug?.Log($"OVERRIDING BUTTON NAME");
                button = "BTN_UDS_CONFIRM";
                ActivateAbility = true;
            }
            else
            {
                ActivateAbility = false;
            }
        }

        static void Postfix(SelectionStateCommandTargetSinglePoint __instance, string button)
        {
            Mod.Log.Trace?.Log("==== SelectionStateCommandTargetSinglePoint_ProcessPressedButton:POSTFIX - entered.");

            Mod.Log.Debug?.Log($"SSCT-SinglePoint:ProcessPressedButton invoked with button: {button}  numPositionsLocked: {__instance.NumPositionsLocked}");

            if (ActivateAbility)
            {
                ActivateAbility = false;
                Mod.Log.Debug?.Log($"Firing Command Ability");
                __instance.HideFireButton(cancelTorsoTwist: false);
                __instance.FromButton.ActivateCommandAbility(__instance.Combat.LocalPlayerTeam.GUID, __instance.targetPosition);
                __instance.isCommandComplete = true;
                __instance.OnInactivate();
            }
        }
    }


    //[HarmonyPatch(typeof(CombatSelectionHandler), "ProcessMousePos")]
    //static class CombatSelectionHandler_ProcessMousePos
    //{
    //    static void Prefix(CombatSelectionHandler __instance, Vector3 worldPos)
    //    {
    //        Mod.Log.Trace?.Log("==== CombatSelectionHandler_ProcessMousePos:POSTFIX- entered.");

    //        Mod.Log.Debug?.Log($"CSH:ProcessMousePos invoked with worldPos: {worldPos}");
    //    }
    //}

    //[HarmonyPatch(typeof(SelectionStateCommandTargetSinglePoint), "ProcessMousePos")]
    //static class SelectionStateCommandTargetSinglePoint_ProcessMousePos
    //{
    //    static void Prefix(SelectionStateCommandTargetSinglePoint __instance, Vector3 worldPos)
    //    {
    //        Mod.Log.Trace?.Log("==== SelectionStateCommandTargetSinglePoint_ProcessMousePos:POSTFIX- entered.");

    //        Mod.Log.Debug?.Log($"SSCT-SinglePoint:ProcessMousePos invoked with worldPos: {worldPos}  " +
    //            $"numPositionsLocked: {__instance.NumPositionsLocked}  hasActivated: {__instance.hasActivated}");
    //    }
    //}


    //[HarmonyPatch(typeof(CombatTargetingReticle), "UpdateReticle")]
    //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool)})]
    //static class CombatTargetingReticle_UpdateReticle
    //{
    //    static void Prefix(CombatTargetingReticle __instance, Vector3 positionA, float radius, bool useThumperReticle)
    //    {
    //        Mod.Log.Trace?.Log("==== CombatTargetingReticle_UpdateReticle:POSTFIX- entered.");

    //        Mod.Log.Debug?.Log($"UPDATING RETICLE with positionA: {positionA}  radius: {radius}  useThumberRectile: {useThumperReticle}\n\n");
    //    }
    //}

    //[HarmonyPatch(typeof(CombatTargetingReticle), "UpdateReticle")]
    //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3), typeof(float), typeof(bool) })]
    //static class CombatTargetingReticle_UpdateReticle2
    //{
    //    static void Postfix(CombatTargetingReticle __instance, Vector3 positionA, Vector3 positionB, float radius, bool useThumperReticle)
    //    {
    //        Mod.Log.Trace?.Log("==== CombatTargetingReticle_UpdateReticle:POSTFIX- entered.");

    //        Mod.Log.Debug?.Log($"UPDATING RETICLE with positionA: {positionA}  positionB: {positionB}  radius: {radius}  useThumberRectile: {useThumperReticle}\n");
    //    }
    //}

}

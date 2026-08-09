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

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateAbility")]
    [HarmonyPatch(new Type[] { })]
    static class CombatHUDActionButton_ActivateAbility
    {
        static void Prefix(CombatHUDActionButton __instance)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateAbility - entered.");

            Mod.Log.Debug?.Log($"Activating ability with id: {__instance.Ability?.Def?.Id}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
    static class CombatHUDActionButton_ActivateAbility2
    {
        static void Prefix(CombatHUDActionButton __instance, string creatorGUID, string targetGUID)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateAbility - entered.");

            Mod.Log.Debug?.Log($"Activating ability with id: {__instance.Ability?.Def?.Id}  creatorGUID: {creatorGUID}  targetGUID: {targetGUID}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    static class CombatHUDActionButton_ActivateCommandAbility
    {
        static void Prefix(CombatHUDActionButton __instance, string teamGUID)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateCommandAbility - entered.");

            Mod.Log.Debug?.Log($"Activating CommandAbility with id: {__instance.Ability?.Def?.Id} for team: {teamGUID}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(ICombatant) })]
    static class CombatHUDActionButton_ActivateCommandAbility_2
    {
        static void Prefix(CombatHUDActionButton __instance, string teamGUID, ICombatant target)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateCommandAbility - entered.");

            Mod.Log.Debug?.Log($"Activating CommandAbility with id: {__instance.Ability?.Def?.Id} for team: {teamGUID} vs. target: {target?.DisplayName}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateCommandAbility_3
    {
        static void Prefix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateCommandAbility - entered.");

            Mod.Log.Debug?.Log($"Activating CommandAbility with id: {__instance.Ability?.Def?.Id} for team: {teamGUID} vs. targetPos: {targetPosition}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateCommandAbility_4
    {
        static void Prefix(CombatHUDActionButton __instance, string teamGUID, Vector3 positionA, Vector3 positionB)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateCommandAbility - entered.");

            Mod.Log.Debug?.Log($"Activating CommandAbility with id: {__instance.Ability?.Def?.Id} for team: {teamGUID} vs. posA: {positionA}  posB: {positionB}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
    static class CombatHUDActionButton_ActivateCommandAbility_5
    {
        static void Prefix(CombatHUDActionButton __instance, string teamGUID, string targetGUID)
        {
            if (__instance == null) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ActivateCommandAbility - entered.");

            Mod.Log.Debug?.Log($"Activating CommandAbility with id: {__instance.Ability?.Def?.Id} for team: {teamGUID} vs. targetGUID: {targetGUID}");
        }
    }
}

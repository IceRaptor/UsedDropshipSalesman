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

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateSpecialAbility
    {
        static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateSpecialAbility:POSTFIX- entered.");
            Mod.Log.Debug?.Log($"ActivateCommandAbility for teamGUID: {teamGUID}  targetPos: {targetPosition}");

            if (!String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id) && __instance.Ability.Def.Description.Id.Contains("_UDS_"))
            {
                MessageCenterMessage messageCenterMessage = new AbilityInvokedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, targetPosition, Vector3.zero)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                messageCenterMessage = new AbilityConfirmedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, targetPosition, Vector3.zero)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                __instance.DisableButton();
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateSpecialAbility_2
    {
        static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 positionA, Vector3 positionB)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateSpecialAbility:POSTFIX- entered.");
            Mod.Log.Debug?.Log($"ActivateCommandAbility for teamGUID: {teamGUID}  positionA: {positionA}  positionB: {positionB}");

            if (!String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id) && __instance.Ability.Def.Description.Id.Contains("_UDS_"))
            {
                MessageCenterMessage messageCenterMessage = new AbilityInvokedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, positionA, positionB)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                messageCenterMessage = new AbilityConfirmedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, positionA, positionB)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                __instance.DisableButton();
            }
        }
    }
}

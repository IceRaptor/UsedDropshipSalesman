using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using CustAmmoCategories;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using Steamworks;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{

    [HarmonyPatch(typeof(Team), "OnAbilityInvoked")]
    static class Team_OnAbilityInvoked
    {
        static void Prefix(Team __instance, MessageCenterMessage message)
        {
            if (__instance == null || !__instance.IsLocalPlayer) return; // Nohting to do

            Mod.Log.Trace?.Log("==== Team_OnAbilityInvoked:PREFIX- entered.");

            AbilityMessage msg = message as AbilityMessage;
            if (msg.actingObjectGuid == __instance.GUID)
            {
                Mod.Log.Debug?.Log($"NEED TO ACTIVATE TEAM ABILITY: {msg.abilityID}");
            }
        }
    }

    [HarmonyPatch(typeof(Team), "ActivateAbility")]
    static class Team_ActivateAbility
    {
        static void Postfix(Team __instance, AbilityMessage msg)
        {
            Mod.Log.Trace?.Log("==== Team_ActivateAbility:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"Team_ActivateAbility with msg: {msg.MessageType}  abilityID: {msg.abilityID}  positionA: {msg.positionA}  positionB: {msg.positionB}");

            // Cleanup the add from CombatHUDActionButton.ActivateCommandAbility patch
            if (ModState.ActivatedTeamAbility != null && __instance.CommandAbilities.Contains(ModState.ActivatedTeamAbility))
            {
                Ability ability = ModState.ActivatedTeamAbility;
                Mod.Log.Debug?.Log($"Removing ability: {ability.Def.Description.Id} from tracking state.");
                Mod.Log.Debug?.Log($"Ability: {ability.Def.Description.Id}" +
                    $"  CurrentCooldown: {ability.CurrentCooldown} < 1" +
                    $"  def.NumberOfUses: {ability.Def.NumberOfUses} < 1" +
                    $"  NumUsesLeft: {ability.NumUsesLeft} > 0" +
                    $"  parentComponent == null ? {ability.parentComponent == null}" +
                    $"  def.ActivationCooldown: {ability.Def.ActivationCooldown}"
                    );
                __instance.CommandAbilities.Remove(ModState.ActivatedTeamAbility);
            }
        }

    }

    [HarmonyPatch(typeof(Team), "SetupTeamAbilities")]
    static class Team_SetupTeamAbilities
    {
        static void Postfix(Team __instance)
        {
            Mod.Log.Trace?.Log("==== Team_SetupTeamAbilities:POSTFIX- entered.");

        }

    }



}

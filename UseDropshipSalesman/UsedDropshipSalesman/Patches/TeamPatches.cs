using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using CustAmmoCategories;
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

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(Team), "ActivateAbility")]
    static class Team_ActivateAbility
    {
        static void Postfix(Team __instance, AbilityMessage msg)
        {
            Mod.Log.Trace?.Log("==== Team_ActivateAbility:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"Team_ActivateAbility with msg: {msg.MessageType}  abilityID: {msg.abilityID}  positionA: {msg.positionA}  positionB: {msg.positionB}");

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

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

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(AbstractActor), "ActivateAbility")]
    static class AbstractActor_ActivateAbility
    {
        static void Postfix(AbstractActor __instance, AbstractActor pilotedActor, string abilityName, string targetGUID, Vector3 posA, Vector3 posB)
        {
            Mod.Log.Trace?.Log("==== AbstractActor_ActivateAbility:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"ActivateAbility for pilotedActor: {pilotedActor?.DisplayName} using ability: {abilityName} vs target: {targetGUID}  at posA: {posA} posB: {posB}");

        }

    }

    [HarmonyPatch(typeof(Pilot), "ActivateAbility")]
    static class Pilot_ActivateAbility
    {
        static void Postfix(Pilot __instance, AbstractActor pilotedActor, string abilityName, string targetGUID)
        {
            Mod.Log.Trace?.Log("==== Pilot_ActivateAbility:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"ActivateAbility for pilotedActor: {pilotedActor?.DisplayName} using ability: {abilityName} vs target: {targetGUID}");

        }

    }
}

using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;
using UsedDropshipSalesman.Sequence;

namespace UsedDropshipSalesman.Patches
{

    [HarmonyPatch(typeof(Ability), "ActivateSpecialAbility")]
    [HarmonyPatch(new Type[] { typeof(Team), typeof(Vector3)})]
    static class Ability_ActivateSpecialAbility
    {
        static void Prefix(Ability __instance, Team team, Vector3 targetPos)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateSpecialAbility:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"ActivateArtilleryStrike for team: {team}  targetPos: {targetPos}");
        }
    }

    // We only patch the team effect, not the actor effect.
    [HarmonyPatch(typeof(Ability), "ActivateArtilleryStrike")]
    [HarmonyPatch(new Type[] { typeof(Team), typeof(Vector3), typeof(float) })]
    static class Ability_ActivateArtilleryStrike
    {
        static void Prefix(Ability __instance, ref bool __runOriginal,  Team team, Vector3 targetPos, float radius)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateArtilleryStrike:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"ActivateArtilleryStrike for team: {team}  targetPos: {targetPos}  radius: {radius}  " +
                $"Combat == null: {__instance.Combat == null}  StackManager == null: {__instance.Combat?.StackManager == null}.");
            __instance.Combat = team.Combat;

            if (!String.IsNullOrEmpty(__instance?.Def?.Description?.Id) && __instance.Def.Description.Id.Contains("_UDS_"))
            {
                __runOriginal = false;

                Turret turret = SpawnHelper.CreateTurretForSequence(__instance.Combat, __instance.Def.ActorResource, __instance.Def.WeaponResource);
                UDSArtillerySequence eventSequence = new(__instance.Combat, team.GUID, turret, __instance.Def.StringParam2, targetPos, radius);
                TurnEvent tEvent = new(GUIDFactory.GetGUID(), __instance.Combat, __instance.Def.ActivationETA, null, eventSequence, __instance.Def, showInPhaseTrack: true);
                __instance.Combat.TurnDirector.AddTurnEvent(tEvent);
                
                if (__instance.Def.IntParam1 > 0)
                {
                    SpawnHelper.SpawnFlares(__instance.Combat, __instance.Def, targetPos, targetPos, __instance.Def.StringParam1, __instance.Def.IntParam1, __instance.Def.ActivationETA);
                }

            }

        }
    }

    [HarmonyPatch(typeof(Ability), "ActivateStrafe")]
    [HarmonyPatch(new Type[] {typeof(Team), typeof(Vector3), typeof(Vector3), typeof(float)})]
    static class Ability_ActivateStrafe
    {
        static void Prefix(Ability __instance, ref bool __runOriginal, Team team, Vector3 positionA, Vector3 positionB, float radius)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateStrafe:POSTFIX- entered.");

            Mod.Log.Debug?.Log($"Ability_ActivateStrafe for team: {team}  positionA: {positionA}  positionB: {positionB}  radius: {radius}  " +
                $"Combat == null: {__instance.Combat == null}");
            __instance.Combat = team.Combat;

            if (!String.IsNullOrEmpty(__instance?.Def?.Description?.Id) && __instance.Def.Description.Id.Contains("_UDS_"))
            {
                __runOriginal = false;

                Vehicle strafingUnit = SpawnHelper.CreateVehicleForSequence(__instance.Combat, __instance.Def.ActorResource, __instance.Def.WeaponResource);
                StrafeSequence eventSequence = new(strafingUnit, positionA, positionB, radius);
                TurnEvent tEvent = new(GUIDFactory.GetGUID(), __instance.Combat, __instance.Def.ActivationETA, null, eventSequence, __instance.Def, showInPhaseTrack: true);
                __instance.Combat.TurnDirector.AddTurnEvent(tEvent);

                if (__instance.Def.IntParam1 > 0)
                {
                    SpawnHelper.SpawnFlares(__instance.Combat, __instance.Def, positionA, positionB, __instance.Def.StringParam1, __instance.Def.IntParam1, __instance.Def.ActivationETA);
                }
            }
        }
    }

}

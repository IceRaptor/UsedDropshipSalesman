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
    [HarmonyPatch(typeof(TurnDirector), "OnInitializeContractComplete")]
    public static class TurnDirector_OnInitializeContractComplete
    {
        static void Postfix(TurnDirector __instance)
        {
            Mod.Log.Trace?.Log("==== TurnDirector_OnInitializeContractComplete:POSTFIX- entered.");

            // Get the local player team
            Team team = __instance.Combat.localPlayerTeam;

            List<string> vehiclesToLoad = new();
            List<string> weaponsToLoad = new();
            List<string> abilities = new() { "AbilityDefCMD_UDS_Strafe", "AbilityDefCMD_UDS_ActiveProbe_Ping", "AbilityDefCMD_UDS_ArtThumperHE", "AbilityDefCMD_UDS_ArtThumperAP" };
            foreach (string abilityId in abilities)
            {
                // Add the def to the command options
                bool had_key = __instance.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
                Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");
                team.CommandAbilities.Add(new(abilityDef));

                if (!String.IsNullOrEmpty(abilityDef.ActorResource) && !weaponsToLoad.Contains(abilityDef.ActorResource))
                {
                    Mod.Log.Debug?.Log($"Loading actorResource: {abilityDef.ActorResource}' for ability: {abilityId}");
                    vehiclesToLoad.Add(abilityDef.ActorResource);
                }

                if (!String.IsNullOrEmpty(abilityDef.WeaponResource) && !weaponsToLoad.Contains(abilityDef.WeaponResource))
                {
                    Mod.Log.Debug?.Log($"Loading weaponResource: {abilityDef.WeaponResource}' for ability: {abilityId}");
                    weaponsToLoad.Add(abilityDef.WeaponResource);
                }
            }

            if (vehiclesToLoad.Count > 0 || weaponsToLoad.Count > 0)
            {
                DataloadHelper.LoadSupportResources(team, new List<string>(), vehiclesToLoad, new List<string>(), weaponsToLoad);
            }

        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnEncounterBegin")]
    public static class TurnDirector_OnEncounterBegin
    {
        static void Postfix(TurnDirector __instance)
        {
            Mod.Log.Trace?.Log("==== TurnDirector_OnEncounterBegin:POSTFIX- entered.");

            // Get the local player team
            Team team = __instance.Combat.localPlayerTeam;

            // Assume everything is loaded already
            List<string> abilityVehicles = new();
            List<string> abilityWeapons = new();
            List<string> abilities = new() { "AbilityDefCMD_UDS_Strafe", "AbilityDefCMD_UDS_ActiveProbe_Ping", "AbilityDefCMD_UDS_ArtThumperHE", "AbilityDefCMD_UDS_ArtThumperAP" };
            foreach (string abilityId in abilities)
            {
                // Add the def to the command options
                bool had_key = __instance.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);

                if (!String.IsNullOrEmpty(abilityDef.ActorResource) && !abilityWeapons.Contains(abilityDef.ActorResource))
                {
                    Mod.Log.Debug?.Log($"Loading actorResource: {abilityDef.ActorResource}' for ability: {abilityId}");
                    abilityVehicles.Add(abilityDef.ActorResource);
                }

                if (!String.IsNullOrEmpty(abilityDef.WeaponResource) && !abilityWeapons.Contains(abilityDef.WeaponResource))
                {
                    Mod.Log.Debug?.Log($"Loading weaponResource: {abilityDef.WeaponResource}' for ability: {abilityId}");
                    abilityWeapons.Add(abilityDef.WeaponResource);
                }
            }

            if (abilityVehicles.Count > 0 || abilityWeapons.Count > 0)
            {

                Lance supportLance = SpawnHelper.CreateAmbushLance(team);

                // Create vehicle actors and attach them as support to the team
                try
                {
                    foreach (string defId in abilityVehicles)
                    {
                        SpawnHelper.CreateVehicleSupportResource(team, supportLance, defId);
                    }
                }
                catch (Exception ex)
                {
                    Mod.Log.Error?.Log("Failed to create support vehicles!", ex);
                }

                try
                {
                    foreach (string defId in abilityWeapons)
                    {
                        SpawnHelper.CreateWeaponSupportResource(team, supportLance, defId);
                    }
                }
                catch (Exception ex)
                {
                    Mod.Log.Error?.Log("Failed to create support weapons!", ex);
                }
            }

        }
    }
}

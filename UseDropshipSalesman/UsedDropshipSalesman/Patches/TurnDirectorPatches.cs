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

            List<string> mechsToLoad = new();
            List<string> turretsToLoad = new();
            List<string> vehiclesToLoad = new();
            List<string> pilotsToLoad = new();
            List<string> abilities = new() { "AbilityDefCMD_UDS_Strafe", "AbilityDefCMD_UDS_ActiveProbe_Ping", "AbilityDefCMD_UDS_ArtThumperHE", "AbilityDefCMD_UDS_ArtThumperAP" };
            foreach (string abilityId in abilities)
            {
                // Add the def to the command options
                bool had_key = __instance.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
                Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");

                if (!String.IsNullOrEmpty(abilityDef.ActorResource))
                {
                    Mod.Log.Debug?.Log($"Loading actorResource: {abilityDef.ActorResource}' for ability: {abilityId}");
                    if (abilityDef.ActorResource.StartsWith("vehicleDef", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!vehiclesToLoad.Contains(abilityDef.ActorResource))
                        {
                            vehiclesToLoad.Add(abilityDef.ActorResource);
                        }
                    }
                    else if (abilityDef.ActorResource.StartsWith("turretDef", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!turretsToLoad.Contains(abilityDef.ActorResource))
                        {
                            turretsToLoad.Add(abilityDef.ActorResource);
                        }
                    }
                    else if (abilityDef.ActorResource.StartsWith("mechDef", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!mechsToLoad.Contains(abilityDef.ActorResource))
                        {
                            mechsToLoad.Add(abilityDef.ActorResource);
                        }
                    }
                }

                if (!String.IsNullOrEmpty(abilityDef.WeaponResource))
                {
                    if (!pilotsToLoad.Contains(abilityDef.WeaponResource))
                    {
                        pilotsToLoad.Add(abilityDef.WeaponResource);
                    }
                }

            }

            if (pilotsToLoad.Count > 0 || mechsToLoad.Count > 0 || vehiclesToLoad.Count > 0 || turretsToLoad.Count > 0)
            {
                DataloadHelper.LoadSupportResources(team, mechsToLoad, vehiclesToLoad, turretsToLoad, pilotsToLoad);
            }

        }

        [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
        public static class TurnDirector_OnCombatGameDestroyed
        {
            static void Postfix(TurnDirector __instance)
            {
                Mod.Log.Trace?.Log("==== TurnDirector_OnCombatGameDestroyed:POSTFIX- entered.");

                ModState.Reset(true);
            }
        }

    }
}

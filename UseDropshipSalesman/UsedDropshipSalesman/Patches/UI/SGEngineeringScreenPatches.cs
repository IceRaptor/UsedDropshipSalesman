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
    [HarmonyPatch(typeof(SGEngineeringScreen), "PopulateUpgradeDictionary")]
    static class SimGameState_PopulateUpgradeDictionary
    {
        static void Postfix(SGEngineeringScreen __instance)
        {
            Mod.Log.Trace?.Log("==== SimGameState_PopulateUpgradeDictionary:POSTFIX- entered.");

            Mod.Log.Debug?.Log("--- Ship upgrades: Purchased");
            foreach (var smu in __instance.PurchasedUpgrades)
            {
                Mod.Log.Debug?.Log($"  -- id: '{smu.Description.Id}'  name: '{smu.Description.Name}'  requires: [{smu.RequiredModules}]");
            }

            Mod.Log.Debug?.Log("--- Ship upgrades: Available");
            foreach (var smu in __instance.AvailableUpgrades)
            {
                Mod.Log.Debug?.Log($"  -- id: '{smu.Description.Id}'  name: '{smu.Description.Name}' requires: [{smu.RequiredModules}]");
            }

            Mod.Log.Debug?.Log("--- Ship upgrades: Unavailable");
            foreach (var smu in __instance.UnavailableUpgrades)
            {
                Mod.Log.Debug?.Log($"  -- id: '{smu.Description.Id}'  name: '{smu.Description.Name}' requires: [{smu.RequiredModules}]");
            }

            // Refresh argo upgrade colors
            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }
            EngineeringScreenUIHelper.RefreshUpgradeIcons(__instance, config);
        }

    }
}

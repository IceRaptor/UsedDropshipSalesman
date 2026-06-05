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

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(SGEngineeringScreen), "PopulateUpgradeDictionary")]
    static class SimGameState_PopulateUpgradeDictionary
    {
        static void Prefix(ref bool __runOriginal, SGEngineeringScreen __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_PopulateUpgradeDictionary - entered.");

            Mod.Log.Debug?.Write("--- Ship upgrades: Purchased");
            foreach (var smu in __instance.PurchasedUpgrades)
            {
                Mod.Log.Debug?.Write($"  -- id: {smu.Description.Id}  name: {smu.Description.Name}");
            }

            Mod.Log.Debug?.Write("--- Ship upgrades: Available");
            foreach (var smu in __instance.AvailableUpgrades)
            {
                Mod.Log.Debug?.Write($"  -- id: {smu.Description.Id}  name: {smu.Description.Name}");
            }

            Mod.Log.Debug?.Write("--- Ship upgrades: Unavailable");
            foreach (var smu in __instance.UnavailableUpgrades)
            {
                Mod.Log.Debug?.Write($"  -- id: {smu.Description.Id}  name: {smu.Description.Name}");
            }




            //List<string> list2 = new List<string>();
            //VersionManifestEntry[] allShipUpgrades = __instance.simState.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade);
            //foreach (VersionManifestEntry versionManifestEntry in allShipUpgrades)
            //{
            //    ShipModuleUpgrade shipModuleUpgrade = __instance.simState.DataManager.ShipUpgradeDefs.Get(versionManifestEntry.Id);
            //    if (__instance.simState.HasShipUpgrade(shipModuleUpgrade.Description.Id) || 
            //        __instance.simState.UpgradeInProgress(shipModuleUpgrade.Description.Id))
            //    {
            //        list2.Add(shipModuleUpgrade.Description.Id);
            //        __instance.PurchasedUpgrades.Add(shipModuleUpgrade);
            //    }
            //    else if (!__instance.simState.HasShipUpgrade(shipModuleUpgrade.Description.Id) && 
            //        __instance.simState.HasShipUpgrade(shipModuleUpgrade.RequiredModules))
            //    {
            //        list2.Add(shipModuleUpgrade.Description.Id);
            //        __instance.AvailableUpgrades.Add(shipModuleUpgrade);
            //    }
            //}
            //foreach (string key in __instance.simState.DataManager.ShipUpgradeDefs.Keys)
            //{
            //    ShipModuleUpgrade shipModuleUpgrade2 = __instance.simState.DataManager.ShipUpgradeDefs.Get(key);
            //    if (!list2.Contains(shipModuleUpgrade2.Description.Id) && __instance.simState.HasShipUpgrade(shipModuleUpgrade2.RequiredModules, list2))
            //    {
            //        __instance.UnavailableUpgrades.Add(shipModuleUpgrade2);
            //    }
            //}
        }
    }
}

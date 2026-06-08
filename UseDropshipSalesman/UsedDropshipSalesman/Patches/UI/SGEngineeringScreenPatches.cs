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
        }
    }
}

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

    [HarmonyPatch(typeof(CombatHUDIconTracker), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatHUD), typeof(TurnEvent)})]
    static class CombatHUDIconTracker_Init2
    {
        static void Prefix(CombatHUDIconTracker __instance, CombatHUD HUD, TurnEvent turnEvent)
        {
            if (__instance == null || HUD == null) return;

            // Fix issue where TurnEvents don't initialize the uiManager properly
            Mod.Log.Trace?.Log("==== CombatHUDIconTracker_Init(CombatHUD, TurnEvent) - entered.");
            if (__instance.uiManager == null)
            {
                __instance.uiManager = HUD.uiManager;
            }
                
        }
    }

   
}

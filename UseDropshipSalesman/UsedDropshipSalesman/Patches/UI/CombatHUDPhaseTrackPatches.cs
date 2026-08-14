using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SignalR.Hubs;
using HBS.Extensions;
using MonoMod.Core.Utils;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UsedDropshipSalesman.Helper;
using UsedDropshipSalesman.UI;

namespace UsedDropshipSalesman.Patches.UI
{

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnTurnEventAdded")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    static class CombatHUDPhaseTrack_OnTurnEventAdded
    {
        static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message)
        {
            Mod.Log.Trace?.Log("==== CombatHUD_OnCombatGameDestroyed:POSTFIX- entered.");
            if (__instance == null) return; // nothing to do
            if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return; // Nothing to do

        }
    }
}

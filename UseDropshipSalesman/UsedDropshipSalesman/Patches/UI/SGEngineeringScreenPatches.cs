using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(SGEngineeringScreen), "PopulateUpgradeDictionary")]
    static class SimGameState_PopulateUpgradeDictionary
    {
        static void Postfix(SGEngineeringScreen __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_PopulateUpgradeDictionary - entered.");
        }
    }
}

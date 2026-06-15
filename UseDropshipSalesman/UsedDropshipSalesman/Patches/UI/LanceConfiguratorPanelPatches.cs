using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
    static class LanceConfiguratorPanel_SetData
    {
        static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance, ref int maxUnits)
        {
            Mod.Log.Trace?.Write("==== LanceConfiguratorPanel_SetData::Prefix - entered.");

            Mod.Log.Debug?.Write($" maxUnits set to: {maxUnits}");
        }
        static void Postfix(LanceConfiguratorPanel __instance, ref int maxUnits)
        {
            Mod.Log.Trace?.Write("==== LanceConfiguratorPanel_SetData::Postfix- entered.");

            Mod.Log.Debug?.Write($" maxUnits set to: {maxUnits}");

            Mod.Log.Debug?.Write($"  Contract: {__instance.activeContract.Name}  " +
                $"contract_id: {__instance.activeContract.internalName}" +
                $"override_ID: {__instance.activeContract.Override.ID}");
            Mod.Log.Debug?.Write($"  Contract maxUnits: {__instance.activeContract?.Override?.maxNumberOfPlayerUnits}");

        }

    }
}

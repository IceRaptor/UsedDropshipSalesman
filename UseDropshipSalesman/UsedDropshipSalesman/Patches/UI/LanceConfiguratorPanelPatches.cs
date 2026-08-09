using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
    static class LanceConfiguratorPanel_SetData
    {
        static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance, ref int maxUnits, BattleTech.Contract contract)
        {
            Mod.Log.Trace?.Log("==== LanceConfiguratorPanel_SetData::Prefix - entered.");

            if (contract == null) return; // nothing to do

            Mod.Log.Debug?.Log($"  Contract: {contract.Name}  " +
                $"contract_id: {contract.internalName}  " +
                $"override_ID: {contract.Override.ID}  " +
                $"IsFlashpointContract: {contract.IsFlashpointContract}  " +
                $"IsFlashpointCampaignContract: {contract.IsFlashpointCampaignContract}  " +
                $"maxNumberOfPlayerUnits: {contract?.Override?.maxNumberOfPlayerUnits}" );
        }

        static void Postfix(LanceConfiguratorPanel __instance, ref int maxUnits, BattleTech.Contract contract)
        {
            Mod.Log.Trace?.Log("==== LanceConfiguratorPanel_SetData::Postfix- entered.");

            if (contract == null) return; // nothing to do
            Mod.Log.Debug?.Log($"Initial lanceConfigState:  maxUnits: {__instance.maxUnits}  lanceMinTonnage: {__instance.lanceMinTonnage}  " +
                $"lanceMaxTonnage: {__instance.lanceMaxTonnage}");

            // Only modify contracts without innate limits. See above
            if (contract.Override.lanceMaxTonnage == -1)
            {
                var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
                Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
                if (config == null)
                {
                    Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                    return;
                }

                __instance.lanceMaxTonnage = config.CustomDropship.DropBays.MaxTonnage;
            }

            Mod.Log.Debug?.Log($"Final lanceConfigState:  maxUnits: {__instance.maxUnits}  lanceMinTonnage: {__instance.lanceMinTonnage}  " +
                $"lanceMaxTonnage: {__instance.lanceMaxTonnage}");
        }

    }
}

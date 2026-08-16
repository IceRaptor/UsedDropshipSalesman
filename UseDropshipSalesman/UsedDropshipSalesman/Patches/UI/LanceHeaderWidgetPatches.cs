using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(LanceHeaderWidget), "RefreshLanceInfo")]
    static class LanceHeaderWidget_RefreshLanceInfo
    {

        static void Postfix(LanceHeaderWidget __instance, List<MechDef> mechs)
        {
            Mod.Log.Trace?.Log("==== LanceHeaderWidget_RefreshLanceInfo::Postfix- entered.");

            if (__instance == null || !__instance.LC.IsSimGame) return; // nothing to do

            int lanceTonnageRating = SimGameBattleSimulator.GetLanceTonnageRating(__instance.LC.sim, mechs, out float combinedTonnage);

            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            RectTransform rectTransform = __instance.simLanceTonnageText.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 56);

            var additionalTonnage = __instance.LC.Sim.CompanyStats.GetValue<int>(ModConsts.STAT_ADDITIONAL_DROP_TONNAGE);
            var currentTonnageMax = config.CustomDropship.DropBays.BaseTonnage + additionalTonnage;
            if (currentTonnageMax > config.CustomDropship.DropBays.MaxTonnage)
            {
                currentTonnageMax = config.CustomDropship.DropBays.MaxTonnage;
            }
            Mod.Log.Debug?.Log($"CurrentTonnageMax: {currentTonnageMax}  " +
                $"baseTonnage: {config.CustomDropship.DropBays.BaseTonnage} + additionalTonnage: {additionalTonnage}");

            __instance.simLanceTonnageText.SetText("{0} of {1} TONS", (int)combinedTonnage, currentTonnageMax);
        }
    }
}

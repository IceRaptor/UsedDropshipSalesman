using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{



    [HarmonyPatch(typeof(SGNavigationList), "RefreshButtonStates")]
    static class SGNavigationList_RefreshButtonStates
    {
        static void Postfix(SimGameState simState, SGNavigationList __instance)
        {
            Mod.Log.Trace?.Write("==== SGNavigationList_RefreshButtonStates - entered.");

            if (simState == null || __instance.argoButton == null) return;
            if (simState.CompanyStats == null || !simState.CompanyStats.ContainsStatistic(ModConsts.STAT_CURRENT_DROPSHIP) || Mod.ModSaveState == null) return;

            //var currentDropshipId = simState.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            var currentDropshipId = Mod.ModSaveState.CurrentDropshipId;
            Mod.Log.Debug?.Write($"Current dropship is: '{currentDropshipId}'");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Write($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            Mod.Log.Debug?.Write($"Argo button currently set to: {__instance.argoButton?.Text?.text}, setting label to: {config.CustomDropship.Description.Name}");
            __instance.argoButton.text.SetText(config.CustomDropship.Description.Name);

        }
    }

}

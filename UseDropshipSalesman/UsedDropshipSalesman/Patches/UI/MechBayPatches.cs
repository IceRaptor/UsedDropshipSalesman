using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(MechBayPanel), "ViewBays")]
    [HarmonyAfter("io.mission.customunits")]
    public static class MechBayPanel_ViewBays
    {
        public static void Postfix(MechBayPanel __instance)
        {
            Mod.Log.Trace?.Log("==== MechBayPanel_ViewBays - entered.");

            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Log.Info?.Log($"Current dropship is: '{currentDropshipId}', updating hanger config.");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            UIHelper.UpdateHangerConfig(config, __instance.sim);
        }
    }
}

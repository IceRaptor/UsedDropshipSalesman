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
            Mod.Log.Trace?.Write("==== MechBayPanel_ViewBays - entered.");

            var currentDropshipId = __instance.sim.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            Mod.Log.Info?.Write($"Current dropship is: '{currentDropshipId}', updating hanger config.");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Write($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            UIHelper.UpdateHangerConfig(config, __instance.sim);
        }
    }

    [HarmonyPatch(typeof(MechBayPanel), "ViewMechStorage")]
    [HarmonyAfter("io.mission.customunits")]
    public static class MechBayPanel_ViewMechStorage
    {
        public static void Postfix(MechBayPanel __instance)
        {
            Mod.Log.Trace?.Write("==== MechBayPanel_ViewMechStorage - entered.");

            //var currentDropshipId = __instance.sim.CompanyStats.GetValue<string>(ModConsts.STAT_CURRENT_DROPSHIP);
            //Mod.Log.Info?.Write($"Current dropship is: '{currentDropshipId}', updating hanger config.");
            //Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            //if (config == null)
            //{
            //    Mod.Log.Error?.Write($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
            //    return;
            //}

            //UIHelper.UpdateHangerConfig(config, __instance.sim);
        }
    }
}

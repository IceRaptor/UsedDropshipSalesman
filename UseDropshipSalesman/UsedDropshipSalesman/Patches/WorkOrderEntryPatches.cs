using GraphCoroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(WorkOrderEntry), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(WorkOrderType), typeof(string), typeof(string), typeof(int), typeof(string), typeof(int) })]
    static class WorkOrderEntry_Ctor
    {
        static void Postfix(WorkOrderEntry __instance)
        {
            if (__instance == null) return;
            if (!String.IsNullOrEmpty(__instance.Description) && __instance.Description.StartsWith("Argo Upgrade: "))
            {
                // TODO: LOCALIZE
                string upgradeName = __instance.Description.Substring(14);
                string newDesc = $"Ship Upgrade: {upgradeName}";
                Mod.Log.Info?.Log($"Updating description from: '{__instance.Description}' to: '{newDesc}'");
                __instance.Description = newDesc;
            }
        }
    }

    [HarmonyPatch(typeof(WorkOrderEntry), "Hydrate")]
    static class WorkOrderEntry_Hydrate
    {
        static void Postfix(WorkOrderEntry __instance)
        {
            if (__instance == null) return;
            if (!String.IsNullOrEmpty(__instance.Description) && __instance.Description.StartsWith("Argo Upgrade: "))
            {
                // TODO: LOCALIZE
                string upgradeName = __instance.Description.Substring(14);
                string newDesc = $"Ship Upgrade: {upgradeName}";
                Mod.Log.Info?.Log($"Updating description from: '{__instance.Description}' to: '{newDesc}'");
                __instance.Description = newDesc;

            }
        }
    }
}

using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(LoadingCurtain), "ShowUntil")]
    static class LoadingCurtain_ShowUntil
    {
        static void Prefix()
        {
            Mod.Log.Trace?.Log("==== LoadingCurtain_ShowUntil - entered.");
            if (LoadingCurtain.activeInstance == null) { return;  }

            if (LoadingCurtain.activeInstance.popupContainer != null)
            {
                Mod.Log.Debug?.Log($"fullScreenContainer GO is: {LoadingCurtain.activeInstance.fullScreenContainer.name}");
            }
            if (LoadingCurtain.activeInstance.popupContainer != null)
            {
                Mod.Log.Debug?.Log($"popupContainer GO is: {LoadingCurtain.activeInstance.popupContainer.name}");
            }

        }
    }
}

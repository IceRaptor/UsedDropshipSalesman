using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.Patches
{

    [HarmonyPatch(typeof(SGTravelManager), MethodType.Constructor, new Type[] { typeof(SimGameTravelStatus) })]
    static class SGTravelManager_Ctor_SimGameTravelStatus
    {
        static void Postfix(SimGameTravelStatus startState, SGTravelManager __instance)
        {
            Mod.Log.Trace?.Write("==== SGTravelManager_Ctor_SimGameTravelStatus - entered.");

            Mod.Log.Info?.Write($"Starting state is: {startState}");
            ModState.CurrentTravelStatus = startState;

            // Force the travel scenes to pause on transitions
            __instance.pauseAtTravelSteps = true;

            // Do not align from here, too early in the initiation chain
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "TransitionAnimating_OnEnter")]
    static class SGTravelManager_TransitionAnimating_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Write("==== SGTravelManager_TransitionAnimating_OnEnter - entered.");

            Mod.Log.Info?.Write($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
            DropshipHelper.AlignSpheriod(ModState.DropshipGO);
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "HandleNextTravelStep")]
    static class SGTravelManager_HandleNextTravelStep
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Write("==== SGTravelManager_HandleNextTravelStep - entered.");

            __instance.pauseAtTravelSteps = true;
        }
    }
}

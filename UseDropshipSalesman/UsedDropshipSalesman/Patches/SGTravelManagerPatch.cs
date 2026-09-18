using BattleTech.UI;
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
            Mod.Log.Trace?.Log("==== SGTravelManager_Ctor_SimGameTravelStatus - entered.");

            Mod.Log.Info?.Log($"Starting state is: {startState}");
            ModState.CurrentTravelStatus = startState;

            // Force the travel scenes to pause on transitions
            __instance.pauseAtTravelSteps = true;

        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "AtJumpPoint_OnEnter")]
    static class SGTravelManager_AtJumpPoint_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_AtJumpPoint_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "InSystem_OnEnter")]
    static class SGTravelManager_InSystem_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_InSystem_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "TransitionAnimating_OnEnter")]
    static class SGTravelManager_TransitionAnimating_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_TransitionAnimating_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "TransitFromJump_OnEnter")]
    static class SGTravelManager_TransitFromJump_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_TransitFromJump_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "TransitFromJump_OnExit")]
    static class SGTravelManager_TransitFromJump_OnExit
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_TransitFromJump_OnExit - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "TransitToJump_OnEnter")]
    static class SGTravelManager_TransitToJump_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_TransitToJump_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "Unknown_OnEnter")]
    static class SGTravelManager_Unknown_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_Unknown_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "WarmingEngines_OnEnter")]
    static class SGTravelManager_WarmingEngines_OnEnter
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_WarmingEngines_OnEnter - entered.");

            Mod.Log.Info?.Log($"Transitioning from animation: {__instance.PreTransitionState} to: {__instance.PostTransitionState}");
            ModState.CurrentTravelStatus = __instance.PostTransitionState;
        }
    }

    [HarmonyPatch(typeof(SGTravelManager), "HandleNextTravelStep")]
    static class SGTravelManager_HandleNextTravelStep
    {
        static void Postfix(SGTravelManager __instance)
        {
            Mod.Log.Trace?.Log("==== SGTravelManager_HandleNextTravelStep - entered.");

            __instance.pauseAtTravelSteps = true;
        }
    }
}

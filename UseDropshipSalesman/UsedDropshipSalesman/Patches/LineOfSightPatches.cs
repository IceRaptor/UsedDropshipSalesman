using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static HoudiniEngineUnity.HEU_MaterialData;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(LineOfSight), "GetVisibilityToTargetWithPositionsAndRotations")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(ICombatant), typeof(Vector3), typeof(Quaternion)})]
    static class LineOfSight_GetVisibilityToTargetWithPositionsAndRotations
    {
        static void Postfix(ref VisibilityLevel __result, AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation)
        {
            Mod.Log.Trace?.Log($"LineOfSight_GetVisibilityToTargetWithPositionsAndRotations::Postfix");

            if (target is AbstractActor actor)
            {
                if (actor.IsForcedVisible)
                {
                    Mod.Log.Debug?.Log($"FORCING VIS FOR TARGET: {actor.DisplayName} TO FULL");
                    __result = VisibilityLevel.LOSFull;
                    return;
                }
            }
        }
    }
}

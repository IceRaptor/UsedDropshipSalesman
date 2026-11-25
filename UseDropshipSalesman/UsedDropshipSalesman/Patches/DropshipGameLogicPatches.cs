using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches
{
    [HarmonyPatch(typeof(DropshipGameLogic.SavedDropshipFlybySpawn), "Spawn")]
    [HarmonyPatch(new Type[] { typeof(Transform), typeof(bool), typeof(bool), typeof(DropshipAnimationStyle) })]
    static class DropshipGameLogic_SavedDropshipFlybySpawn_Spawn
    {
        static void Postfix(DropshipGameLogic.SavedDropshipFlybySpawn __instance, DropshipGameLogic __result)
        {
            Mod.Log.Trace?.Log("==== DropshipGameLogic_SavedDropshipFlybySpawn_Spawn::POSTFIX");
            
            Mod.Log.Info?.Log("ATTEMPTING TO REPLACE COMBAT VIEW");

        }
    }
}

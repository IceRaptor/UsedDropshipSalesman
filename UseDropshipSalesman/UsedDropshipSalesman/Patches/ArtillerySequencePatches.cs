using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches
{
    //internal class ArtillerySequencePatches
    //{
    //    [HarmonyPatch(typeof(ArtillerySequence), "PlaySoundEffect")]
    //    [HarmonyPatch(new Type[] { })]
    //    static class ArtillerySequence_PlaySoundEffect
    //    {
    //        static void Prefix(ArtillerySequence __instance)
    //        {
    //            if (__instance == null) return;

    //            // Fix a bug in vanilla artillery sequence, that calls playSoundEffect before this is declared
    //            if (__instance.osd == null)
    //            {
    //                __instance.osd = new ObjectSpawnData(__instance.ExplodeFX, __instance.TargetPos, Quaternion.identity, playFX: true, autoPoolObject: true);
    //            }
    //        }
    //    }
    //}
}

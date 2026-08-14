using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Patches
{
    internal class GameRepresentation
    {
        [HarmonyPatch(typeof(BattleTech.GameRepresentation), "Update")]
        [HarmonyPatch(new Type[] { })]
        static class GameRepresentation_Update
        {
            static void Prefix(BattleTech.GameRepresentation __instance)
            {
                if (__instance == null) return;

                //Mod.Log.Trace?.Log("==== GameRepresentation_Update - entered.");

                //if (__instance._parentActor != null && __instance._parentActor.IsTeleportedOffScreen && EncounterLayerParent.encounterBegan)
                //{
                //    Mod.Log.Trace?.Log($"Will cause safety teleport for actor: {__instance._parentActor?.DisplayName}  " +
                //        $"with spawnerGUID: {__instance._parentActor?.spawnerGUID}");
                //}
            }
        }
    }
}

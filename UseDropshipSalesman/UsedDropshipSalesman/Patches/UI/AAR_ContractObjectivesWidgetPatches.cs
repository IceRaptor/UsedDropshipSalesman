using BattleTech.Framework;
using BattleTech.UI;
using GraphCoroutines;
using System;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(AAR_ContractObjectivesWidget), "FillInObjectives")]
    [HarmonyAfter("ca.jwolf.DropCostsEnhanced", "us.frostraptor.humanresources")]
    public static class AAR_ContractObjectivesWidget_FillInObjectives
    {
        public static void Postfix(AAR_ContractObjectivesWidget __instance)
        {
            Mod.Log.Trace?.Log("==== AAR_ContractObjectivesWidget_FillInObjectives - entered.");

            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            if (config.CustomDropship.Costs.Drop != 0)
            {
                string formattedCosts = string.Format("{0:n0}", config.CustomDropship.Costs.Drop);
                Mod.Log.Info?.Log($"Dropship with id: {currentDropshipId} has drop cost of: {config.CustomDropship.Costs.Drop}, formatted as: {formattedCosts}");
                // TODO: Localize
                string objectiveText = "Dropship Landing Fees";
                string missionObjectiveResultString = $"{objectiveText}: ¢{formattedCosts}";
                string uuid = "0544e6e7-b237-4ceb-8591-1d039efa2438"; // hardcoded 
                MissionObjectiveResult missionObjectiveResult = 
                    new MissionObjectiveResult(missionObjectiveResultString, uuid, false, true, ObjectiveStatus.Ignored, false);
                __instance.AddObjective(missionObjectiveResult);

            }
        }
    }
}

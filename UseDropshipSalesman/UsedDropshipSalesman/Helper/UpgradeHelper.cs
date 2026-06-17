using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using CustomUnits.CustomHangars;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomUnits.CustomHangars.CustomHangarHelper;

namespace UsedDropshipSalesman.Helper
{

    public static class UpgradeHelper
    {
        // Invoke CU lance control APIs to fixate drop sizes
        public static void UpdateDropConfig(DropshipConfig config)
        {
            int totalUnits = 0;
            List<List<string>> layout = new List<List<string>>();
            foreach (var slot in config.CustomDropship.DropBays.Slots)
            {
                totalUnits += slot.Length;
                layout.Add(slot.ToList());
            }

            var labels = config.CustomDropship.DropBays.Labels.ToList();
            Mod.Log.Info?.Write($"Updating CU dropConfig to support {totalUnits} across {layout.Count} lances.");
            CustomLanceHelper.PushDropLayout(config.CustomDropship.Description.Id, layout, totalUnits, labels);
        }

        public static void UpdateHangarConfig(DropshipConfig config)
        {
            Mod.Log.Info?.Write($"Updating CU hangarConfig to support hangars: ");
            foreach (KeyValuePair<string, int> kvp in config.CustomDropship.HangarBays)
            {
                Mod.Log.Info?.Write($" -- bay: {kvp.Key}  value: {kvp.Value}");
            }

            Dictionary<string, CustomHangarConstraint> constraints;
            constraints = config.CustomDropship.HangarBays.ToDictionary(x => x.Key, y => new CustomHangarConstraint() { MaxAvailableUnits= y.Value });

            CustomHangarHelper.SetConstraints(constraints, Mod.LogName);
        }
    }

}

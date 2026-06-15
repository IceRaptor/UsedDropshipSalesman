using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomUnits;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Helper
{

    public static class UpgradeHelper
    {
        // Invoke CU lance control APIs to fixate drop sizes
        public static void UpdateDropConfig(DropshipConfig config)
        {
            int totalUnits = 0;
            List<List<string>> layout = new List<List<string>>();
            foreach (var slot in config.DropBays.Slots)
            {
                totalUnits += slot.Length;
                layout.Add(slot.ToList());
            }

            var labels = config.DropBays.Labels.ToList();
            Mod.Log.Info?.Write($"Updating CU dropConfig to support {totalUnits} across {layout.Count} lances.");
            CustomLanceHelper.PushDropLayout(config.Label, layout, totalUnits, labels);
        }

        public static void UpdateHangarConfig(DropshipConfig config)
        {
            // TODO: HACK FOR TESTING
            ModState.SimGameSpaceController.sim.companyStats.Set<int>(ModState.SimGameSpaceController.sim.Constants.Story.MechBayPodsID, 3);
        }
    }

}

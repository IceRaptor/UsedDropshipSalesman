using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman
{
    public class ModText
    {
        public Dictionary<string, string> UpgradeDescs_Abilities;
        public Dictionary<string, string> UpgradeDescs_Stats;

        internal void InitUnset()
        {
            if (UpgradeDescs_Abilities == null || UpgradeDescs_Abilities.Count == 0)
            {
                Mod.Log.Debug?.Log("Populating default values for UpgradeDescs_Abilities");
                UpgradeDescs_Abilities = new()
                {
                    { "AbilityDefCMD_UDS_Art_Longtom_AP", "Grants an artillery strike from a Longtom artillery piece that will pierce armor" },
                    { "AbilityDefCMD_UDS_Art_Longtom_Cluster", "Grants an artillery strike from a Longtom artillery piece with many hits." },
                    { "AbilityDefCMD_UDS_Art_Longtom_HE", "Grants an artillery strike from a Longtom artillery piece." },

                    { "AbilityDefCMD_UDS_Art_Sniper_AP", "Grants an artillery strike from a Sniper artillery piece that will pierce armor" },
                    { "AbilityDefCMD_UDS_Art_Sniper_Cluster", "Grants an artillery strike from a Sniper artillery piece with many hits." },
                    { "AbilityDefCMD_UDS_Art_Sniper_HE", "Grants an artillery strike from a Sniper artillery piece." },

                    { "AbilityDefCMD_UDS_Art_Thumper_AP", "Grants an artillery strike from a Thumper artillery piece that will pierce armor" },
                    { "AbilityDefCMD_UDS_Art_Thumper_Cluster", "Grants an artillery strike from a Thumper artillery piece with many hits." },
                    { "AbilityDefCMD_UDS_Art_Thumper_HE", "Grants an artillery strike from a Thumper artillery piece." },

                    { "AbilityDefCMD_UDS_Strafe_Light", "Grants a strafing run from a light aerofighter" },
                    { "AbilityDefCMD_UDS_Strafe_Medium", "Grants a strafing run from a medium aerofighter" },
                    { "AbilityDefCMD_UDS_Strafe_Heavy", "Grants a strafing run from a heavy aerofighter" }
                };
            }

            if (UpgradeDescs_Stats == null || UpgradeDescs_Stats.Count == 0)
            {
                Mod.Log.Debug?.Log("Populating default values for UpgradeDescs_Stats");
                UpgradeDescs_Stats = new()
                {
                    { "UDS_COMBAT_BTN_1_ABILITYDEF_ID", "Command 1" },
                    { "UDS_COMBAT_BTN_2_ABILITYDEF_ID", "Command 2" },
                    { "UDS_COMBAT_BTN_3_ABILITYDEF_ID", "Command 3" },
                    { "UDS_COMBAT_BTN_4_ABILITYDEF_ID", "Command 4" }
                };
            }
        }
    }
}

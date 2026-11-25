using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman
{
    public class ModText
    {
        public const string LT_UI_COMBAT_COMMAND_BLOCK_LABEL = "COMBAT_COMMAND_BLOCK_LABEL";
        public const string LT_UI_WORK_ORDER_ENTRY_DESC = "WORK_ORDER_ENTRY_DESC";
        public const string LT_UI_UDS_UPGRADE_BLOCKED_REASON_SHIP_UPGRADE = "UPGRADE_BLOCKED_REASON_SHIP_UPGRADE";
        public const string LT_UI_UDS_UPGRADE_BLOCKED_REASON_HANGAR_LIMIT = "UPGRADE_BLOCKED_REASON_HANGAR_LIMIT";
        public const string LT_UI_UDS_UPGRADE_BLOCKED_REASON_BERTH_LIMIT = "UPGRADE_BLOCKED_REASON_BERTH_LIMIT";
        public const string LT_UI_UDS_UPGRADE_FAILED_PREAMBLE = "UPGRADE_FAILED_PREAMBLE";
        public const string LT_UI_UDS_UPGRADE_FAILED_DESC = "UPGRADE_FAILED_DESC";
        public const string LT_UI_UDS_UPGRADE_FAILED_LABEL = "UPGRADE_FAILED_LABEL";
        public const string LT_UI_AAR_LANDING_FEES = "AAR_LANDING_FEES_DESC";
        public const string LT_UI_CAPTAINS_QUARTERS_DROPSHIP_OPERATING_COSTS = "CAPTAINS_QUARTERS_DROPSHIP_OPERATING_COSTS";
        public const string LT_UI_CAPTAINS_QUARTERS_SHIP_UPGRADE_PREFIX = "CAPTAINS_QUARTERS_SHIP_UPGRADE_PREFIX";
        public Dictionary<string, string> UI_Elements;


        public Dictionary<string, string> UpgradeDescs_Abilities;


        // No need for consts because these are de-refernced by the stat name
        public Dictionary<string, string> UpgradeDescs_Stats;

        internal void InitUnset()
        {

            if (UI_Elements == null || UI_Elements.Count == 0)
            {
                Mod.Log.Debug?.Log("Populating default values for UpgradeDescs_Abilities");
                UI_Elements = new()
                {
                    { LT_UI_COMBAT_COMMAND_BLOCK_LABEL, "Dropship Command" },
                    { LT_UI_WORK_ORDER_ENTRY_DESC, "{0}" },
                    { LT_UI_UDS_UPGRADE_BLOCKED_REASON_SHIP_UPGRADE, "Pending ship upgrade" },
                    { LT_UI_UDS_UPGRADE_BLOCKED_REASON_HANGAR_LIMIT,  "New dropship hangar {0} limit is {1}, there are {2} units active or readying." },
                    { LT_UI_UDS_UPGRADE_BLOCKED_REASON_BERTH_LIMIT, "New dropship berth limit is {0}, but there are {1} active pilots." },
                    { LT_UI_UDS_UPGRADE_FAILED_PREAMBLE, "Your dropship cannot be changed until you resolve the following issues:\n"},
                    { LT_UI_UDS_UPGRADE_FAILED_DESC, "\n\nCancel active ship upgrades or readying mechs in the timeline view from the main screen. " +
                        "Store active units or cancel reading actions to reduce the hangar count to match the new dropship. " +
                        "Dismiss pilots that exceed the berth limit on the new dropship. " +
                        "\nDo not navigate away from the current star system until you resolve these actions. " +
                        "This dialog will repeat each day until the conditions are cleared.\n<b>You cannot cancel the dropship upgrade.</b>"},
                    { LT_UI_UDS_UPGRADE_FAILED_LABEL, "Dropship Upgrade Failed"},
                    { LT_UI_AAR_LANDING_FEES, "Dropship Landing Fees" },
                    { LT_UI_CAPTAINS_QUARTERS_DROPSHIP_OPERATING_COSTS, "{0} Upkeep Costs" },
                    { LT_UI_CAPTAINS_QUARTERS_SHIP_UPGRADE_PREFIX, "SHIPUPGRADE: {0}" },
                };
            }

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
                    { ModConsts.STAT_COMBAT_BTN_1_ABILITYDEF_ID, "Command 1" },
                    { ModConsts.STAT_COMBAT_BTN_2_ABILITYDEF_ID, "Command 2" },
                    { ModConsts.STAT_COMBAT_BTN_3_ABILITYDEF_ID, "Command 3" },
                    { ModConsts.STAT_COMBAT_BTN_4_ABILITYDEF_ID, "Command 4" }
                };
            }
        }
    }
}

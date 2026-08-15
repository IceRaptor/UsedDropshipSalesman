using System.Collections.Generic;
using UnityEngine;

namespace UsedDropshipSalesman
{

    internal static class ModConsts
    {
        public const string CUSTOM_RESOURCE_DROPSHIP_CONFIG = "CustomDropshipDef";

        public const string STAT_CURRENT_DROPSHIP = "UDS_CURRENT_DROPSHIP";
        public const string STAT_ADDITIONAL_DROP_TONNAGE = "UDS_ADDTIONAL_DROP_TONNAGE";
        public const string STAT_ADDITIONAL_HANGAR_BAYS = "UDS_ADDITIONAL_HANGAR_BAYS";
        public const string STAT_ADDITIONAL_BERTHS = "UDS_ADDITIONAL_BERTHS";

        public const string STAT_COMBAT_BTN_1_ABILITYDEF_ID = "UDS_COMBAT_BTN_1_ABILITYDEF_ID";
        public const string STAT_COMBAT_BTN_2_ABILITYDEF_ID = "UDS_COMBAT_BTN_2_ABILITYDEF_ID";
        public const string STAT_COMBAT_BTN_3_ABILITYDEF_ID = "UDS_COMBAT_BTN_3_ABILITYDEF_ID";
        public const string STAT_COMBAT_BTN_4_ABILITYDEF_ID = "UDS_COMBAT_BTN_4_ABILITYDEF_ID";

        public const string HBS_PREFAB_LEOPARD = "HBS_LEOPARD";
        public const string HBS_PREFAB_ARGO = "HBS_ARGO";
        public const string FALLBACK_DROPSHIP_ID = "argo";

        public const string DROPSHIP_GO_PREFIX_SIMGAME = "UDS_DROPSHIP_ROOT_SG_";
        public const string DROPSHIP_GO_PREFIX_BRIEFING = "UDS_DROPSHIP_ROOT_BRF_";
        public const string UPGRADE_PANEL_CATEGORY_PREFIX = "UDS_UPGRADE_PANEL_CATEGORY_";
        public const string UPGRADE_PANEL_SYSTEM_PREFIX = "UDS_UPGRADE_PANEL_SYSTEM_";
        public const string UPGRADE_PANEL_ITEM_PREFIX = "UDS_UPGRADE_PANEL_ITEM_";

        // These are set by the base game
        public static List<string> BASEGAME_DEFAULT_ARGO_UPGRADES = new() 
        {
            "argoUpgrade_drive0", "argoUpgrade_mechBay1", "argoUpgrade_medBay1", "argoUpgrade_pod1", "argoUpgrade_structure0"
        };

        public const int HBS_LEOPARD_PREFAB_LAYER = 20;

        public static Color UPGRADE_COLOR_DEFAULT_PURCHASED = new Color(0.253f, 1.0f, 0.0f, 0.5f);
        public static Color UPGRADE_COLOR_DEFAULT_PURCHASED_HOVER = new Color(0.253f, 1.0f, 0.0f, 1.0f);

        public static Color UPGRADE_COLOR_DEFAULT_AVAILABLE = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        public static Color UPGRADE_COLOR_DEFAULT_AVAILABLE_HOVER = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        public static Color UPGRADE_COLOR_DEFAULT_UNAVAILABLE = new Color(1.0f, 0.235f, 0.0f, 0.196f);
        public static Color UPGRADE_COLOR_DEFAULT_UNAVAILABLE_HOVER = new Color(1.0f, 0.235f, 0.0f, 0.5f);

        public static Color UPGRADE_COLOR_DEFAULT_INNATE = new Color(0.0f, 0.65f, 1.0f, 0.5f);
        public static Color UPGRADE_COLOR_DEFAULT_INNATE_HOVER = new Color(0.0f, 0.65f, 1.0f, 1.0f);

    }
}



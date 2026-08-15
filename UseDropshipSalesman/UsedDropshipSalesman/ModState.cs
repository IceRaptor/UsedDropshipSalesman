using BattleTech.Data;
using System.Collections.Generic;
using UnityEngine;
using UsedDropshipSalesman.Helper;
using UsedDropshipSalesman.UI;

namespace UsedDropshipSalesman
{

    internal static class ModState
    {

        // -- Travel State for alignment purposes
        internal static SimGameTravelStatus CurrentTravelStatus = new SimGameTravelStatus();
        internal static SimGameSpaceController SimGameSpaceController = null;
        internal static DataManagerUnityInstance DataManagerUnityInstance= null;

        // Visuals state
        internal static LeopardPrefabState SimGameLeopardState = null;
        internal static LeopardPrefabState BriefingLeopardState = null;

        // Upgrades state
        internal static bool HasCustomUpgradeScreen = false;

        // Combat state
        internal static Lance SeqSupportLance = null;
        internal static UDSDropshipCombatFrame UDSCombatFrame = null;
        internal static Ability ActivatedTeamAbility = null;

        internal static string CombatButton_1_AbilityDefId = null;
        internal static string CombatButton_2_AbilityDefId = null;
        internal static string CombatButton_3_AbilityDefId = null;
        internal static string CombatButton_4_AbilityDefId = null;

        internal static void Reset(bool afterCombat)
        {
            if (afterCombat)
            {
                SeqSupportLance = null;
                UDSCombatFrame = null;
                ActivatedTeamAbility = null;
                CombatButton_1_AbilityDefId = null;
                CombatButton_2_AbilityDefId = null;
                CombatButton_3_AbilityDefId = null;
                CombatButton_4_AbilityDefId = null;
            }
            else
            {
                // Reinitialize state
                CurrentTravelStatus = SimGameTravelStatus.IN_SYSTEM;
            }
        }

    }

}

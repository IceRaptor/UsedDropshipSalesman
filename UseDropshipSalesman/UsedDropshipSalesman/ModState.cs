using BattleTech.Data;
using System.Collections.Generic;
using UnityEngine;
using UsedDropshipSalesman.Helper;

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

        internal static void Reset()
        {
            
            // Reinitialize state
            CurrentTravelStatus = SimGameTravelStatus.IN_SYSTEM;
        }


    }

}

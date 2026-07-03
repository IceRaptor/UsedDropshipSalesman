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

        // Visuals state
        internal static SimGameLeopardState SGLeopardState = null;
        //internal static Dictionary<string, GameObject> DropshipPrefabs = new Dictionary<string, GameObject>();
        internal static Dictionary<string, GameObject> DropshipInstances = new Dictionary<string, GameObject>();

        // Upgrades state
        internal static bool HasCustomUpgradeScreen = false;

        internal static void Reset()
        {
            
            // Reinitialize state
            CurrentTravelStatus = SimGameTravelStatus.IN_SYSTEM;
        }


    }

}



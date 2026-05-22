using System.Collections.Generic;
using UnityEngine;

namespace UsedDropshipSalesman
{

    internal static class ModState
    {

        // -- Travel State for alignment purposes
        internal static SimGameTravelStatus CurrentTravelStatus = new SimGameTravelStatus();
        internal static GameObject DropshipGO = null;

        internal static void Reset()
        {
            // Reinitialize state
            CurrentTravelStatus = SimGameTravelStatus.IN_SYSTEM;
            DropshipGO = null;
        }

    }

}



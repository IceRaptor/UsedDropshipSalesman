using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Helper
{
    public static class AlignmentHelper
    {
        public static void AlignSpheriod(GameObject dropshipGO)
        {
            if (dropshipGO == null)
            {
                Mod.Log.Warn?.Write("Invoked without a gameobject!");
                return;
            }


            if (ModState.CurrentTravelStatus == SimGameTravelStatus.WARMING_ENGINES)
            {
                Mod.Log.Info?.Write("Aligning spheriod dropship docked to jumpship");
                // Align docked downward
                // Align towards direction of travel
                dropshipGO.gameObject.transform.localPosition = new Vector3(12.0f, 0.0f, 7.0f);
                dropshipGO.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                dropshipGO.gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            }
            else
            {
                Mod.Log.Info?.Write("Aligning spheriod dropship for travel");
                // Align towards direction of travel
                dropshipGO.gameObject.transform.localPosition = new Vector3(12.0f, 0.0f, 7.0f);
                dropshipGO.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                dropshipGO.gameObject.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
            }

        }

    }
}

using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UsedDropshipSalesman.Helper;
using UsedDropshipSalesman.Sequence;

namespace UsedDropshipSalesman.Patches.Data
{

    [HarmonyPatch(typeof(DataManagerExtensions), "GetStatDescDef")]
    static class DataManagerExtensions_GetStatDescDef
    {

        static void Prefix(DataManager dataManager, SimGameStat simGameStat, ref bool __runOriginal, ref SimGameStatDescDef __result)
        {
            Mod.Log.Trace?.Log($"==== DataManagerExtensions_GetStatDescDef:PRE- entered for stat: {(simGameStat.name)}");


            if (!String.IsNullOrEmpty(simGameStat.name) && simGameStat.name.StartsWith("UDS_"))
            {
                string resourceId = "SimGameStatDesc_" + simGameStat.name;
                if (!dataManager.Exists(BattleTechResourceType.SimGameStatDescDef, resourceId))
                {
                    // This normally causes an error, but we should be handling this elsewhere. SimGameStatDescDef cant support string values, which I'm trying to circumvent
                    __runOriginal = false;
                    __result = null;
                }

            }
        }

    }

}

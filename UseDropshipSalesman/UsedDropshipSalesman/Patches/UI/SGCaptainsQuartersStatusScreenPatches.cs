using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(SGCaptainsQuartersStatusScreen), "RefreshData")]
    static class SGCaptainsQuartersStatusScreen_RefreshData
    {
        static void Postfix(SGCaptainsQuartersStatusScreen __instance)
        {
            Mod.Log.Trace?.Log("==== SGCaptainsQuartersStatusScreen_RefreshData - entered.");

            var currentDropshipId = Mod.ModSaveData.CurrentDropshipId;
            Mod.Log.Info?.Log($"Current dropship is: '{currentDropshipId}', captain's quarter info.");
            Mod.Config.Dropships.TryGetValue(currentDropshipId, out DropshipConfig config);
            if (config == null)
            {
                Mod.Log.Error?.Log($"Cannot find dropship with id: {currentDropshipId} - this should not happen!");
                return;
            }

            // Walk the SectionOneExpensesList, replacing string values
            IEnumerator enumerator = __instance.SectionOneExpensesList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    Transform transform = (Transform)obj;
                    SGKeyValueView component = transform.gameObject.GetComponent<SGKeyValueView>();

                    Mod.Log.Trace?.Log($"SGCQSS:RD - Reading key from component:{component.name}.");
                    TextMeshProUGUI keyText = component.Key;
                    string key = keyText.text;
                    Mod.Log.Trace?.Log($"SGCQSS:RD - key found as: {key}");

                    if (String.Equals(key, "Argo Operating Costs", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: Localize
                        keyText.text = $"{config.CustomDropship.Description.Name} Upkeep Costs";
                        continue;
                    }
                    
                    foreach (ShipModuleUpgrade smu in __instance.simState.shipUpgrades)
                    {
                        if (String.Equals(smu?.Description?.Name, key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Found a ship module upgrade, prefix it
                            // TODO: Localize
                            keyText.text = "SHIPUPGRADE: " + key;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Mod.Log.Warning?.Log($"Failed to substitute key-value pairs on SGCaptainsQuartersStatusScreen: {e.Message}");
            }
        }
    }
}

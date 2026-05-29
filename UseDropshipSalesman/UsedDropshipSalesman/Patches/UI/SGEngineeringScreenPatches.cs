using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SocketIO;
using FluffyUnderware.DevTools;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Patches.UI
{
    [HarmonyPatch(typeof(SGEngineeringScreen), "PopulateUpgradeDictionary")]
    static class SimGameState_PopulateUpgradeDictionary
    {
        static void Prefix(ref bool __runOriginal, SGEngineeringScreen __instance)
        {
            Mod.Log.Trace?.Write("==== SimGameState_PopulateUpgradeDictionary - entered.");

            // Disable all the existing ones
            var upgradePanelRootGO = __instance.gameObject.FindFirstChildNamed("OBJ_upgradePanels");
            foreach (Transform childT in upgradePanelRootGO.transform)
            {
                childT.gameObject.SetActive(false);
            }
            var imageShipHoloGO = __instance.gameObject.FindFirstChildNamed("image_shipHologram");
            imageShipHoloGO.SetActive(false);

            // Create the category panel
            var categoryReferenceGO = __instance.gameObject.FindFirstChildNamed("uixPrbPanl_SystemsAndSupportPanel");
            
            var newCategoryPanelGO = UnityEngine.Object.Instantiate(categoryReferenceGO);
            newCategoryPanelGO.transform.parent = upgradePanelRootGO.transform;
            newCategoryPanelGO.transform.position = categoryReferenceGO.transform.position;
            newCategoryPanelGO.transform.rotation = categoryReferenceGO.transform.rotation;
            newCategoryPanelGO.name = "UDS_TEST_CATEGORY_PANEL";
            newCategoryPanelGO.SetActive(true);

            // Disable the connectorLine
            var categoryConnectorLineGO = newCategoryPanelGO.FindFirstChildNamed("connectorLine");
            categoryConnectorLineGO.SetActive(false);

            // Remove unnecessary systems
            GameObject categoryPanelHeaderGO = null;
            GameObject categoryBgAndDecoGO = null;
            GameObject systemReferenceGO = null;
            HashSet<string> namesToDisable = new HashSet<string>() { "StructureSystem", "DriveSystem", "HabitatSystem" };
            GameObject categoryPanelLayoutGO = newCategoryPanelGO.FindFirstChildNamed("systemsAndSupport-layout");
            foreach (Transform childT in categoryPanelLayoutGO.transform)
            {
                if (childT.gameObject.name.Equals("StructureSystem", StringComparison.InvariantCulture) ||
                    childT.gameObject.name.Equals("DriveSystem", StringComparison.InvariantCulture) ||
                    childT.gameObject.name.Equals("HabitatSystem", StringComparison.InvariantCulture))
                {
                    childT.gameObject.SetActive(false);
                }
                else if (childT.gameObject.name.Equals("PowerSystem", StringComparison.InvariantCulture))
                {
                    systemReferenceGO = childT.gameObject;
                }
                else if (childT.gameObject.name.Equals("bg-and-deco", StringComparison.InvariantCulture))
                {
                    categoryBgAndDecoGO = childT.gameObject;
                }
                else if (childT.gameObject.name.Equals("systemsAndSupport_header", StringComparison.InvariantCulture))
                {
                    categoryPanelHeaderGO = childT.gameObject;
                }
            }
            // Update the category text
            var categoryPanelHeaderTextComponent = categoryPanelHeaderGO.GetComponent<LocalizableText>();
            categoryPanelHeaderTextComponent.text = "New Category";

            // Find the catgegory icon
            var categoryPanelIconGO = newCategoryPanelGO.FindFirstChildNamed("icon");
            var categoryPanelIconSVGComponent = categoryPanelIconGO.GetComponent<SVGImage>();
            __instance.simState.RequestItem<SVGAsset>("lv_robber-mask", 
                delegate (SVGAsset asset) { categoryPanelIconSVGComponent.vectorGraphics = asset; }, 
                BattleTechResourceType.SVGAsset);

            // Grab the system 
            var systemReferenceTextGO = systemReferenceGO.FindFirstChildNamed("text_powerSystem");
            var systemReferenceTextComponent = systemReferenceTextGO.GetComponent<LocalizableText>();
            systemReferenceTextComponent.text = "New System";

            // Grab the upgradePip
            var upgradePipSlotsGO = newCategoryPanelGO.FindFirstChildNamed("powerPipSlots");
            // Disable the existing ones
            foreach (Transform childT in upgradePipSlotsGO.transform)
            {
                if (childT.name.StartsWith("uixPrfIndc_SIM_argo", StringComparison.InvariantCulture))
                {
                    childT.gameObject.SetActive(false);
                }
            }
            // Add new ones
            string systemId = "uixPrfIndc_SIM_argoUpgradePipUnavailable-element";
            //if (AvailableUpgrades.Contains(upgrade))
            //{
            //    systemId = "uixPrfIndc_SIM_argoUpgradePipAvailable-element";
            //}
            //else if (PurchasedUpgrades.Contains(upgrade))
            //{
            //    systemId = "uixPrfIndc_SIM_argoUpgradePip-element";
            //}
            ShipModuleUpgrade newUpgrade = new ShipModuleUpgrade();
            newUpgrade.Description = new DescriptionDef();
            newUpgrade.Description.Details = "This is a completely new upgrade";
            newUpgrade.Description.Name = "New Upgrade - N1";
            newUpgrade.Description.Icon = "lv_cyber-eye";
            SGEngineeringShipUpgradePip component = 
                __instance.uiManager.dataManager.
                PooledInstantiate(systemId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform)
                .GetComponent<SGEngineeringShipUpgradePip>();
            component.transform.localScale = Vector3.one;
            component.SetUpgadeModule(newUpgrade);
            __instance.simState.RequestItem<SVGAsset>(newUpgrade.Description.Icon, component.SetIcon, BattleTechResourceType.SVGAsset);
            //component.OnModuleSelected.RemoveAllListeners();
            //component.OnModuleSelected.AddListener(OnUpgradeSelected);

            ShipModuleUpgrade newUpgrade2 = new ShipModuleUpgrade();
            newUpgrade2.Description = new DescriptionDef();
            newUpgrade2.Description.Details = "This is a completely new upgrade";
            newUpgrade2.Description.Name = "New Upgrade - N2";
            newUpgrade2.Description.Icon = "lv_eye-shield";
            SGEngineeringShipUpgradePip component2 =
                __instance.uiManager.dataManager.
                PooledInstantiate(systemId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform)
                .GetComponent<SGEngineeringShipUpgradePip>();
            component2.transform.localScale = Vector3.one;
            component2.SetUpgadeModule(newUpgrade2);
            __instance.simState.RequestItem<SVGAsset>(newUpgrade2.Description.Icon, component2.SetIcon, BattleTechResourceType.SVGAsset);
            //component.OnModuleSelected.RemoveAllListeners();
            //component.OnModuleSelected.AddListener(OnUpgradeSelected);

            ShipModuleUpgrade newUpgrade3 = new ShipModuleUpgrade();
            newUpgrade3.Description = new DescriptionDef();
            newUpgrade3.Description.Details = "This is a completely new upgrade";
            newUpgrade3.Description.Name = "New Upgrade - N3";
            newUpgrade3.Description.Icon = "lv_radar-sweep";
            SGEngineeringShipUpgradePip component3 =
                __instance.uiManager.dataManager.
                PooledInstantiate(systemId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform)
                .GetComponent<SGEngineeringShipUpgradePip>();
            component3.transform.localScale = Vector3.one;
            component3.SetUpgadeModule(newUpgrade3);
            __instance.simState.RequestItem<SVGAsset>(newUpgrade3.Description.Icon, component3.SetIcon, BattleTechResourceType.SVGAsset);
            //component.OnModuleSelected.RemoveAllListeners();
            //component.OnModuleSelected.AddListener(OnUpgradeSelected);


            ShipModuleUpgrade newUpgrade4 = new ShipModuleUpgrade();
            newUpgrade4.Description = new DescriptionDef();
            newUpgrade4.Description.Details = "This is a completely new upgrade";
            newUpgrade4.Description.Name = "New Upgrade - N4";
            newUpgrade4.Description.Icon = "lv_target-laser";
            SGEngineeringShipUpgradePip component4 =
                __instance.uiManager.dataManager.
                PooledInstantiate(systemId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform)
                .GetComponent<SGEngineeringShipUpgradePip>();
            component4.transform.localScale = Vector3.one;
            component4.SetUpgadeModule(newUpgrade4);
            __instance.simState.RequestItem<SVGAsset>(newUpgrade4.Description.Icon, component4.SetIcon, BattleTechResourceType.SVGAsset);
            //component.OnModuleSelected.RemoveAllListeners();
            //component.OnModuleSelected.AddListener(OnUpgradeSelected);

        }
    }
}

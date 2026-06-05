using BattleTech.Data;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using DG.Tweening;
using HBS.Extensions;
using IRBTModUtils;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Mono.Options.OptionSet;

namespace UsedDropshipSalesman.Helper
{

    /*
     * Known icons
     * categories
     * =========
     * uixSvgIcon_shipUpgrade_typeSystemSupport
     * uixSvgIcon_shipUpgrade_typeCombat
     * uixSvgIcon_shipUpgrade_typePersonnel
     * uixSvgIcon_shipUpgrade_typeSystemSupport
     * 
     * items
     * =========
     * uixSvgIcon_shipUpgrade_power0, uixSvgIcon_shipUpgrade_power1, uixSvgIcon_shipUpgrade_power2
     * uixSvgIcon_shipUpgrade_structure0, uixSvgIcon_shipUpgrade_structure1, uixSvgIcon_shipUpgrade_structure2
     * uixSvgIcon_shipUpgrade_drive0, uixSvgIcon_shipUpgrade_drive1, uixSvgIcon_shipUpgrade_drive2
     * uixSvgIcon_shipUpgrade_podAlpha, uixSvgIcon_shipUpgrade_podBeta, uixSvgIcon_shipUpgrade_podGamma
     * uixSvgIcon_shipUpgrade_mechbay1, uixSvgIcon_shipUpgrade_mechbay2, uixSvgIcon_shipUpgrade_mechbay3
     * uixSvgIcon_shipUpgrade_repairscaffold
     * uixSvgIcon_shipUpgrade_automation
     * uixSvgIcon_shipUpgrade_automationUpgrade
     * uixSvgIcon_shipUpgrade_machineshop
     * uixSvgIcon_shipUpgrade_refitharness
     * uixSvgIcon_shipUpgrade_training1, uixSvgIcon_shipUpgrade_training2, uixSvgIcon_shipUpgrade_training3
     * uixSvgIcon_shipUpgrade_medbay1, uixSvgIcon_shipUpgrade_medbay2, uixSvgIcon_shipUpgrade_medbay3, uixSvgIcon_shipUpgrade_medbay4
     * uixSvgIcon_shipUpgrade_lounge, uixSvgIcon_shipUpgrade_loungeUpgrade
     * uixSvgIcon_shipUpgrade_arcade, uixSvgIcon_shipUpgrade_gym, uixSvgIcon_shipUpgrade_hydroponics
     * uixSvgIcon_shipUpgrade_library, uixSvgIcon_shipUpgrade_libraryUpgrade
     * uixSvgIcon_shipUpgrade_pool
     * uixSvgIcon_action_multitarget
     */

    public record DropshipUpgradeCategory
    {
        public string CategoryId;
        public string HeaderText;
        public string Icon;
        public List<DropshipUpgradeSystem> Systems;
    }

    public record DropshipUpgradeSystem
    {
        public string SystemId;
        public string HeaderText;
        public List<string> innateUpgrades;
        public List<string> optionalUpgrades;

        // Derived from argoUpgradeDefs
        public List<DropshipUpgradeItem> ItemUpgrades;

    }

    public record DropshipUpgradeItem
    {
        public string Name;
        public string Description;
        public string Icon;
        public bool isPurchased = false;
        public bool isAvailable = true;
    }


    public static class UpgradeUIHelper
    {
        private const int CATEGORY_Y_PADDING = 140;

        public static void ResetUpgradePanel(SGEngineeringScreen engineeringScreen)
        {
            // enable the argo hologram
            Mod.Log.Trace?.Write("Disabling Argo hologram");
            var imageShipHoloGO = engineeringScreen.gameObject.FindFirstChildNamed("image_shipHologram");
            imageShipHoloGO.SetActive(false);

            // Iterate OBJ_upgradePanels children; disabling any UDS ones
            Mod.Log.Trace?.Write("Disabling UDS panels, enabling defaults");
            var upgradePanelRootGO = engineeringScreen.gameObject.FindFirstChildNamed("OBJ_upgradePanels");
            foreach (Transform childT in upgradePanelRootGO.transform)
            {
                if (childT.gameObject.name.StartsWith(ModConsts.UPGRADE_PANEL_CATEGORY_PREFIX))
                {
                    childT.gameObject.SetActive(false);
                }
                else
                {
                    childT.gameObject.SetActive(true);
                }
            }
        }

        public static void RefreshUpgradeIcons(SGEngineeringScreen engineeringScreen)
        {
            // Iterate OBJ_upgradePanels children; disabling any UDS ones
            var upgradePanelRootGO = engineeringScreen.gameObject.FindFirstChildNamed("OBJ_upgradePanels");
            List<GameObject> customCategoryGO = new List<GameObject>();
            foreach (Transform childT in upgradePanelRootGO.transform)
            {
                if (childT.gameObject.name.StartsWith(ModConsts.UPGRADE_PANEL_CATEGORY_PREFIX))
                {
                    customCategoryGO.Add(childT.gameObject);
                }
            }

            foreach (GameObject categoryGO in customCategoryGO)
            {
                Mod.Log.Debug?.Write($"Processing icons for category: {categoryGO.name}");
                
                SGEngineeringShipUpgradePip[] upgradePipComps = categoryGO.transform.GetComponentsInChildren<SGEngineeringShipUpgradePip>();
                foreach (SGEngineeringShipUpgradePip pip in upgradePipComps)
                {
                    Mod.Log.Debug?.Write($" Refreshing pip for module.name: {pip.name}  desc.Name {pip.UpgradeModule?.Description?.Name}  desc.Id: {pip.UpgradeModule?.Description?.Id}");
                    if (engineeringScreen.PurchasedUpgrades.Contains(pip.UpgradeModule))
                    {
                        Mod.Log.Debug?.Write($" -- Module has been purchased");
                        // TODO: Find innate upgrades
                    }
                    else if (engineeringScreen.AvailableUpgrades.Contains(pip.UpgradeModule))
                    {
                        Mod.Log.Debug?.Write($" -- Module is available");
                    }
                    else
                    {
                        Mod.Log.Debug?.Write($" -- Module is unavailable");
                    }

                    // TODO: Innate upgrades should be automatically purchased - where to do?

                    // Find the actual pip
                    GameObject iconGO = pip.gameObject.FindFirstChildNamed("pip_ICON");
                    // Set the initial color
                    SVGImage icon = iconGO.GetComponent<SVGImage>();
                    icon.color = Color.green;

                    DOTweenAnimation[] anims = iconGO.GetComponents<DOTweenAnimation>();
                    anims[0].endValueColor = Color.yellow;
                    anims[1].endValueColor = Color.green;
                    anims[2].endValueColor = Color.green;

                }

                /*
            //  systemId = "uixPrfIndc_SIM_argoUpgradePipUnavailable-element";
            //  systemId = "uixPrfIndc_SIM_argoUpgradePipAvailable-element";
            //  systemId = "uixPrfIndc_SIM_argoUpgradePip-element";
            // TODO: Handle 'innate' states
            string systemToCloneId = "uixPrfIndc_SIM_argoUpgradePipAvailable-element";
            //if (item.isPurchased) { systemToCloneId = "uixPrfIndc_SIM_argoUpgradePip-element";  }
            //else if (!item.isAvailable) { systemToCloneId = "uixPrfIndc_SIM_argoUpgradePipUnavailable-element"; }

            SGEngineeringScreen engineeringScreen = ModState.SimGameSpaceController.sim.RoomManager.EngineeringRoom.engineeringScreen;
            DataManager dm = engineeringScreen.uiManager.dataManager;
            GameObject newUpgradeItemGO = dm.PooledInstantiate(systemToCloneId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform);
            newUpgradeItemGO.name = ModConsts.UPGRADE_PANEL_ITEM_PREFIX + upgradeDef.Description.Id;
                 */
            }


        }

        public static void OverlayCustomUpgrades(List<DropshipUpgradeCategory> categories, SGEngineeringScreen engineeringScreen)
        {
            // Disable all the existing ones
            Mod.Log.Trace?.Write("Disabling existing upgrade panel");
            var upgradePanelRootGO = engineeringScreen.gameObject.FindFirstChildNamed("OBJ_upgradePanels");
            foreach (Transform childT in upgradePanelRootGO.transform)
            {
                childT.gameObject.SetActive(false);
            }

            // Disable the argo hologram
            Mod.Log.Trace?.Write("Disabling Argo hologram");
            var imageShipHoloGO = engineeringScreen.gameObject.FindFirstChildNamed("image_shipHologram");
            imageShipHoloGO.SetActive(false);

            Mod.Log.Info?.Write($"Generating {categories.Count} upgrade categories.");
            var categoryReferenceGO = engineeringScreen.gameObject.FindFirstChildNamed("uixPrbPanl_SystemsAndSupportPanel");
            foreach (var (category, idx) in categories.Select((category, idx) => (category, idx)))
            {
                GameObject categoryGO = UpgradeUIHelper.BuildCategoryUpgradeGO(category, categoryReferenceGO, upgradePanelRootGO, idx);
            }

        }

        internal static GameObject BuildCategoryUpgradeGO(
            DropshipUpgradeCategory category, GameObject categoryReferenceGO, GameObject upgradePanelRootGO, int categoryIdx)
        {
            var newCategoryPanelGO = UnityEngine.Object.Instantiate(categoryReferenceGO);
            newCategoryPanelGO.transform.position = categoryReferenceGO.transform.position;
            newCategoryPanelGO.transform.rotation = categoryReferenceGO.transform.rotation;
            string panelID = ModConsts.UPGRADE_PANEL_CATEGORY_PREFIX + category.CategoryId;
            newCategoryPanelGO.name = panelID;
            Mod.Log.Trace?.Write($"Created new category panel with name: {panelID}");
            newCategoryPanelGO.transform.parent = upgradePanelRootGO.transform;
            newCategoryPanelGO.SetActive(true);
            categoryReferenceGO.SetActive(false);

            // Shift downwards for each iteration
            newCategoryPanelGO.transform.position -= new Vector3(0, categoryIdx * CATEGORY_Y_PADDING, 0);

            // Disable the connectorLine
            var categoryConnectorLineGO = newCategoryPanelGO.FindFirstChildNamed("connectorLine");
            categoryConnectorLineGO.SetActive(false);
            Mod.Log.Trace?.Write("Disabled connector line");

            // Remove unnecessary systems
            GameObject categoryPanelHeaderGO = null;
            GameObject categoryBgAndDecoGO = null;
            GameObject systemReferenceGO = null;
            HashSet<string> namesToDisable = new() { "StructureSystem", "DriveSystem", "HabitatSystem" };
            GameObject categoryPanelLayoutGO = newCategoryPanelGO.FindFirstChildNamed("systemsAndSupport-layout");
            Mod.Log.Trace?.Write("Iterating category children");
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
                    childT.gameObject.SetActive(false);
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
            Mod.Log.Trace?.Write($"Updating category text to: {category.HeaderText}");
            var categoryPanelHeaderTextComponent = categoryPanelHeaderGO.GetComponent<LocalizableText>();
            categoryPanelHeaderTextComponent.text = category.HeaderText;

            // Find the catgegory icon
            Mod.Log.Trace?.Write($"Updating category text to: {category.Icon}");
            var categoryPanelIconGO = newCategoryPanelGO.FindFirstChildNamed("icon");
            var categoryPanelIconSVGComponent = categoryPanelIconGO.GetComponent<SVGImage>();
            ModState.SimGameSpaceController.sim.RequestItem<SVGAsset>(category.Icon,
                delegate (SVGAsset asset) { categoryPanelIconSVGComponent.vectorGraphics = asset; },
                BattleTechResourceType.SVGAsset);

            // Create system upgrades
            Mod.Log.Trace?.Write("Iterating systems");
            foreach (DropshipUpgradeSystem system in category.Systems)
            {
                UpgradeUIHelper.BuildSystemUpgradeGO(system, systemReferenceGO);
            }

            return newCategoryPanelGO;
        }

        internal static void BuildSystemUpgradeGO(DropshipUpgradeSystem system, GameObject categoryPanelGO)
        {
            var systemPanelGO = UnityEngine.Object.Instantiate(categoryPanelGO);
            systemPanelGO.transform.position = categoryPanelGO.transform.position;
            systemPanelGO.transform.rotation = categoryPanelGO.transform.rotation;
            string panelID = ModConsts.UPGRADE_PANEL_SYSTEM_PREFIX + system.SystemId;
            systemPanelGO.name = panelID;
            Mod.Log.Trace?.Write($"Created new systems group with name: {system.SystemId}");
            systemPanelGO.transform.parent = categoryPanelGO.transform.parent;
            systemPanelGO.SetActive(true);

            var systemReferenceTextGO = systemPanelGO.FindFirstChildNamed("text_powerSystem");
            var systemReferenceTextComponent = systemReferenceTextGO.GetComponent<LocalizableText>();
            systemReferenceTextComponent.text = system.HeaderText;

            // Grab the upgradePip and disable existing pip slots
            Mod.Log.Trace?.Write("Disabling powerPipSlots");
            var upgradePipSlotsGO = systemPanelGO.FindFirstChildNamed("powerPipSlots");
            foreach (Transform childT in upgradePipSlotsGO.transform)
            {
                if (childT.name.StartsWith("uixPrfIndc_SIM_argo", StringComparison.InvariantCulture))
                {
                    childT.gameObject.SetActive(false);
                }
            }

            Mod.Log.Trace?.Write("Finding innate and optional ShipModuleUpgrade defs");
            Dictionary<string, ShipModuleUpgrade> dropshipModules = new();
            DataManager dataManager = ModState.SimGameSpaceController.sim.DataManager;
            VersionManifestEntry[] array = dataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade);
            foreach (VersionManifestEntry vme in array)
            {
                if (system.innateUpgrades.Contains<string>(vme.Id) || system.optionalUpgrades.Contains<string>(vme.Id))
                {
                    ShipModuleUpgrade shipModuleUpgrade = dataManager.ShipUpgradeDefs.Get(vme.Id);
                    dropshipModules.Add(vme.Id, shipModuleUpgrade);
                }
            }
            Mod.Log.Trace?.Write($"  DONE. Found {dropshipModules.Count} upgrades for this dropship.");

            // Create new upgrade items
            Mod.Log.Trace?.Write("Creating new innate upgrade items");
            foreach (string upgradeDefId in system.innateUpgrades)
            {
                bool exists = dropshipModules.TryGetValue(upgradeDefId, out ShipModuleUpgrade module);
                if (exists)
                {
                    UpgradeUIHelper.BuildUpgradeItemGO(module, upgradePipSlotsGO, true);
                }
                else
                {
                    Mod.Log.Warn?.Write($"Failed to fetch upgrade def: {upgradeDefId} from all known upgrades, skipping!");
                }                
            }

            Mod.Log.Trace?.Write("Creating new optional upgrade items");
            foreach (string upgradeDefId in system.optionalUpgrades)
            {
                bool exists = dropshipModules.TryGetValue(upgradeDefId, out ShipModuleUpgrade module);
                if (exists)
                {
                    UpgradeUIHelper.BuildUpgradeItemGO(module, upgradePipSlotsGO, false);
                }
                else
                {
                    Mod.Log.Warn?.Write($"Failed to fetch upgrade def: {upgradeDefId} from all known upgrades, skipping!");
                }
            }

        }

        // TODO: Handles states of innate, avialable, unavailable, purchased
        internal static void BuildUpgradeItemGO(ShipModuleUpgrade upgradeDef, GameObject upgradePipSlotsGO, bool isInnate=false)
        {
            Mod.Log.Trace?.Write($"Creating new upgrade item: {upgradeDef.Description.Name}");

            // States
            //  systemId = "uixPrfIndc_SIM_argoUpgradePipUnavailable-element";
            //  systemId = "uixPrfIndc_SIM_argoUpgradePipAvailable-element";
            //  systemId = "uixPrfIndc_SIM_argoUpgradePip-element";
            // TODO: Handle 'innate' states
            string systemToCloneId = "uixPrfIndc_SIM_argoUpgradePipAvailable-element";
            //if (item.isPurchased) { systemToCloneId = "uixPrfIndc_SIM_argoUpgradePip-element";  }
            //else if (!item.isAvailable) { systemToCloneId = "uixPrfIndc_SIM_argoUpgradePipUnavailable-element"; }

            SGEngineeringScreen engineeringScreen = ModState.SimGameSpaceController.sim.RoomManager.EngineeringRoom.engineeringScreen;
            DataManager dm = engineeringScreen.uiManager.dataManager;
            GameObject newUpgradeItemGO = dm.PooledInstantiate(systemToCloneId, BattleTechResourceType.UIModulePrefabs, null, null, upgradePipSlotsGO.transform);
            newUpgradeItemGO.name = ModConsts.UPGRADE_PANEL_ITEM_PREFIX + upgradeDef.Description.Id;
            
            SGEngineeringShipUpgradePip component = newUpgradeItemGO.GetComponent<SGEngineeringShipUpgradePip>();
            component.transform.localScale = Vector3.one;
            component.SetUpgadeModule(upgradeDef);
            ModState.SimGameSpaceController.sim.RequestItem<SVGAsset>(upgradeDef.Description.Icon, component.SetIcon, BattleTechResourceType.SVGAsset);

            component.OnModuleSelected.RemoveAllListeners();
            component.OnModuleSelected.AddListener(engineeringScreen.OnUpgradeSelected);
        }

    }
}

using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BestHTTP.SignalR.Hubs;
using CustomAmmoCategoriesPatches;
using HBS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Helper;

namespace UsedDropshipSalesman.UI
{
    internal class UDSButtonTray : MonoBehaviour
    {
        CombatGameState Combat;
        CombatHUD HUD;

        CombatHUDActionButton Button1;
        CombatHUDActionButton Button2;
        CombatHUDActionButton Button3;
        CombatHUDActionButton Button4;

        bool ButtonsInitialized = false;

        internal void Init(CombatGameState combat, CombatHUD HUD)
        {
            this.Combat = combat;
            this.HUD = HUD;
            GameObject buttonPrefab = HUD.MechWarriorTray.FireButton.gameObject;

            // Create 4 buttons
            GameObject button_1 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_1.name = "UDS_DROPSHIP_CMD_BTN_1";
            this.Button1 = button_1.GetComponent<CombatHUDActionButton>();
            this.Button1.Init(Combat, HUD, BTInput.Instance.Key_None());
            AdjustButtonPrefabs(this.Button1);

            GameObject button_2 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_2.name = "UDS_DROPSHIP_CMD_BTN_2";
            this.Button2 = button_2.GetComponent<CombatHUDActionButton>();
            this.Button2.Init(Combat, HUD, BTInput.Instance.Key_None());
            AdjustButtonPrefabs(this.Button2);

            GameObject button_3 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_3.name = "UDS_DROPSHIP_CMD_BTN_3";
            this.Button3 = button_3.GetComponent<CombatHUDActionButton>();
            this.Button3.Init(Combat, HUD, BTInput.Instance.Key_None());
            AdjustButtonPrefabs(this.Button3);

            GameObject button_4 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_4.name = "UDS_DROPSHIP_CMD_BTN_4";
            this.Button4 = button_4.GetComponent<CombatHUDActionButton>();
            this.Button4.Init(Combat, HUD, BTInput.Instance.Key_None());
            AdjustButtonPrefabs(this.Button4);

            SubscribeMessages(true);
            // UIHelper.BuildDropshipActionButtons(udsRootGO, __instance.MoveButton.gameObject, Combat, __instance);
            // UIHelper.BuildDropshipCommandButtons(udsRootGO, __instance.CommandButton.gameObject, __instance.Combat, __instance.HUD, actor);

            // UIRoot / uixPrfPanl_HUD(Clone) / Representation / BottomHUD_LayoutGroup / MechWarriorTray /
            //     mwt_ActionButtonsLayout / ActionTray2 / actionButton_Holder2 / uixPrfBttn_actionButton-MANAGED
        }

        // Make any changes we want the prefab to show
        internal void AdjustButtonPrefabs(CombatHUDActionButton button)
        {
            GameObject outline_GO = button.gameObject.FindFirstChildNamed("action_Outline");
            RectTransform outline_RT = outline_GO.GetComponent<RectTransform>();
            outline_RT.sizeDelta = new Vector2(45, 45);
            outline_RT.anchoredPosition = new Vector2(0, 0);

            GameObject uses_left_GO = button.gameObject.FindFirstChildNamed("action_UsesLeftCounter");
            LocalizableText uses_left_LT = uses_left_GO.GetComponent<LocalizableText>();
            uses_left_LT.fontSize = 18;
            RectTransform uses_left_RT = uses_left_GO.GetComponent<RectTransform>();
            uses_left_RT.anchoredPosition3D = new Vector3(10f, -40f, 0f);

            GameObject background_GO = button.gameObject.FindFirstChildNamed("action_Background");
            RectTransform background_RT = background_GO.GetComponent<RectTransform>();
            background_RT.sizeDelta = new Vector2(40, 40);
            background_RT.anchoredPosition = new Vector2(2, 2);

            GameObject numbertext_GO = button.gameObject.FindFirstChildNamed("action_numberText");
            numbertext_GO.SetActive(false);

        }

        internal void ResetMechwarriorButtons(AbstractActor actor)
        {
            if (actor == null)
            {
                Button1.DisableButton();
                Button2.DisableButton();
                Button3.DisableButton();
                Button4.DisableButton();
            }
            else
            {
                Mod.Log.Debug?.Log($"Enabling Dropship butttons for actor: {actor.DisplayName}");

                if (!this.ButtonsInitialized)
                {
                    InitButtonFromAbilityDef(Button1, "AbilityDefCMD_UDS_ActiveProbe_Ping", actor);
                    InitButtonFromAbilityDef(Button2, "AbilityDefCMD_UDS_ArtThumperAP", actor);
                    InitButtonFromAbilityDef(Button3, "AbilityDefCMD_UDS_ArtThumperHE", actor);
                    InitButtonFromAbilityDef(Button4, "AbilityDefCMD_UDS_Strafe", actor);
                    this.ButtonsInitialized = true;
                }

                // Check cooldowns and num uses on the buttons before enabling.
                if (Button1.IsAvailable) 
                {
                    Button1.isClickable = true;
                    Button1.setState(CombatHUDActionButton.ButtonState.Active, actor);
                }
                if (Button2.IsAvailable)
                {
                    Button2.isClickable = true;
                    Button2.setState(CombatHUDActionButton.ButtonState.Active, actor);
                }
                if (Button3.IsAvailable)
                {
                    Button3.isClickable = true;
                    Button3.setState(CombatHUDActionButton.ButtonState.Active, actor);
                }
                if (Button4.IsAvailable)
                {
                    Button4.isClickable = true;
                    Button4.setState(CombatHUDActionButton.ButtonState.Active, actor);
                }

                Button1.RefreshUIColors();
                Button2.RefreshUIColors();
                Button3.RefreshUIColors();
                Button4.RefreshUIColors();
            }
        }

        private void InitButtonFromAbilityDef(CombatHUDActionButton button, string abilityId, AbstractActor actor)
        {
            bool had_key = this.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");
            Ability ability = new(abilityDef);

            // TODO: Probably need to track these across initializations
            ability.NumUsesLeft = ability.Def.NumberOfUses;
            SelectionType abilitySelectionType = CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false);
            button.InitButton(abilitySelectionType, ability, ability?.Def?.AbilityIcon, ability?.Def?.Description?.Id, ability?.Def?.Description?.Name, actor);
        }

        private void OnActorSelected(MessageCenterMessage message)
        {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);
            if (actor != null && actor.team != null && actor.team.IsLocalPlayer)
            {
                // What we want: Show the buttons, re-initialize them to the current actor
                ResetMechwarriorButtons(actor);
            }
        }

        private void OnActorDeselected(MessageCenterMessage message)
        {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            ResetMechwarriorButtons(null);
        }

        public void OnTurnActorActivated(MessageCenterMessage message)
        {
            TurnActorActivateMessage msg = message as TurnActorActivateMessage;
            if (msg == null) return;
            if (!Combat.TurnDirector.IsInterleaved) return;

            Team team = Combat.Teams.Find((Team x) => x.GUID == msg.TurnActorGUID);
            if (msg.TurnActorGUID == Combat.LocalPlayerTeam.GUID)
            {
                Mod.Log.Trace?.Log("Updating defs for availability");
                // TODO: Update defs
            }
        }

        internal void SubscribeMessages(bool subscribe = false)
        {
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, OnActorSelected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorDeselectedMessage, OnActorDeselected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnTurnActorActivate, OnTurnActorActivated, subscribe);
        }

        public void OnCombatGameDestroyed()
        {
            SubscribeMessages(subscribe: false);
        }

        public void Update()
        {

        }
    }
}

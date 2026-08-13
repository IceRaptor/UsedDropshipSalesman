using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using BestHTTP.SignalR.Hubs;
using CustomAmmoCategoriesPatches;
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
            this.Button1.DisableButton();

            GameObject button_2 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_2.name = "UDS_DROPSHIP_CMD_BTN_2";
            this.Button2 = button_2.GetComponent<CombatHUDActionButton>();
            this.Button2.Init(Combat, HUD, BTInput.Instance.Key_None());
            this.Button2.DisableButton();

            GameObject button_3 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_3.name = "UDS_DROPSHIP_CMD_BTN_3";
            this.Button3 = button_3.GetComponent<CombatHUDActionButton>();
            this.Button3.Init(Combat, HUD, BTInput.Instance.Key_None());
            this.Button3.DisableButton();

            GameObject button_4 = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
            button_4.name = "UDS_DROPSHIP_CMD_BTN_4";
            this.Button4 = button_4.GetComponent<CombatHUDActionButton>();
            this.Button4.Init(Combat, HUD, BTInput.Instance.Key_None());
            this.Button4.DisableButton();

            SubscribeMessages(true);
            // UIHelper.BuildDropshipActionButtons(udsRootGO, __instance.MoveButton.gameObject, Combat, __instance);
            // UIHelper.BuildDropshipCommandButtons(udsRootGO, __instance.CommandButton.gameObject, __instance.Combat, __instance.HUD, actor);

            // UIRoot / uixPrfPanl_HUD(Clone) / Representation / BottomHUD_LayoutGroup / MechWarriorTray /
            //     mwt_ActionButtonsLayout / ActionTray2 / actionButton_Holder2 / uixPrfBttn_actionButton-MANAGED
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
                InitButtonFromAbilityDef(Button1, "AbilityDefCMD_UDS_ActiveProbe_Ping", actor);
                InitButtonFromAbilityDef(Button2, "AbilityDefCMD_UDS_ArtThumperAP", actor);
                InitButtonFromAbilityDef(Button3, "AbilityDefCMD_UDS_ArtThumperHE", actor);
                InitButtonFromAbilityDef(Button4, "AbilityDefCMD_UDS_Strafe", actor);

                Button1.isClickable = true;
                Button2.isClickable = true;
                Button3.isClickable = true;
                Button4.isClickable = true;

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
            //AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);
            //if (actor != null && actor.team != null && actor.team.IsLocalPlayer)
            //{
            //    // What we want: Show the buttons, re-initialize them to the current actor
            //    ResetMechwarriorButtons(actor);
            //}
        }

        public void OnTurnActorActivated(MessageCenterMessage message)
        {
            TurnActorActivateMessage msg = message as TurnActorActivateMessage;
            if (msg == null) return;
            
            Team team = Combat.Teams.Find((Team x) => x.GUID == msg.TurnActorGUID);
            if (msg.TurnActorGUID == Combat.LocalPlayerTeam.GUID)
            {
                Mod.Log.Trace?.Log("Enabling the UDS_DROPSHIP_BTN_ROOT");
                this.transform.parent.gameObject.SetActive(true);
            }
            else 
            {
                Mod.Log.Trace?.Log("Disabling the UDS_DROPSHIP_BTN_ROOT");
                this.transform.parent.gameObject.SetActive(false);
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

    }
}

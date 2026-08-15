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

        CombatHUDActionButton[] Buttons = new CombatHUDActionButton[4];
        bool[] ButtonsInitialized = new bool[4];

        internal void Init(CombatGameState combat, CombatHUD HUD)
        {
            this.Combat = combat;
            this.HUD = HUD;
            GameObject buttonPrefab = HUD.MechWarriorTray.FireButton.gameObject;

            // Create 4 buttons
            for (int i = 0; i < 4; i++)
            {
                var buttonGO = UnityEngine.Object.Instantiate(buttonPrefab, this.gameObject.transform);
                buttonGO.name = $"UDS_DROPSHIP_CMD_BTN_{i}";
                var button = buttonGO.GetComponent<CombatHUDActionButton>();
                button.Init(Combat, HUD, BTInput.Instance.Key_None());
                AdjustButtonPrefabs(button);
                this.Buttons[i] = button;
            }

            SubscribeMessages(true);
        }

        public bool HasActiveButtons()
        {
            foreach (var button in Buttons)
            {
                if (button.gameObject.activeSelf) return true;
            }
            return false;
        }

        // Make any changes we want the prefab to show
        internal void AdjustButtonPrefabs(CombatHUDActionButton button)
        {
            GameObject outline_GO = button.gameObject.FindFirstChildNamed("action_Outline");
            RectTransform outline_RT = outline_GO.GetComponent<RectTransform>();
            outline_RT.sizeDelta = new Vector2(45, 45);

            GameObject uses_left_GO = button.gameObject.FindFirstChildNamed("action_UsesLeftCounter");
            LocalizableText uses_left_LT = uses_left_GO.GetComponent<LocalizableText>();
            uses_left_LT.fontSize = 18;
            RectTransform uses_left_RT = uses_left_GO.GetComponent<RectTransform>();
            uses_left_RT.anchoredPosition3D = new Vector3(10f, -40f, 0f);

            GameObject background_GO = button.gameObject.FindFirstChildNamed("action_Background");
            RectTransform background_RT = background_GO.GetComponent<RectTransform>();
            background_RT.sizeDelta = new Vector2(45, 45);

            GameObject numbertext_GO = button.gameObject.FindFirstChildNamed("action_numberText");
            numbertext_GO.SetActive(false);

            GameObject cooldown_GO = button.gameObject.FindFirstChildNamed("action_CooldownTimer");

        }

        internal void ResetMechwarriorButtons(AbstractActor actor)
        {
            ResetDropshipButton(0, actor, ModState.CombatButton_1_AbilityDefId);
            ResetDropshipButton(1, actor, ModState.CombatButton_2_AbilityDefId);
            ResetDropshipButton(2, actor, ModState.CombatButton_3_AbilityDefId);
            ResetDropshipButton(3, actor, ModState.CombatButton_4_AbilityDefId);
        }

        private void ResetDropshipButton(int buttonIdx, AbstractActor actor, string abilityDefId)
        {

            CombatHUDActionButton button = this.Buttons[buttonIdx];
            if (!this.ButtonsInitialized[buttonIdx])
            {
                Ability ability = null;
                if ((ability = GetAbility(abilityDefId)) != null)
                {
                    InitButtonFromAbility(button, ability, actor);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
                this.ButtonsInitialized[buttonIdx] = true;
            }

            // If we've disabled ourself, just skip processing
            if (!button.gameObject.activeSelf) return;

            if (actor == null)
            {
                Mod.Log.Debug?.Log($"Disabling Dropship buttton: '{button.gameObject.name}' as actor is null");
                button.DisableButton();
                button.RefreshColors(null, null);
            }
            else
            {
                Mod.Log.Debug?.Log($"Enabling Dropship butttons: '{button.gameObject.name}' for actor: {actor.DisplayName}");
                // Check cooldowns and num uses on the buttons before enabling.
                if (button.Ability.IsAvailable)
                {
                    button.isClickable = true;
                    button.setState(CombatHUDActionButton.ButtonState.Active, actor);
                }
                else
                {
                    button.isClickable = false;
                    button.ShowAbilityTiming();
                    button.setState(CombatHUDActionButton.ButtonState.Disabled, actor);
                }

                button.RefreshColors(actor, null);
            }

        }

        private Ability GetAbility(string abilityId)
        {
            if (String.IsNullOrEmpty(abilityId)) return null;

            bool had_key = this.Combat.DataManager.abilityDefs.TryGet(abilityId, out AbilityDef abilityDef);
            Mod.Log.Trace?.Log($"AbilityDef with id: {abilityId} was found: {had_key}?");
            Ability ability = new(abilityDef);

            return ability;
        }

        private void InitButtonFromAbility(CombatHUDActionButton button, Ability ability, AbstractActor actor)
        {
            ability.Init(actor.Combat);
            Mod.Log.Trace?.Log($"Ability: {ability.Def.Description.Id} initialized  NumUsesLeft: {ability.NumUsesLeft}  CurrentCooldow: {ability.CurrentCooldown}");

            SelectionType abilitySelectionType = CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, warnAboutUnsupportedTypes: false);
            button.InitButton(abilitySelectionType, ability, ability?.Def?.AbilityIcon, ability?.Def?.Description?.Id, ability?.Def?.Description?.Name, actor);
            Mod.Log.Trace?.Log($"Initialized CHUDActionButton: {button.name} with ability: {ability.Def.Description.Id} and selectionType: {abilitySelectionType}");
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

        public void OnRoundBegin(MessageCenterMessage message)
        {
            RoundBeginMessage msg = message as RoundBeginMessage;
            if (msg == null) return;

            foreach (var button in this.Buttons)
            {
                if (button != null && button.Ability != null && button.gameObject.activeSelf)
                {
                    button.Ability.OnNewRound();
                }
            }
        }

        internal void SubscribeMessages(bool subscribe = false)
        {
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, OnActorSelected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorDeselectedMessage, OnActorDeselected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, OnRoundBegin, subscribe);
        }

        public void OnCombatGameDestroyed()
        {
            SubscribeMessages(subscribe: false);
        }

    }
}

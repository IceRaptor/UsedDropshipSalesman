using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using HBS.Extensions;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UsedDropshipSalesman.UI
{
    internal class UDSDropshipCombatFrame : MonoBehaviour 
    {
        CombatGameState Combat;
        CombatHUD HUD;

        GameObject TrayImageGO;
        GameObject TextTrayGO;
        GameObject ButtonTrayGO;

        SVGImage DropshipCommandLabelImage;

        public Color ActiveColor = new(1f, 0.635f, 0, 1f);

        UDSButtonTray ButtonTray;

        // All values assume parenting against CombatHUD.PhaseTrack 
        public void Init(CombatGameState Combat, CombatHUD HUD, CombatHUDPhaseTrack phaseTrack)
        {
            Mod.Log.Trace?.Log("UDSDropshipCombatFrame::Init");

            this.Combat = Combat;
            this.HUD = HUD;

            this.gameObject.transform.localPosition = new Vector3(-520, -50, 0);

            GameObject trayLabelGO = new("UDS_DROPSHIP_BTN_LABEL_ROW");
            trayLabelGO.transform.parent = this.gameObject.transform;
            trayLabelGO.transform.position = this.gameObject.transform.position;
            trayLabelGO.transform.localPosition = Vector3.zero;

            // Build the label + background image. Must be first to allow text to overlay
            GameObject trayImagePrefab = phaseTrack.gameObject.FindFirstChildNamed("playerT_BG (1)");
            Mod.Log.Trace?.Log($" trayImagePrefab == null {trayImagePrefab == null}");
            TrayImageGO = UnityEngine.Object.Instantiate(trayImagePrefab, trayLabelGO.transform);
            TrayImageGO.name = "UDS_DROPSHIP_BTN_LABEL_IMG";
            TrayImageGO.transform.position = trayLabelGO.transform.position;
            TrayImageGO.transform.localPosition = Vector3.zero;

            DropshipCommandLabelImage = TrayImageGO.GetComponent<SVGImage>(); // "uixSvgLine_hor3pt";
            DropshipCommandLabelImage.color = ActiveColor;

            RectTransform rt2 = TrayImageGO.GetComponent<RectTransform>();
            rt2.sizeDelta = new Vector2(220, 30);

            // Build the text next
            GameObject trayTextPrefab = phaseTrack.gameObject.FindFirstChildNamed("playerT_Text (1)");
            Mod.Log.Trace?.Log($" trayTextPrefab == null {trayTextPrefab == null}");
            TextTrayGO = UnityEngine.Object.Instantiate(trayTextPrefab, trayLabelGO.transform);
            TextTrayGO.name = "UDS_DROPSHIP_BTN_LABEL_TEXT";
            TextTrayGO.transform.position = trayLabelGO.transform.position;
            TextTrayGO.transform.localPosition = Vector3.zero;

            LocalizableText lt = TextTrayGO.GetComponent<LocalizableText>();
            lt.fontSize = 18;
            // TODO: Make localized
            lt.text = "Dropship Command";

            // Build the tray for buttons
            ButtonTrayGO = new("UDS_DROPSHIP_BTN_TRAY");
            ButtonTrayGO.transform.parent = this.gameObject.transform;
            ButtonTrayGO.transform.position = this.gameObject.transform.position + new Vector3(0, -60, 0);

            RectTransform rt = ButtonTrayGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(128, 128);
            HorizontalLayoutGroup hlg = ButtonTrayGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            ContentSizeFitter csf = ButtonTrayGO.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ButtonTray = ButtonTrayGO.AddComponent<UDSButtonTray>();
            ButtonTray.Init(Combat, HUD);

            SubscribeMessages(true);

            this.gameObject.SetActive(true);
            this.ButtonTrayGO.SetActive(true);
        }

        internal void SubscribeMessages(bool subscribe = false)
        {
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorSelectedMessage, OnActorSelected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.ActorDeselectedMessage, OnActorDeselected, subscribe);
            Combat.MessageCenter.Subscribe(MessageCenterMessageType.OnEncounterBegin, OnEncounterBegin, subscribe);
        }

        private void OnActorSelected(MessageCenterMessage message)
        {
            if (!Combat.TurnDirector.IsInterleaved)
            {
                Mod.Log.Trace?.Log("Non-Interleaved mode, disabling the UDS_DROPSHIP_BTN_ROOT");
                this.gameObject.SetActive(false);
                this.ButtonTrayGO.SetActive(false);
                return;
            }

            if (!ButtonTray.HasActiveButtons())
            {
                Mod.Log.Trace?.Log("No active buttons, disabling the UDS_DROPSHIP_BTN_ROOT");
                this.gameObject.SetActive(false);
                this.ButtonTrayGO.SetActive(false);
                return;
            }

            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);

            Mod.Log.Trace?.Log($"UDSDropshipCombatFrame::OnActorSelected - actorSelectedMessage.affectedObjectGuid: {actorSelectedMessage.affectedObjectGuid}  " +
                $"actor: {actor?.DisplayName}  actorTeam: {actor?.team?.Name}  actorTeamGuid: {actor?.team?.GUID}  " +
                $"localPlayerTeamGuid: {Combat.LocalPlayerTeamGuid} isInterleaved? {Combat?.TurnDirector?.IsInterleaved}");

            if (actor != null && actor.team != null && actor.team.IsLocalPlayer)
            {
                Mod.Log.Trace?.Log("Enabling the UDS_DROPSHIP_BTN_ROOT");
                DropshipCommandLabelImage.color = ActiveColor;

                this.gameObject.SetActive(true);
                this.ButtonTrayGO.SetActive(true);

            }
            else
            {
                Mod.Log.Trace?.Log("Not the local player team, disabling the UDS_DROPSHIP_BTN_ROOT");
                this.gameObject.SetActive(false);
                this.ButtonTrayGO.SetActive(false);
            }
        }

        private void OnActorDeselected(MessageCenterMessage message)
        {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
        }


        // This is necessary to allow the label's color to be changed when the player first sees it.
        //  No matter what I do, the very first time the label is drawn the color of the label is white.
        //  We enable the GO during the briefing slide, then immediately hide it as the encounter starts. 
        //  This is key to letting our color changes appear when the first actor is selected.
        private void OnEncounterBegin(MessageCenterMessage message)
        {
            this.gameObject.SetActive(false);
            this.ButtonTrayGO.SetActive(false);
        }

        public void OnCombatGameDestroyed()
        {
            this.ButtonTray.OnCombatGameDestroyed();
            SubscribeMessages(false);
        }
    }
}

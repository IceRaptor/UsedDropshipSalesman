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
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
            AbstractActor actor = Combat.FindActorByGUID(actorSelectedMessage.affectedObjectGuid);
            if (actor != null && actor.team != null && actor.team.IsLocalPlayer && Combat.TurnDirector.IsInterleaved)
            {
                Mod.Log.Trace?.Log("Enabling the UDS_DROPSHIP_BTN_ROOT");

                Mod.Log.Trace?.Log($"   -- DROPSHIP_COMMAND_IMG_COLOR_PRE : {DropshipCommandLabelImage.color}");
                DropshipCommandLabelImage.color = ActiveColor;

                this.gameObject.SetActive(true);
                this.ButtonTrayGO.SetActive(true);

                Mod.Log.Trace?.Log($"   -- DROPSHIP_COMMAND_IMG_COLOR_POST: {DropshipCommandLabelImage.color}");
            }
            else
            {
                Mod.Log.Trace?.Log("Disabling the UDS_DROPSHIP_BTN_ROOT");
                this.gameObject.SetActive(false);
                this.ButtonTrayGO.SetActive(false);
            }
        }

        private void OnActorDeselected(MessageCenterMessage message)
        {
            ActorSelectedMessage actorSelectedMessage = message as ActorSelectedMessage;
        }


        private void OnEncounterBegin(MessageCenterMessage message)
        {
            this.gameObject.SetActive(false);
            this.ButtonTrayGO.SetActive(false);
        }


        //public void Update()
        //{
        //    Mod.Log.Trace?.Log($"DROPSHIP CMD IMG COLOR: {DropshipCommandLabelImage?.color}");
        //}

        public void OnCombatGameDestroyed()
        {
            this.ButtonTray.OnCombatGameDestroyed();
            SubscribeMessages(false);
        }
    }
}

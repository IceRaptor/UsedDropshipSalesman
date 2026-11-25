using BattleTech.UI;
using CustomUnits;
using FogOfWar;
using HBS;
using HBS.Collections;
using HBS.Logging;
using HBS.Util;
using Localize;
using ShaderControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UsedDropshipSalesman.Defs;
using static ErosionBrushPlugin.Matrix;

namespace UsedDropshipSalesman.Sequence
{
    internal class UDSSatelliteViewSequence : MultiSequence
    {
        private enum SequenceState
        {
            None,
            RevealRadius,
            HighlightTargets,
            Finished
        }

        private SequenceState state;

        private float timeInCurrentState;

        private float timeSinceLastTargetChanged;

        private const float TimeDuringReveal = 1f;
        private const float TimeBetweenTargets = 1f;

        public Team LocalPlayerTeam { get; private set; }

        public Vector3 TargetPos { get; private set; }

        public float Radius { get; private set; }

        public CustomCommandActionEffectsDef ActionEffectsDef { get; private set;  }
        
        public List<AbstractActor> Targets { get; private set; }

        public override bool IsValidMultiSequenceChild => false;

        public override bool IsParallelInterruptable => false;

        public override bool IsCancelable => false;

        public override bool IsComplete => state == SequenceState.Finished;


        public UDSSatelliteViewSequence(CombatGameState combat, Vector3 targetPos, float radius, string commandActionDefId)
            : base(combat)
        {
            TargetPos = targetPos;
            Radius = radius;
            LocalPlayerTeam = base.Combat.LocalPlayerTeam;
            state = SequenceState.None;

            bool hasEffect = Mod.Config.CommandActionEffects.TryGetValue(commandActionDefId, out CustomCommandActionEffectsDef actionEffectDef);
            if (hasEffect) { this.ActionEffectsDef = actionEffectDef; }

            Mod.Log.Info?.Log($"Created UDSSatelliteSequence at pos: {targetPos} with radius: {radius}");

        }

        private void SetState(SequenceState newState)
        {
            if (state == newState) return; // Nothing to do

            state = newState;
            timeInCurrentState = 0f;
            switch (newState)
            {
                case SequenceState.RevealRadius:
                    CollectTargets();
                    // Focus camera on targetPos
                    SetCamera(CameraControl.Instance
                        .ShowRandomizedFocalCam(TargetPos, CameraControl.Instance.MinTerrainHeight, CameraControl.Instance.MaxTerrainHeight * 0.5f, 3f),
                        base.SequenceGUID
                        );

                    // Show probe effect
                    RadarPingIndicator.Instance.Ping(TargetPos);
                    WwiseManager.PostEvent(AudioEventList_ui.ui_target_sensor_lock_hard, WwiseManager.GlobalAudioObject);

                    // Show the space
                    this.RevealRadius(TargetPos, Radius, $"{this.SequenceGUID}");

                    Mod.Log.Trace?.Log($"UDSSatelliteSequence::RevealRadius");
                    break;
                case SequenceState.HighlightTargets:
                    
                    AudioEventManager.PlayRandomPilotVO(VOEvents.SensorLock_Ally, base.Combat.LocalPlayerTeam, base.Combat.LocalPlayerTeam.units);

                    Mod.Log.Trace?.Log($"UDSSatelliteSequence::Incoming");
                    break;
                case SequenceState.Finished:
                    Mod.Log.Trace?.Log($"UDSSatelliteSequence::Finished");
                    break;
            }
        }

        private void Update()
        {
            timeInCurrentState += Time.deltaTime;
            switch (state)
            {
                case SequenceState.RevealRadius:
                    // Find the targets and center on the target pos as they are revealed
                    if (timeInCurrentState > TimeDuringReveal)
                    {
                        SetState(SequenceState.HighlightTargets);
                    }
                    break;
                case SequenceState.HighlightTargets:
                    if (Targets.Count <= 0)
                    {
                        SetState(SequenceState.Finished);
                        break;
                    }
                    timeSinceLastTargetChanged += Time.deltaTime;
                    if (timeSinceLastTargetChanged > TimeBetweenTargets && Targets.Count > 0)
                    {
                        AbstractActor targetActor = Targets[0];
                        Targets.Remove(targetActor);
                        SetCamera(CameraControl.Instance.ShowActorCam(targetActor, targetActor.CurrentRotation, TimeBetweenTargets), base.SequenceGUID);
                        ApplyEffects(targetActor);
                        if (targetActor.GameRep is PilotableActorRepresentation par)
                        {
                            par.SetForcedPlayerVisibilityLevel(VisibilityLevel.LOSFull);
                        }
                        timeSinceLastTargetChanged = 0f;
                    }
                    break;
                case SequenceState.Finished:
                    break;
            }
        }

        private void ApplyEffects(AbstractActor targetActor)
        {
            // TODO: Apply all effects here
            if (ActionEffectsDef == null || ActionEffectsDef.Effects == null || ActionEffectsDef.Effects.Count == 0)
            { 
                return;
            }

            foreach (EffectData effectData in ActionEffectsDef.Effects)
            {
                try
                {
                    Mod.Log.Debug?.Log($"Applying effect: {effectData.Description.Id} to target: {targetActor.DisplayName} from action: {ActionEffectsDef.Description.Id}");
                    base.Combat.EffectManager.CreateEffect(effectData,
                        $"{base.SequenceGUID}_{ActionEffectsDef.Description.EffectSuffix}",
                        base.SequenceGUID, LocalPlayerTeam, targetActor, default(WeaponHitInfo), -1);
                }
                catch (Exception ex)
                {
                    Mod.Log.Info?.Log($"Failed to apply effect to target: {targetActor}", ex);                    
                }
            }
        }

        private void CollectTargets()
        {
            Targets = new List<AbstractActor>();
            List<AbstractActor> allActors = base.Combat.AllActors;
            foreach (AbstractActor actor in allActors)
            {
                if (actor.IsDead || actor.IsFlaggedForDeath) continue; // Don't target

                float distance = Vector3.Distance(TargetPos, actor.currentPosition);
                if (distance < Radius)
                {
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is INSIDE satellite view, adding to targets.");
                    Targets.Add(actor);
                }
                else
                {
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is not in satellite view, skipping.");
                }
            }
        }

        // Override MultiSequence.OnComplete to not call ClearShownList().
        //   We instead leave the forced visibility flag in place and will clear it when the actor moved
        public override void OnComplete()
        {

            // DEBUG: Look at all combatants and print effects on them
            foreach (ICombatant combatant in base.Combat.GetAllCombatants())
            {
                if (combatant is AbstractActor actor && !actor.IsDead)
                {
                    Mod.Log.Debug?.Log($" === Effects for actor: {actor.DisplayName}  isForcedVisible: {actor.IsForcedVisible}");
                    Mod.Log.Debug?.Log($"  SpottingVisibilityAbsolute: {actor.SpottingVisibilityAbsolute}  " +
                        $"SpotterDistanceMultiplier: {actor.SpotterDistanceMultiplier}  " +
                        $"SensorSignatureModifier: {actor.SensorSignatureModifier}");
                    List<BattleTech.Effect> actorEffects = base.Combat.EffectManager.GetAllEffectsTargeting(actor);
                    foreach (BattleTech.Effect effect in actorEffects)
                    {
                        Mod.Log.Debug?.Log($"    -- Effect: '{effect?.id}'  " +
                            $"ED.Name:  '{effect?.effectData?.Description?.Name}'  " +
                            $"ED.ID  '{effect?.effectData?.Description?.Id}'");
                    }
                }
            }

            ClearCamera();
            ClearFocalPoint();
            if (base.CompletedCallback != null)
            {
                base.CompletedCallback();
            }
        }

        public override void OnAdded()
        {
            base.OnAdded();
            SetState(SequenceState.RevealRadius);           
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Update();
        }

        public override int Size()
        {
            return 0;
        }

        public override bool ShouldSave()
        {
            return false;
        }

        public override void Save(SerializationStream stream)
        {
        }

        public override void Load(SerializationStream stream)
        {
        }

        public override void LoadComplete()
        {
        }
    }
}

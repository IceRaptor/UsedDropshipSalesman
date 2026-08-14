using BattleTech.UI;
using HBS.Collections;
using HBS.Logging;
using HBS.Util;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ErosionBrushPlugin.Matrix;

namespace UsedDropshipSalesman.Sequence
{
    internal class UDSActiveProbeSequence : MultiSequence
    {
        private enum SequenceState
        {
            None,
            ShowingEffects,
            Targeting,
            Finished
        }

        private SequenceState state;

        private float timeInCurrentState;

        private float timeSinceLastTargetChanged;

        private const float TimeBetweenWaves = 1f;

        private const float WaveCameraViewTime = 2f;

        private const float timeBetweenAttacks = 0.25f;

        private const float timeShowingEffects = 2f;

        public string TeamGUID { get; private set; }

        public Turret Attacker { get; private set; }

        public Vector3 TargetPos { get; private set; }

        public float Radius { get; private set; }

        private float timeSinceLastWave;

        private int numWavesFired;

        private ParticleSystem probeParticles;

        private bool playedVO;

        public List<AbstractActor> Targets { get; private set; }

        public override bool IsValidMultiSequenceChild => false;

        public override bool IsParallelInterruptable => false;

        public override bool IsCancelable => false;

        public override bool IsComplete => state == SequenceState.Finished;


        public UDSActiveProbeSequence(CombatGameState combat, string teamGUID, Turret attacker, Vector3 targetPos, float radius)
            : base(combat)
        {
            Attacker = attacker;
            TargetPos = targetPos;
            Radius = radius;
            TeamGUID = base.Combat.LocalPlayerTeamGuid;
            state = SequenceState.None;
            Mod.Log.Info?.Log($"Created UDSActiveProbeSequence with source: {attacker?.DisplayName} at pos: {targetPos} with radius: {radius}");
        }

        private void Update()
        {
            timeInCurrentState += Time.deltaTime;
            switch (state)
            {
                case SequenceState.ShowingEffects:
                    PlayWaveEffect();
                    if (timeInCurrentState > 2f || numWavesFired >= 3)
                    {
                        SetState(SequenceState.Targeting);
                    }
                    break;
                case SequenceState.Targeting:
                    if (Targets.Count == 0)
                    {
                        SetState(SequenceState.Finished);
                        break;
                    }
                    timeSinceLastTargetChanged += Time.deltaTime;
                    if (timeSinceLastTargetChanged > 0.25f && Targets.Count > 0)
                    {
                        AbstractActor targetActor = Targets[0];
                        Targets.Remove(targetActor);
                        ApplyEffects(targetActor);
                        timeSinceLastTargetChanged = 0f;
                    }
                    break;
                case SequenceState.Finished:
                    break;
            }
        }

        private void SetState(SequenceState newState)
        {
            if (state == newState) return; // Nothing to do

            state = newState;
            timeInCurrentState = 0f;
            switch (newState)
            {
                case SequenceState.ShowingEffects:
                    CollectTargets();
                    SetCamera(CameraControl.Instance
                        .ShowRandomizedFocalCam(TargetPos, CameraControl.Instance.MinTerrainHeight, CameraControl.Instance.MaxTerrainHeight * 0.5f, 3f), 
                        base.SequenceGUID
                        );
                    AudioEventManager.PlayRandomPilotVO(VOEvents.SensorLock_Ally, base.Combat.LocalPlayerTeam, base.Combat.LocalPlayerTeam.units);

                    Mod.Log.Trace?.Log($"UDSActiveProbeSequence::Incoming");
                    break;
                case SequenceState.Targeting:

                    Mod.Log.Trace?.Log($"UDSActiveProbeSequence::Shelling");
                    break;
                case SequenceState.Finished:
                    ClearCamera();
                    ClearVFX();
                    base.Combat.FlagECMStateNeedsRefreshing();
                    Mod.Log.Trace?.Log($"UDSActiveProbeSequence::Finished");
                    break;
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
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is within radius of attack, adding to targets.");
                    Targets.Add(actor);
                }
                else
                {
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is outside radius of attack, skipping.");
                }
            }
        }

        private void PlayWaveEffect()
        {
            CameraControl.Instance.ClearTargets();
            WwiseManager.PostEvent(AudioEventList_ui.ui_target_sensor_lock_hard, WwiseManager.GlobalAudioObject);
            numWavesFired++;
            timeSinceLastWave = 0f;
        }

        private void FireWave(AbstractActor Target)
        {
            //RadarPingIndicator.Instance.Ping(Target.CurrentPosition);
            //SetCamera(CameraControl.Instance.ShowSensorLockCam(Target, 2f), base.MessageIndex);
            //CameraControl.Instance.ClearTargets();
            //WwiseManager.PostEvent(AudioEventList_ui.ui_target_sensor_lock_hard, WwiseManager.GlobalAudioObject);
            //List<Effect> list = base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorLockSingleStepEffect, $"{base.SequenceGUID}_SensorLock", base.SequenceGUID, owningActor, Target, default(WeaponHitInfo), -1);
            //for (int i = 0; i < list.Count; i++)
            //{
            //    Target.ProcessAddedMark(list[i]);
            //}
            //base.Combat.MessageCenter.PublishMessage(new FloatieMessage(TeamGUID, Target.GUID, base.Combat.Constants.Visibility.SensorsImpairedEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
            //for (int j = 0; j < base.Combat.Constants.Visibility.NumSensorLockImpairedEffects; j++)
            //{
            //    base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorsImpairedEffect, $"{base.SequenceGUID}_SensorsImpaired", base.SequenceGUID, owningActor, Target, default(WeaponHitInfo), -1);
            //}
            //StripECMEffects(Target);
            //base.Combat.FlagECMStateNeedsRefreshing();
            //if (Target.BehaviorTree != null && !Target.BehaviorTree.IsTargetIgnored(owningActor))
            //{
            //    Target.LastTargetedPhaseNumber = base.Combat.TurnDirector.TotalElapsedPhases;
            //}
            //owningActor.UpdateVisibilityCache(base.Combat.GetAllImporantCombatants());
            //StripEvasivePips(Target);
            //numWavesFired++;
            //timeSinceLastWave = 0f;
            //if (!playedVO)
            //{
            //    playedVO = true;
            //    if (owningActor.team.LocalPlayerControlsTeam)
            //    {
            //        AudioEventManager.PlayPilotVO(VOEvents.SensorLock_Ally, owningActor);
            //    }
            //    else
            //    {
            //        AudioEventManager.PlayPilotVO(VOEvents.SensorLock_Enemy, Target);
            //    }
            //}
        }



        private void ClearVFX()
        {
            // TODO: Why do they create this?
            //base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.FiredWeaponsAntiStealthEffect, 
            //    $"{base.SequenceGUID}_AntiStealth", base.SequenceGUID, owningActor, owningActor, default(WeaponHitInfo), -1);

            if (probeParticles != null)
            {
                probeParticles.Stop(withChildren: true);
                base.Combat.DataManager.PoolGameObject(base.Combat.Constants.VFXNames.active_probe_effect, probeParticles.gameObject);
            }

            WwiseManager.PostEvent(AudioEventList_activeProbe.activeProbe_stop, WwiseManager.GlobalAudioObject);
        }

        private void ApplyEffects(AbstractActor actor)
        {
            StripECMEffects(actor);
            StripEvasivePips(actor);
            ApplyVisionEffect(actor);
        }

        private void StripECMEffects(AbstractActor target)
        {
            List<Effect> allEffects = base.Combat.EffectManager.GetAllEffectsTargeting(target);
            List<Effect> allEcmEffects = allEffects.FindAll((Effect x) => x.EffectData.targetingData.auraEffectType == AuraEffectType.ECM_GENERAL);
            foreach (Effect item in allEffects)
            {
                target.CancelEffect(item);
            }

        }

        private void StripEvasivePips(AbstractActor target)
        {
            // TODO: Do we need to use an actor GUID here, instead of team GUID?
            base.Combat.MessageCenter.PublishMessage(new FloatieMessage(TeamGUID, target.GUID, "SENSOR LOCKED", FloatieMessage.MessageNature.Debuff));
            if (target.HasSensorLockEvasiveImmunity)
            {
                base.Combat.MessageCenter.PublishMessage(new FloatieMessage(TeamGUID, target.GUID, "EVASION UNCHANGED", FloatieMessage.MessageNature.Buff));
                return;
            }

            // TODO: Make this configurable by the abilityDef
            int evasivePipsCurrent = target.EvasivePipsCurrent;
            if (base.Combat.Constants.ToHit.SensorLockStripsEvasivePips)
            {
                for (int i = 0; i < base.Combat.Constants.ToHit.SensorLockPipsStripped; i++)
                {
                    target.ConsumeEvasivePip();
                }
            }

            int evasivePipsCurrent2 = target.EvasivePipsCurrent;
            if (evasivePipsCurrent2 < evasivePipsCurrent)
            {
                int num = evasivePipsCurrent - evasivePipsCurrent2;
                base.Combat.MessageCenter.PublishMessage(new FloatieMessage(target.GUID, target.GUID, Strings.T("-{0} EVASION", num), FloatieMessage.MessageNature.Debuff));
            }
        }

        private void ApplyVisionEffect(AbstractActor target)
        {
            // TODO: Make this configurable by abilityDef
            Team effectTeam = Combat.LocalPlayerTeam;
            base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorLockAntiStealthEffect,
                $"{base.SequenceGUID}_AntiStealth", base.SequenceGUID, effectTeam, target, default(WeaponHitInfo), -1);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            SetState(SequenceState.ShowingEffects);
            probeParticles = base.Combat.DataManager
                .PooledInstantiate(base.Combat.Constants.VFXNames.active_probe_effect, BattleTechResourceType.Prefab, TargetPos)
                .GetComponent<ParticleSystem>();
            probeParticles.gameObject.transform.position = TargetPos;
            probeParticles.Clear(withChildren: true);
            probeParticles.Play(withChildren: true);
            WwiseManager.PostEvent(AudioEventList_activeProbe.activeProbe_play, WwiseManager.GlobalAudioObject);
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

using BattleTech.UI;
using HBS.Logging;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Sequence
{
    //internal class UDSSensorLockSequence : MultiSequence
    internal class UDSSensorLockSequence
    {
        //public static readonly ILog attackLogger = HBS.Logging.Logger.GetLogger("CombatLog.Attacking", LogLevel.Warning);

        //private const float TimeBetweenWaves = 1f;

        //private float timeSinceLastWave;

        //private int numWavesFired;

        //private bool playedVO;

        //public AbstractActor Target { get; private set; }

        //public SensorLockSequence(AbstractActor source, AbstractActor target)
        //    : base(source)
        //{
        //    Target = target;
        //    timeSinceLastWave = 0f;
        //    numWavesFired = 0;
        //    playedVO = false;
        //}

        //public void ClearAllChildren()
        //{
        //    ClearCamera();
        //    for (int num = base.childSequences.Count - 1; num >= 0; num--)
        //    {
        //        IStackSequence stackSequence = base.childSequences[num];
        //        base.Combat.MessageCenter.PublishMessage(new SequenceCompleteMessage(stackSequence));
        //        base.childSequences.RemoveAt(num);
        //        stackSequence.OnComplete();
        //    }
        //}

        //public override void CompleteOrders()
        //{
        //    base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.FiredWeaponsAntiStealthEffect, $"{base.SequenceGUID}_AntiStealth", base.SequenceGUID, owningActor, owningActor, default(WeaponHitInfo), -1);
        //    base.Combat.FlagECMStateNeedsRefreshing();
        //    owningActor.OnAttackComplete();
        //}

        //private void StripECMEffects(AbstractActor Target)
        //{
        //    foreach (Effect item in base.Combat.EffectManager.GetAllEffectsTargeting(Target).FindAll((Effect x) => x.EffectData.targetingData.auraEffectType == AuraEffectType.ECM_GENERAL))
        //    {
        //        Target.CancelEffect(item);
        //    }
        //    base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorLockAntiStealthEffect, $"{base.SequenceGUID}_AntiStealth", base.SequenceGUID, owningActor, Target, default(WeaponHitInfo), -1);
        //}

        //private void StripEvasivePips()
        //{
        //    base.Combat.MessageCenter.PublishMessage(new FloatieMessage(owningActor.GUID, Target.GUID, "SENSOR LOCKED", FloatieMessage.MessageNature.Debuff));
        //    if (Target.HasSensorLockEvasiveImmunity)
        //    {
        //        base.Combat.MessageCenter.PublishMessage(new FloatieMessage(owningActor.GUID, Target.GUID, "EVASION UNCHANGED", FloatieMessage.MessageNature.Buff));
        //        return;
        //    }
        //    int evasivePipsCurrent = Target.EvasivePipsCurrent;
        //    if (base.Combat.Constants.ToHit.SensorLockStripsEvasivePips)
        //    {
        //        for (int i = 0; i < base.Combat.Constants.ToHit.SensorLockPipsStripped; i++)
        //        {
        //            Target.ConsumeEvasivePip();
        //        }
        //    }
        //    int evasivePipsCurrent2 = Target.EvasivePipsCurrent;
        //    if (evasivePipsCurrent2 < evasivePipsCurrent)
        //    {
        //        int num = evasivePipsCurrent - evasivePipsCurrent2;
        //        base.Combat.MessageCenter.PublishMessage(new FloatieMessage(Target.GUID, Target.GUID, Strings.T("-{0} EVASION", num), FloatieMessage.MessageNature.Debuff));
        //    }
        //}

        //public override void OnAdded()
        //{
        //    base.OnAdded();
        //    RadarPingIndicator.Instance.Ping(Target.CurrentPosition);
        //    SetCamera(CameraControl.Instance.ShowSensorLockCam(Target, 1f * (float)(base.Combat.Constants.Visibility.NumSensorLockSteps + 1) * 0.8f), base.MessageIndex);
        //    CameraControl.Instance.ClearTargets();
        //    WwiseManager.PostEvent(AudioEventList_ui.ui_target_sensor_lock_hard, WwiseManager.GlobalAudioObject);
        //}

        //private void FireWave()
        //{
        //    List<Effect> list = base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorLockSingleStepEffect, $"{base.SequenceGUID}_SensorLock", base.SequenceGUID, owningActor, Target, default(WeaponHitInfo), -1);
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        Target.ProcessAddedMark(list[i]);
        //    }
        //    base.Combat.MessageCenter.PublishMessage(new FloatieMessage(owningActor.GUID, Target.GUID, base.Combat.Constants.Visibility.SensorsImpairedEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
        //    for (int j = 0; j < base.Combat.Constants.Visibility.NumSensorLockImpairedEffects; j++)
        //    {
        //        base.Combat.EffectManager.CreateEffect(base.Combat.Constants.Visibility.SensorsImpairedEffect, $"{base.SequenceGUID}_SensorsImpaired", base.SequenceGUID, owningActor, Target, default(WeaponHitInfo), -1);
        //    }
        //    StripECMEffects(Target);
        //    base.Combat.FlagECMStateNeedsRefreshing();
        //    if (Target.BehaviorTree != null && !Target.BehaviorTree.IsTargetIgnored(owningActor))
        //    {
        //        Target.LastTargetedPhaseNumber = base.Combat.TurnDirector.TotalElapsedPhases;
        //    }
        //    owningActor.UpdateVisibilityCache(base.Combat.GetAllCombatants());
        //    StripEvasivePips();
        //    numWavesFired++;
        //    timeSinceLastWave = 0f;
        //    if (!playedVO)
        //    {
        //        playedVO = true;
        //        if (owningActor.team.LocalPlayerControlsTeam)
        //        {
        //            AudioEventManager.PlayPilotVO(VOEvents.SensorLock_Ally, owningActor);
        //        }
        //        else
        //        {
        //            AudioEventManager.PlayPilotVO(VOEvents.SensorLock_Enemy, Target);
        //        }
        //    }
        //}

        //public override void OnUpdate()
        //{
        //    base.OnUpdate();
        //    if (base.OrdersAreComplete)
        //    {
        //        return;
        //    }
        //    timeSinceLastWave += Time.deltaTime;
        //    if (timeSinceLastWave > 1f)
        //    {
        //        if (numWavesFired < base.Combat.Constants.Visibility.NumSensorLockSteps)
        //        {
        //            FireWave();
        //        }
        //        else
        //        {
        //            base.OrdersAreComplete = true;
        //        }
        //    }
        //}

        //public override int Size()
        //{
        //    return base.Size();
        //}

        //public override bool ShouldSave()
        //{
        //    return base.ShouldSave();
        //}

        //public override void Save(SerializationStream stream)
        //{
        //    base.Save(stream);
        //}

        //public override void Load(SerializationStream stream)
        //{
        //    base.Load(stream);
        //}

        //public override void LoadComplete()
        //{
        //    base.LoadComplete();
        //}
    }
}

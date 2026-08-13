using HBS.Math;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Sequence
{
    internal class UDSStrafeSequence : MultiSequence
    {
        private enum SequenceState
        {
            None,
            Incoming,
            Strafing,
            Finished
        }

        private Vector3 zeroStartPos;

        private Vector3 zeroEndPos;

        private SequenceState state;

        private float timeInCurrentState;

        private float timeSinceLastAttack;

        private const float speed = 150f;

        private const float timeIncoming = 6f;

        private const float horizMultiplier = 4f;

        private const float timeBetweenAttacks = 0.35f;

        public AbstractActor Attacker { get; private set; }

        public Vector3 StartPos { get; private set; }

        public Vector3 EndPos { get; private set; }

        public float Radius { get; private set; }

        private List<AbstractActor> AllTargets { get; set; }

        private List<Weapon> StrafeWeapons { get; set; }

        private float HeightOffset { get; set; }

        private float MinWeaponRange { get; set; }

        private Vector3 Velocity { get; set; }

        public override bool IsValidMultiSequenceChild => false;

        public override bool IsParallelInterruptable => false;

        public override bool IsCancelable => false;

        public override bool IsComplete => state == SequenceState.Finished;

        public UDSStrafeSequence(AbstractActor attacker, Vector3 positionA, Vector3 positionB, float radius)
            : base(attacker.Combat)
        {
            Attacker = attacker;
            StartPos = positionA;
            EndPos = positionB;
            Radius = radius;
            state = SequenceState.None;
            Mod.Log.Info?.Log($"Created UDSStrafeSequence with source: {attacker?.DisplayName} at startPosition: {StartPos} and endPos: {EndPos} with radius: {radius}");
        }

        private void SetState(SequenceState newState)
        {
            if (state == newState)
            {
                return;
            }
            state = newState;
            timeInCurrentState = 0f;
            switch (newState)
            {
                case SequenceState.Incoming:
                    {
                        zeroStartPos = StartPos;
                        zeroStartPos.y = 0f;
                        zeroEndPos = EndPos;
                        zeroEndPos.y = 0f;
                        CalcTargets();
                        GetWeaponsForStrafe();
                        Vector3 vector = zeroEndPos - zeroStartPos;
                        vector.Normalize();
                        Velocity = vector * 150f;
                        Vector3 position = CalcStartPos();
                        Quaternion rotation = Quaternion.LookRotation(vector);
                        Quaternion rotation2 = Quaternion.LookRotation(Vector3.forward * 5f + Vector3.down * 1f);
                        SetPosition(position, rotation);
                        SetCamera(CameraControl.Instance.ShowActorCam(Attacker, rotation2, 30f), base.MessageIndex);
                        Mod.Log.Trace?.Log($"UDSStrafeSequence::Incoming");
                        break;
                    }
                case SequenceState.Strafing:
                    ClearCamera();
                    AudioEventManager.PlayRandomPilotVO(VOEvents.AirstrikeLaunched_Ally, base.Combat.LocalPlayerTeam, base.Combat.LocalPlayerTeam.units);
                    Mod.Log.Trace?.Log($"UDSStrafeSequence::Strafing");
                    break;
                case SequenceState.Finished:
                    {
                        UDSFlyAwaySequence sequence = new(Attacker, Velocity, 150f);
                        base.Combat.MessageCenter.PublishMessage(new AddParallelSequenceToStackMessage(sequence));
                        Mod.Log.Trace?.Log($"UDSStrafeSequence::Finished");
                        break;
                    }
            }
        }

        private void Update()
        {
            timeInCurrentState += Time.deltaTime;
            switch (state)
            {
                case SequenceState.Incoming:
                    if (Vector3.Distance(Attacker.CurrentPosition, StartPos) < MinWeaponRange)
                    {
                        SetState(SequenceState.Strafing);
                    }
                    break;
                case SequenceState.Strafing:
                    if (Vector3.Distance(Attacker.CurrentPosition, EndPos) < MinWeaponRange)
                    {
                        SetState(SequenceState.Finished);
                    }
                    break;
            }
            switch (state)
            {
                case SequenceState.Incoming:
                    SetPosition(Attacker.CurrentPosition + Velocity * Time.deltaTime, Attacker.CurrentRotation);
                    break;
                case SequenceState.Strafing:
                    SetPosition(Attacker.CurrentPosition + Velocity * Time.deltaTime, Attacker.CurrentRotation);
                    AttackNextTarget();
                    break;
                case SequenceState.Finished:
                    break;
            }
        }

        private void SetPosition(Vector3 position, Quaternion rotation)
        {
            Attacker.GameRep.thisTransform.position = position;
            Attacker.GameRep.thisTransform.rotation = rotation;
            Attacker.OnPositionUpdate(position, rotation, base.SequenceGUID, updateDesignMask: false, null, skipAbilityLogging: true);
        }

        private void CalcTargets()
        {
            AllTargets = new List<AbstractActor>();
            List<AbstractActor> allActors = base.Combat.AllActors;
            for (int i = 0; i < allActors.Count; i++)
            {
                if (IsTarget(allActors[i]))
                {
                    AllTargets.Add(allActors[i]);
                }
            }
            Vector3 preStartPos = EndPos - StartPos * 2f;
            AllTargets.Sort((AbstractActor x, AbstractActor y) => Vector3.Distance(y.CurrentPosition, preStartPos).CompareTo(Vector3.Distance(x.CurrentPosition, preStartPos)));
        }

        private bool IsTarget(AbstractActor actor)
        {
            Vector3 currentPosition = actor.CurrentPosition;
            Vector3 vector = NvMath.NearestPointStrict(StartPos, EndPos, currentPosition);
            vector.y = base.Combat.MapMetaData.GetLerpedHeightAt(vector);
            return Vector3.Distance(vector, currentPosition) < Radius;
        }

        private void GetWeaponsForStrafe()
        {
            StrafeWeapons = Attacker.Weapons.FindAll((Weapon x) => x.WeaponCategoryValue.IsEnergy);
            if (StrafeWeapons.Count == 0)
            {
                CombatGameState.gameInfoLogger.LogError("ERROR!! No weapons found for strafing run.");
                return;
            }
            StrafeWeapons.Sort((Weapon x, Weapon y) => y.MaxRange.CompareTo(x.MaxRange));
        }

        private Vector3 CalcStartPos()
        {
            Vector3 result = StartPos - Velocity * 6f;
            MinWeaponRange = StrafeWeapons[0].MaxRange;
            HeightOffset = MinWeaponRange / 4f;
            result.y += HeightOffset;
            return result;
        }

        private void AttackNextTarget()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (!(timeSinceLastAttack > 0.35f) || base.Combat.AttackDirector.IsAnyAttackSequenceActive)
            {
                return;
            }
            while (AllTargets.Count > 0 && !(Vector3.Distance(Attacker.CurrentPosition, AllTargets[0].CurrentPosition) > MinWeaponRange * 0.95f))
            {
                if (!Attacker.HasLOFToTargetUnit(AllTargets[0], StrafeWeapons[0]))
                {
                    AllTargets.RemoveAt(0);
                    continue;
                }
                CombatGameState.gameInfoLogger.LogWarning("attacking");
                AttackDirector attackDirector = base.Combat.AttackDirector;
                AttackDirector.AttackSequence attackSequence = attackDirector.CreateAttackSequence(base.SequenceGUID, Attacker, AllTargets[0], Attacker.CurrentPosition, Attacker.CurrentRotation, AllTargets.Count, StrafeWeapons, MeleeAttackType.NotSet, 0, isMoraleAttack: false);
                attackSequence.ResetWeapons();
                attackDirector.PerformAttack(attackSequence);
                AllTargets.RemoveAt(0);
                timeSinceLastAttack = 0f;
                break;
            }
        }

        public override void OnAdded()
        {
            base.OnAdded();
            SetState(SequenceState.Incoming);
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

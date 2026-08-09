using HBS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ErosionBrushPlugin.Matrix;

namespace UsedDropshipSalesman.Sequence
{
    internal class UDSFlyAwaySequence : MultiSequence
    {
        public enum SequenceState
        {
            None,
            FlyingAway,
            Finished
        }

        public SequenceState state;

        public float timeInCurrentState;

        public float minMapCoord = -1200f;

        public float maxMapCoord = 1200f;

        public float lift = 5f;

        public float thrust = 50f;

        public Vector3 velocity;

        public AbstractActor actor { get; set; }

        public Vector3 startDirection { get; set; }

        public float speed { get; set; }

        public float heightOffset { get; set; }

        public float minWeaponRange { get; set; }

        public bool IsOffMap
        {
            get
            {
                if (actor.CurrentPosition.x < minMapCoord || actor.CurrentPosition.x > maxMapCoord)
                {
                    if (!(actor.CurrentPosition.z < minMapCoord))
                    {
                        return actor.CurrentPosition.z > maxMapCoord;
                    }

                    return true;
                }

                return false;
            }
        }

        public override bool IsValidMultiSequenceChild => false;

        public override bool IsParallelInterruptable => false;

        public override bool IsCancelable => false;

        public override bool IsComplete => state == SequenceState.Finished;

        public UDSFlyAwaySequence(AbstractActor actor, Vector3 directionStart, float speed)
            : base(actor.Combat)
        {
            this.actor = actor;
            startDirection = directionStart;
            this.speed = speed;
            state = SequenceState.None;
            Mod.Log.Info?.Log($"Created UDSFlyAwaySequence with actor: {actor?.DisplayName} at startDirection: {directionStart} and speed: {speed}");
        }

        public void SetState(SequenceState newState)
        {
            if (state != newState)
            {
                state = newState;
                timeInCurrentState = 0f;
                switch (newState)
                {
                    case SequenceState.FlyingAway:
                        {
                            Vector3 vector = startDirection;
                            vector.Normalize();
                            velocity = vector * speed;
                            Mod.Log.Trace?.Log($"UDSArtillerySequence::FlyingAway");
                            break;
                        }
                    case SequenceState.Finished:
                        CleanupAttacker();
                        Mod.Log.Trace?.Log($"UDSArtillerySequence::Finished");
                        break;
                }
            }
        }

        private void CleanupAttacker()
        {
            actor.PlaceFarAwayFromMap();
            actor.GetPilot()?.KillPilot(Combat.Constants, "", 0, DamageType.Unknown, null, null);
            actor.FlagForDeath("Death after strike!", DeathMethod.DespawnedNoMessage, DamageType.Unknown, -1, -1, "", isSilent: true);
            actor.HandleDeath("0");
        }

        public void Update()
        {
            timeInCurrentState += Time.deltaTime;
            switch (state)
            {
                case SequenceState.FlyingAway:
                    if (IsOffMap)
                    {
                        SetState(SequenceState.Finished);
                    }

                    break;
            }

            SequenceState sequenceState = state;
            if (sequenceState != SequenceState.FlyingAway)
            {
                _ = 2;
                return;
            }

            Vector3 vector = velocity;
            vector.y = 0f;
            velocity += vector.normalized * thrust * Time.deltaTime;
            velocity.y += Time.deltaTime * lift;
            SetPosition(actor.CurrentPosition + velocity * Time.deltaTime, actor.CurrentRotation);
        }

        public void SetPosition(Vector3 position, Quaternion rotation)
        {
            actor.GameRep.thisTransform.position = position;
            actor.GameRep.thisTransform.rotation = rotation;
            actor.OnPositionUpdate(position, rotation, base.SequenceGUID, updateDesignMask: false, null, skipAbilityLogging: true);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            SetState(SequenceState.FlyingAway);
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

using HBS.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UsedDropshipSalesman.Sequence
{
    internal class UDSArtillerySequence : MultiSequence
    {

        private enum SequenceState
        {
            None,
            Incoming,
            Shelling,
            ShowingEffects,
            Finished
        }

        private SequenceState state;

        private float timeInCurrentState;

        private float timeSinceLastAttack;

        private const float timeIncoming = 3f;

        private const float timeBetweenAttacks = 0.25f;

        private const float timeShowingEffects = 2f;

        private float timeSinceLastSound;

        private float timeBetweenSounds;

        private float minTimeBetweenExplosions = 0.0625f;

        private float maxTimeBetweenExplosions = 0.125f;

        private ObjectSpawnData osd;

        public string TeamGUID { get; private set; }

        public Turret Attacker { get; private set; }

        public Vector3 TargetPos { get; private set; }

        public float Radius { get; private set; }

        private List<ICombatant> AllTargets { get; set; }

        public string ExplodeFX { get; private set; }

        public override bool IsValidMultiSequenceChild => false;

        public override bool IsParallelInterruptable => false;

        public override bool IsCancelable => false;

        public override bool IsComplete => state == SequenceState.Finished;

        public UDSArtillerySequence(CombatGameState combat, string teamGUID, Turret attacker, string explodeFX, Vector3 targetPos, float radius)
            : base(combat)
        {
            Attacker = attacker;
            ExplodeFX = explodeFX;
            TargetPos = targetPos;
            Radius = radius;
            TeamGUID = base.Combat.LocalPlayerTeamGuid;
            state = SequenceState.None;
            Mod.Log.Info?.Log($"Created UDSArtillerySequence with source: {attacker?.DisplayName} at pos: {targetPos} with radius: {radius}");
        }

        private void SetState(SequenceState newState)
        {
            if (state == newState) return; // Nothing to do

            state = newState;
            timeInCurrentState = 0f;
            switch (newState)
            {
                case SequenceState.Incoming:
                    CollectTargets();
                    SetCamera(CameraControl.Instance.ShowRandomizedFocalCam(TargetPos, CameraControl.Instance.MinTerrainHeight, CameraControl.Instance.MaxTerrainHeight * 0.5f, 3f), base.SequenceGUID);
                    AudioEventManager.PlayRandomPilotVO(VOEvents.ArtilleryLaunched_Ally, base.Combat.LocalPlayerTeam, base.Combat.LocalPlayerTeam.units);
                    Mod.Log.Trace?.Log($"UDSArtillerySequence::Incoming");
                    break;
                case SequenceState.Shelling:
                    PlayExplosionFX();
                    Mod.Log.Trace?.Log($"UDSArtillerySequence::Shelling");
                    break;
                case SequenceState.ShowingEffects:
                    Mod.Log.Trace?.Log($"UDSArtillerySequence::ShowingEffects");
                    break;
                case SequenceState.Finished:
                    ClearCamera();
                    CleanupAttacker();
                    Mod.Log.Trace?.Log($"UDSArtillerySequence::Finished");
                    break;
            }
        }

        private void CleanupAttacker()
        {
            Attacker.PlaceFarAwayFromMap();
            Attacker.GetPilot()?.KillPilot(Combat.Constants, "", 0, DamageType.Unknown, null, null);
            Attacker.FlagForDeath("Death after strike!", DeathMethod.Unknown, DamageType.Unknown, -1, -1, "", isSilent: true);
            Attacker.HandleDeath("0");
        }

        private void Update()
        {
            timeInCurrentState += Time.deltaTime;
            switch (state)
            {
                case SequenceState.Incoming:
                    if (timeInCurrentState > 3f)
                    {
                        SetState(SequenceState.Shelling);
                    }
                    timeSinceLastSound += Time.deltaTime;
                    if (timeSinceLastSound >= timeBetweenSounds)
                    {
                        PlaySoundEffect(); // TODO: SHOULD BE INCOMING SOUND EFFECT, NOT EXPLOSION
                    }                    
                    break;
                case SequenceState.Shelling:
                    if (AllTargets.Count == 0)
                    {
                        SetState(SequenceState.ShowingEffects);
                        break;
                    }
                    timeSinceLastAttack += Time.deltaTime;
                    if (timeSinceLastAttack > 0.25f && AllTargets.Count > 0)
                    {
                        ICombatant combatant = AllTargets[0];
                        AllTargets.Remove(combatant);
                        PerformAttack(combatant);
                        timeSinceLastAttack = 0f;
                    }
                    break;
                case SequenceState.ShowingEffects:
                    if (timeInCurrentState > 2f)
                    {
                        SetState(SequenceState.Finished);
                    }
                    break;
                case SequenceState.Finished:
                    base.Combat.MessageCenter.PublishMessage(new OnArtillerySequenceCompleteMessage());
                    break;

            }

        }

        private void PlayExplosionFX()
        {
            Mod.Log.Trace?.Log($"UDSArtillerySequence showing explodeFX: {ExplodeFX} at targetPos: {TargetPos}");
            osd = new ObjectSpawnData(ExplodeFX, TargetPos, Quaternion.identity, playFX: true, autoPoolObject: true);
            try
            {
                osd.Spawn(base.Combat);
                PlaySoundEffect();
            }
            catch (Exception ex)
            {
                Mod.Log.Trace?.Log($"Failed to spawn the OSD", ex);

            }
        }

        private void PlaySoundEffect()
        {
            //float cameraShakeIntensity = 10f *
            //    (ArtilleryWeapon.DamagePerShot * (float)ArtilleryWeapon.ShotsWhenFired) *
            //    base.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageRelativeMod +
            //    base.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageAbsoluteMod;
            float cameraShakeIntensity = 2000f; // Based off AC20
            Mod.Log.Trace?.Log($"UDSArtillerySequence cameraShakeIntensity of: {cameraShakeIntensity}");
            CameraControl.Instance.AddCameraShake(cameraShakeIntensity, 2f, TargetPos);
            timeSinceLastSound = 0f;
            timeBetweenSounds = UnityEngine.Random.Range(minTimeBetweenExplosions, maxTimeBetweenExplosions);

            if (osd != null)
            {
                GameObject spawnedObject = osd.spawnedObject;
                if (spawnedObject != null)
                {
                    AkGameObj akGameObj = spawnedObject.GetComponent<AkGameObj>() ?? spawnedObject.AddComponent<AkGameObj>();
                    WwiseManager.PostEvent(AudioEventList_explosion.explosion_large, akGameObj);
                    Mod.Log.Trace?.Log($"UDSArtillerySequence playing large exposion noise on GO: {spawnedObject?.name}");
                }
            }
        }

        private void CollectTargets()
        {
            AllTargets = new List<ICombatant>();
            List<AbstractActor> allActors = base.Combat.AllActors;
            foreach (AbstractActor actor in allActors) 
            {
                if (actor.IsDead || actor.IsFlaggedForDeath) continue; // Don't target
                float distance = Vector3.Distance(TargetPos, actor.currentPosition);
                if (distance < Radius)
                {
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is within radius of attack, adding to targets.");
                    AllTargets.Add(actor);
                }
                else
                {
                    Mod.Log.Debug?.Log($"Actor {actor?.DisplayName} is outside radius of attack, skipping.");
                }
            }
        }

        private void PerformAttack(ICombatant target)
        {

            foreach (Weapon weapon in Attacker.Weapons)
            {
                try
                {
                    Mod.Log.Info?.Log($"Attacking combatant: {target?.DisplayName} with weapon: {weapon?.Name} from parent: {Attacker?.DisplayName}");
                    weapon.PreFireWeapon(base.SequenceGUID); // Prevent the weapon ahs not prefired error
                    int totalHits = weapon.ShotsWhenFired;
                    WeaponHitInfo hitInfo = new WeaponHitInfo
                    {
                        attackerId = Attacker.GUID,
                        targetId = target.GUID,
                        numberOfShots = totalHits,
                        stackItemUID = base.SequenceGUID,
                        locationRolls = new float[totalHits],
                        hitLocations = new int[totalHits]
                    };

                    AttackDirection attackDirection = base.Combat.HitLocation.GetAttackDirection(TargetPos, target);
                    hitInfo.attackDirections = new AttackDirection[totalHits];
                    hitInfo.hitQualities = new AttackImpactQuality[totalHits];
                    hitInfo.locationRolls = base.Combat.AttackDirector.GetRandomFromCache(hitInfo, hitInfo.numberOfShots);
                    hitInfo.hitVariance = base.Combat.AttackDirector.GetVarianceSumsFromCache(hitInfo, hitInfo.numberOfShots, weapon);
                    Mod.Log.Debug?.Log($"Resolved individual hit locations against combatant: {target?.DisplayName}");
                    for (int hitIndex = 0; hitIndex < totalHits; hitIndex++)
                    {
                        hitInfo.attackDirections[hitIndex] = attackDirection;
                        hitInfo.hitQualities[hitIndex] = AttackImpactQuality.Solid;
                        hitInfo.hitLocations[hitIndex] = target.GetHitLocation(weapon.parent, TargetPos, hitInfo.locationRolls[hitIndex], 0, 1f);

                        Mod.Log.Debug?.Log($" -- Applying hit to location: {hitInfo.hitLocations[hitIndex]}");
                        target.TakeWeaponDamage(hitInfo, hitInfo.hitLocations[hitIndex], weapon, weapon.DamagePerShot, 0f, hitIndex, DamageType.Artillery);

                    }

                    Mod.Log.Debug?.Log($"Resolving weapon damage for sequence: {hitInfo.attackSequenceId}");
                    target.ResolveWeaponDamage(hitInfo, weapon, MeleeAttackType.NotSet);
                    target.HandleDeath("Artillery");
                }
                catch (Exception ex)
                {
                    Mod.Log.Warning?.Log("Failed to perform attack from UDSArtillerySequence!", ex);
                }
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

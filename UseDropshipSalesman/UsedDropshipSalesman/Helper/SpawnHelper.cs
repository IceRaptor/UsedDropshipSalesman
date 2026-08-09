using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Helper
{
    internal class SpawnHelper
    {
        internal static Lance GetSupportLance(CombatGameState combat)
        {
            if (ModState.SeqSupportLance == null)
            {
                Team team = combat.LocalPlayerTeam;
                Lance lance = new(team, new BattleTech.Framework.LanceSpawnerRef[] { });
                Guid g = Guid.NewGuid();
                string lanceGuid = LanceSpawnerGameLogic.GetLanceGuid(g.ToString());
                lance.lanceGuid = lanceGuid;
                team.Combat.ItemRegistry.AddItem(lance);
                team.lances.Add(lance);

                Mod.Log.Debug?.Log($"Created new support lance with GUID: {lance.lanceGuid} for team: {team}");
                ModState.SeqSupportLance = lance;
            }

            return ModState.SeqSupportLance;
        }

        internal static Vehicle CreateVehicleForSequence(CombatGameState combat, string vehicleDefId, string pilotDefId)
        {
            // TODO: Pull from mod stats
            VehicleDef vehicleDef = combat.DataManager.VehicleDefs.Get(vehicleDefId);
            PilotDef pilotDef = combat.DataManager.PilotDefs.Get(pilotDefId);
            Mod.Log.Debug?.Log($"Refreshing {vehicleDefId} with pilotDef: {pilotDef}");
            vehicleDef.Refresh();

            Team team = combat.LocalPlayerTeam;
            Vehicle vehicle = ActorFactory.CreateVehicle(vehicleDef, pilotDef, team.EncounterTags, team.Combat, team.GetNextSupportUnitGuid(), "", null);
            if (vehicle == null)
            {
                Mod.Log.Error?.Log($"Failed to spawn vehicleDefId: {vehicleDefId} / pilotDefId: pilot_d10_sharpshooter !");
            }
            else
            {
                Mod.Log.Debug?.Log($"Created vehicle");
            }

            vehicle.Init(Vector3.zero, 0f, false);
            Mod.Log.Debug?.Log($"Initted vehicle");
            vehicle.InitGameRep(null);
            Mod.Log.Debug?.Log($"Initted gameRep");

            Mod.Log.Debug?.Log($"Adding vehicle to team and support units");
            //team.AddUnit(vehicle);
            team.SupportUnits.Add(vehicle);

            Mod.Log.Debug?.Log($"Adding team and lance to vehicle");
            vehicle.AddToTeam(team);
            vehicle.AddToLance(GetSupportLance(combat));

            Mod.Log.Debug?.Log($"Adding behavior tree");
            vehicle.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(team.Combat.BattleTechGame, vehicle, BehaviorTreeIDEnum.CoreAITree);

            Mod.Log.Debug?.Log("Moving unit off map");
            //vehicle.PlaceFarAwayFromMap();
            vehicle.GameRep.transform.position = team.OffScreenPosition;
            vehicle.OnPositionUpdate(team.OffScreenPosition, vehicle.CurrentRotation, -1, updateDesignMask: true, null);
            Mod.Log.Debug?.Log($"Vehicle moved to offMap position: {team.OffScreenPosition} with currentRotation: {vehicle.CurrentRotation}");

            return vehicle;
        }

        internal static Turret CreateTurretForSequence(CombatGameState combat, string turretDefId, string pilotDefId)
        {
            // TODO: Pull from mod stats
            TurretDef turretDef = combat.DataManager.TurretDefs.Get(turretDefId);
            PilotDef pilotDef = combat.DataManager.PilotDefs.Get(pilotDefId);
            Mod.Log.Debug?.Log($"Refreshing {turretDefId} with pilotDef: {pilotDef}");
            turretDef.Refresh();

            Team team = combat.LocalPlayerTeam;
            Turret turret = ActorFactory.CreateTurret(turretDef, pilotDef, team.EncounterTags, team.Combat, team.GetNextSupportUnitGuid(), "", null);
            if (turret == null)
            {
                Mod.Log.Error?.Log($"Failed to spawn vehicleDefId: {turretDefId} / pilotDefId: pilot_d10_sharpshooter !");
            }
            else
            {
                Mod.Log.Debug?.Log($"Created vehicle");
            }

            turret.Init(Vector3.zero, 0f, false);
            Mod.Log.Debug?.Log($"Initted turret");
            turret.InitGameRep(null);
            Mod.Log.Debug?.Log($"Initted gameRep");

            //Mod.Log.Debug?.Log($"Adding vehicle to team and support units");
            team.AddUnit(turret);
            //team.SupportUnits.Add(vehicle);

            //Mod.Log.Debug?.Log($"Adding team and lance to vehicle");
            turret.AddToTeam(team);
            turret.AddToLance(GetSupportLance(combat));

            Mod.Log.Debug?.Log($"Adding behavior tree");
            turret.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(team.Combat.BattleTechGame, turret, BehaviorTreeIDEnum.CoreAITree);

            Mod.Log.Debug?.Log("Moving unit off map");
            //turret.PlaceFarAwayFromMap();
            turret.GameRep.transform.position = team.OffScreenPosition;
            turret.OnPositionUpdate(team.OffScreenPosition, turret.CurrentRotation, -1, updateDesignMask: true, null);
            Mod.Log.Debug?.Log($"Turret moved to offMap position: {team.OffScreenPosition} with currentRotation: {turret.CurrentRotation}");

            return turret;
        }

        internal static void SpawnFlares(CombatGameState combat, AbilityDef sourceAbility, Vector3 positionA, Vector3 positionB, 
            string prefabName, int numFlares, int numPhases)
        {
            Vector3 spaceBetweenFlares = (positionB - positionA) / numFlares;
            Vector3 startFlarePos = positionA;
            startFlarePos.y = combat.MapMetaData.GetLerpedHeightAt(startFlarePos);
            List<ObjectSpawnData> list = new();
            for (int i = 0; i < numFlares; i++)
            {
                ObjectSpawnData item = new(prefabName, startFlarePos, Quaternion.identity, playFX: true, autoPoolObject: false);
                list.Add(item);
                startFlarePos += spaceBetweenFlares;
                startFlarePos.y = combat.MapMetaData.GetLerpedHeightAt(startFlarePos);
            }

            SpawnObjectSequence spawnObjectSequence = new(combat, list);
            combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(spawnObjectSequence));
            List<ObjectSpawnData> spawnedObjects = spawnObjectSequence.spawnedObjects;
            CleanupObjectSequence eventSequence = new(combat, spawnedObjects);

            TurnEvent tEvent = new(GUIDFactory.GetGUID(), combat, numPhases, null, eventSequence, sourceAbility, showInPhaseTrack: false);
            combat.TurnDirector.AddTurnEvent(tEvent);
        }

    }
}

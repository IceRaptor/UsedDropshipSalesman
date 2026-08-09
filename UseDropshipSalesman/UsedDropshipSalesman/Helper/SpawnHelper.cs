using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Helper
{
    internal class SpawnHelper
    {
        internal static Turret WeaponsTurret;

        internal static Lance CreateSupportLance(Team team)
        {
            Lance lance = new(team, new BattleTech.Framework.LanceSpawnerRef[] { });
            Guid g = Guid.NewGuid();
            string lanceGuid = LanceSpawnerGameLogic.GetLanceGuid(g.ToString());
            lance.lanceGuid = lanceGuid;
            team.Combat.ItemRegistry.AddItem(lance);
            team.lances.Add(lance);

            Mod.Log.Error?.Log($"Created new support lance with GUID: {lance.lanceGuid} fo team: {team}");
            return lance;
        }

        internal static void CreateVehicleSupportResource(Team team, Lance lance, string vehicleDefId)
        {
            // TODO: Pull from mod stats
            PilotDef pilotDef = team.combat.DataManager.PilotDefs.Get("pilot_d10_sharpshooter");
            VehicleDef vehicleDef = team.combat.DataManager.VehicleDefs.Get(vehicleDefId);
            Mod.Log.Debug?.Log($"Refreshing {vehicleDefId} with pilotDef: {pilotDef}");
            vehicleDef.Refresh();

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
            vehicle.AddToLance(lance);

            Mod.Log.Debug?.Log($"Adding behavior tree");
            vehicle.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(team.Combat.BattleTechGame, vehicle, BehaviorTreeIDEnum.CoreAITree);

            Mod.Log.Debug?.Log("Moving unit off map");
            //vehicle.PlaceFarAwayFromMap();
            vehicle.GameRep.transform.position = team.OffScreenPosition;
            vehicle.OnPositionUpdate(team.OffScreenPosition, vehicle.CurrentRotation, -1, updateDesignMask: true, null);
            Mod.Log.Debug?.Log($"Vehicle moved to offMap position: {team.OffScreenPosition} with currentRotation: {vehicle.CurrentRotation}");
        }

        internal static Turret CreateTurret(Team team, Lance lance, string turretDefId)
        {
            // TODO: Pull from mod stats
            PilotDef pilotDef = team.combat.DataManager.PilotDefs.Get("pilot_d10_sharpshooter");
            TurretDef turretDef = team.combat.DataManager.TurretDefs.Get(turretDefId);
            Mod.Log.Debug?.Log($"Refreshing {turretDefId} with pilotDef: {pilotDef}");
            turretDef.Refresh();

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
            turret.AddToLance(lance);

            Mod.Log.Debug?.Log($"Adding behavior tree");
            turret.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(team.Combat.BattleTechGame, turret, BehaviorTreeIDEnum.CoreAITree);

            Mod.Log.Debug?.Log("Moving unit off map");
            //turret.PlaceFarAwayFromMap();
            turret.GameRep.transform.position = team.OffScreenPosition;
            turret.OnPositionUpdate(team.OffScreenPosition, turret.CurrentRotation, -1, updateDesignMask: true, null);
            Mod.Log.Debug?.Log($"Turret moved to offMap position: {team.OffScreenPosition} with currentRotation: {turret.CurrentRotation}");

            return turret;
        }

        internal static void CreateWeaponSupportResource(Team team, Lance lance, string weaponDefId)
        {

            // CU requires a parent turret + pilot or throws an NRE - see Weapon_Constructor_Turret
            if (WeaponsTurret == null)
            {
                WeaponsTurret = CreateWeaponsTurret(team, lance);
            }
            WeaponDef weaponDef = team.combat.DataManager.WeaponDefs.Get(weaponDefId);

            TurretComponentRef turretComponentRef = new()
            {
                Def = weaponDef
            };

            Weapon weapon = new(WeaponsTurret, team.combat, turretComponentRef, team.GetNextSupportUnitGuid());
            weapon.Init();
            weapon.InitStats();

            Mod.Log.Debug?.Log($"Added weapon: {weaponDefId} to team: {team}'s support weapons");
            team.SupportWeapons.Add(weapon);
        }

        private static Turret CreateWeaponsTurret(Team team, Lance lance)
        {

            PilotDef pilotDef = team.combat.DataManager.PilotDefs.Get("pilot_d10_sharpshooter");
            // TODO: Make this a mod config value
            Mod.Log.Info?.Log($"== ITERATING TURRETS");
            foreach (KeyValuePair<string, TurretDef> kvp in team.combat.DataManager.TurretDefs)
            {
                Mod.Log.Info?.Log($" -- found Def with key: {kvp.Key} with desc.id: {kvp.Value?.Description?.Id}");
            }

            TurretDef turretDef = team.combat.DataManager.TurretDefs.Get("turretdef_Standard_Sniper");
            Mod.Log.Debug?.Log($"Refreshing {turretDef} with pilotDef: {pilotDef}");
            turretDef.Refresh();

            Turret turret = ActorFactory.CreateTurret(turretDef, pilotDef, team.EncounterTags, team.Combat, team.GetNextSupportUnitGuid(), "", null);
            if (turret == null)
            {
                Mod.Log.Error?.Log($"Failed to spawn turretDef: {turretDef} / pilotDefId: pilot_d10_sharpshooter !");
            }
            else
            {
                Mod.Log.Debug?.Log($"Created turret");
            }

            turret.Init(Vector3.zero, 0f, false);
            Mod.Log.Debug?.Log($"Initted turret");
            turret.InitGameRep(null);
            Mod.Log.Debug?.Log($"Initted gameRep");

            Mod.Log.Debug?.Log($"Adding turret to team and support units");
            //team.AddUnit(turret);
            team.SupportUnits.Add(turret);

            Mod.Log.Debug?.Log($"Adding team and lance to turret");
            turret.AddToTeam(team);
            turret.AddToLance(lance);

            Mod.Log.Debug?.Log($"Adding behavior tree");
            turret.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(team.Combat.BattleTechGame, turret, BehaviorTreeIDEnum.CoreAITree);

            Mod.Log.Debug?.Log("Moving unit off map");
            //turret.PlaceFarAwayFromMap();
            turret.GameRep.transform.position = team.OffScreenPosition;
            turret.OnPositionUpdate(team.OffScreenPosition, turret.CurrentRotation, -1, updateDesignMask: true, null);
            Mod.Log.Debug?.Log($"Unit moved to offMap position: {team.OffScreenPosition} with currentRotation: {turret.CurrentRotation}");
            return turret;
        }
    }
}

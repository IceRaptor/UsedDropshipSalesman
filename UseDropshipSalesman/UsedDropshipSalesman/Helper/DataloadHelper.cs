using BattleTech.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UsedDropshipSalesman.Helper
{
    internal class DataloadHelper
    {


        internal static void LoadSupportResources(Team team, List<string> mechDefs, List<string> vehicleDefs, 
            List<string> turretDefs, List<string> weaponDefs)
        {
            // Load the necessary turret defs
            Mod.Log.Info?.Log($"== BEGIN load request for support elements");
            LoadRequest asyncSpawnReq = team.combat.DataManager.CreateLoadRequest(
                delegate (LoadRequest loadRequest) { OnLoadComplete(team, vehicleDefs, weaponDefs); }, false
                );
            Mod.Log.Info?.Log($" -- Pre-load counts => weaponDefs: {team.combat.DataManager.WeaponDefs.Count}  " +
                $"pilotDefs: {team.combat.DataManager.PilotDefs.Count}  mechDefs: {team.combat.DataManager.MechDefs.Count}" +
                $"turretDefs: {team.combat.DataManager.TurretDefs.Count}  vehicleDefs: {team.combat.DataManager.VehicleDefs.Count}");

            foreach (string defId in vehicleDefs)
            {
                Mod.Log.Info?.Log($"  - VehicleDefId: {defId}");
                asyncSpawnReq.AddBlindLoadRequest(BattleTechResourceType.VehicleDef, defId, new bool?(false));
                // TODO: Make a mod option
                asyncSpawnReq.AddBlindLoadRequest(BattleTechResourceType.PilotDef, "pilot_d10_sharpshooter", new bool?(false));
            }

            // TODO: HACK - force modders to load appropriate turrets
            asyncSpawnReq.AddBlindLoadRequest(BattleTechResourceType.TurretDef, "turretdef_Standard_Sniper", new bool?(false));

            foreach (string defId in weaponDefs)
            {
                Mod.Log.Info?.Log($"  - WeaponDefId: {defId}");
                asyncSpawnReq.AddBlindLoadRequest(BattleTechResourceType.WeaponDef, defId, new bool?(false));
            }

            // Fire the load request
            asyncSpawnReq.ProcessRequests(1000U);
        }

        private static void OnLoadComplete(Team team, List<string> vehicleDefs, List<string> weaponDefs)
        {
            Mod.Log.Info?.Log($"== END load request for support elements");
            Mod.Log.Info?.Log($" -- Post-load counts => weaponDefs: {team.combat.DataManager.WeaponDefs.Count}  " +
                $"pilotDefs: {team.combat.DataManager.PilotDefs.Count}  mechDefs: {team.combat.DataManager.MechDefs.Count}" +
                $"turretDefs: {team.combat.DataManager.TurretDefs.Count}  vehicleDefs: {team.combat.DataManager.VehicleDefs.Count}");
        }

        internal static void UnloadSupportResources(CombatGameState combat)
        {
            // TODO: Looks like data manager has no unload function, just a 'clear' function?
            // Possibly just set defs=true for clear... but would that screw up say salvage?
        }

    }
}

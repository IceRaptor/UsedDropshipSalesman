

using BattleTech.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UsedDropshipSalesman.Patches.UI
{

    [HarmonyPatch(typeof(CombatHUDButtonBase), "OnPointerExit")]
    [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
    static class CombatHUDButtonBase_OnPointerExit
    {
        static void Postfix(CombatHUDButtonBase __instance, PointerEventData eventData)
        {
            if (__instance == null) return;
            CombatHUDActionButton CHUDActionButton = __instance as CombatHUDActionButton;
            if (CHUDActionButton == null) return;
            if (String.IsNullOrEmpty(CHUDActionButton?.Ability?.Def?.Description?.Id)) return;
            if (!CHUDActionButton.Ability.Def.Description.Id.Contains("_UDS")) return;

            Mod.Log.Trace?.Log("==== CombatHUDButtonBase_OnPointerExit:Postfix- entered.");
            Mod.Log.Debug?.Log($"OnPointerExit  mouseHover: {__instance.mouseHover}  mouseDown: {__instance.mouseDown}  " +
                $"HUD.hoveredbutton == this? {__instance.HUD.HoveredButton == __instance}  " +
                $"abilityId: {CHUDActionButton.Ability.Def.Description.Id}");

            // Force a refresh of the dropship buttons
            CHUDActionButton.RefreshColors(__instance.HUD.SelectedActor);
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ExecuteClick")]
    [HarmonyPatch(new Type[] { })]
    static class CombatHUDActionButton_ExecuteClick
    {
        static void Postfix(CombatHUDActionButton __instance)
        {
            if (__instance == null) return;
            if (String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id)) return;
            if (!__instance.Ability.Def.Description.Id.Contains("_UDS")) return;

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_ExecuteClick:Postfix- entered.");

            // Allow each actor to only click once per turn
            __instance.isClickable = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "IsShowingTooltip", MethodType.Getter)]
    static class CombatHUDActionButton_IsShowingTooltip
    {
        static void Postfix(CombatHUDActionButton __instance, ref bool __result)
        {
            if (__instance == null || String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id)) return;
            if (!__instance.Ability.Def.Description.Id.Contains("_UDS_", StringComparison.InvariantCultureIgnoreCase)) return; // nothing to do

            // Should be a _UDS_ ability here

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_IsShowingTooltip:POSTFIX- entered.");
            Mod.Log.Debug?.Log($"IsShowingTooltip for abilityDef: {__instance.Ability.Def.Description.Id}  result: {__result}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "RefreshColors")]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ColorOverrides) })]
    static class CombatHUDActionButton_RefreshColors
    {
        static void Postfix(CombatHUDActionButton __instance, AbstractActor actor, ColorOverrides overrides = null)
        {
            if (__instance == null || __instance.Ability?.Def?.Description?.Id == null ) return;
            if (!__instance.Ability.Def.Description.Id.Contains("_UDS_", StringComparison.InvariantCultureIgnoreCase)) return; // nothing to do

            // Should be a _UDS_ ability here

            Mod.Log.Trace?.Log("==== CombatHUDActionButton_RefreshColors:Postfix- entered.");
            Mod.Log.Debug?.Log($"RefreshColors for actor: {actor?.DisplayName}  overrides: {overrides}  abilityDef: {__instance.Ability.Def.Description.Id}" +
                $"  state: {__instance.state}  isClickable: {__instance.isClickable}  mouseHover: {__instance.mouseHover}  mouseDown: {__instance.mouseDown}");

            //__instance.RefreshColors(actor);
            overrides ??= CombatHUDButtonBase.defaultColors;

            Color activeColor = new Color(1f, 0.635f, 0, 1f);
            if (__instance.state == CombatHUDActionButton.ButtonState.None)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using ButtonState.NONE colors");
                __instance.SetColors(Color.black, Color.magenta, Color.magenta, Color.magenta);
            }
            else if (__instance.state == CombatHUDActionButton.ButtonState.Unavailable || (actor == null && __instance.HUD.SelectedActor == null))
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using ButtonState.Unavailable colors");
                __instance.SetColors(Color.black, Color.gray, Color.gray, Color.clear);
                // SetColors(overrides.bgDisabledColor, overrides.outlineDisabledColor, overrides.iconDisabledColor, overrides.toolTipDisabledColor);
            }
            else if (__instance.mouseDown && __instance.isClickable)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using mouseDown + isClickable colors");
                __instance.SetColors(Color.black, activeColor, activeColor, activeColor);
                // SetColors(overrides.bgPressedColor, overrides.outlinePressedColor, overrides.iconPressedColor, overrides.toolTipPressedColor);
            }
            else if (__instance.mouseHover && __instance.isClickable)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using mouseHover + isClickable colors");
                __instance.SetColors(Color.black, activeColor, activeColor, activeColor);
                // SetColors(overrides.bgHighlightedColor, overrides.outlineHighlightedColor, overrides.iconHighlightedColor, overrides.toolTipHighlightedColor);
            }
            else if (__instance.state == CombatHUDActionButton.ButtonState.Inactive)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using ButtonState.INACTIVE colors");
                __instance.SetColors(Color.black, Color.gray, Color.gray, Color.clear);
                // __instance.SetColors(overrides.bgEnabledColor, overrides.outlineEnabledColor, color, overrides.toolTipEnabledColor);
            }
            else if (__instance.state == CombatHUDActionButton.ButtonState.Preview)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using ButtonState.Preview colors");
                __instance.SetColors(Color.black, activeColor, activeColor, activeColor);
                // __instance.SetColors(overrides.bgEnabledColor, overrides.outlineEnabledColor, color, overrides.toolTipEnabledColor);
            }
            else if (__instance.state == CombatHUDActionButton.ButtonState.Active)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using ButtonState.ACTIVE");
                __instance.SetColors(Color.black, Color.gray, Color.gray, Color.clear);
                //__instance.SetColors(Color.black, activeColor, activeColor, activeColor);
                //__instance.SetColors(overrides.bgSelectedColor, overrides.outlineSelectedColor, overrides.iconSelectedColor, tooltipColor);
            }
            else
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Using FALLTHROUGH colors");
                __instance.SetColors(Color.black, Color.gray, Color.gray, Color.clear);
            }

            if (!__instance.isClickable)
            {
                Mod.Log.Trace?.Log("CombatHUDActionButton_RefreshColors => Setting outline to GRAY");
                __instance.outlineTargetColor = Color.gray;
                //__instance.outlineTargetColor = overrides.outlineDisabledColor;
            }

            __instance.ShowAbilityTiming();

    }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ShowAbilityTiming")]
    [HarmonyPatch(new Type[] { })]
    static class CombatHUDActionButton_ShowAbilityTiming
    {
        static void Prefix(CombatHUDActionButton __instance, ref bool __runOriginal)
        {
            if (__instance == null || __instance.Ability == null | __instance.Ability?.Def?.Description?.Id == null) return;
            if (!__instance.Ability.Def.Description.Id.Contains("_UDS_", StringComparison.InvariantCultureIgnoreCase)) return; // nothing to do

            __runOriginal = false;
            Mod.Log.Trace?.Log($"==== CombatHUDActionButton_ShowAbilityTiming:Prefix- entered for: {__instance.Ability.Def.Description.Id}");

            if (__instance.Ability.CurrentCooldown > 0)
            {
                // Should be disabled, so just show the time remaining
                __instance.CooldownTimer.SetText($"{__instance.Ability.CurrentCooldown}");
                __instance.CooldownTimer.gameObject.SetActive(true);
            }
            else
            {
                __instance.CooldownTimer.gameObject.SetActive(false);
            }

            if (__instance.Ability.Def.NumberOfUses > 0)
            {
                __instance.UsesLeftCounter.SetText($"{__instance.Ability.NumUsesLeft} / {__instance.Ability.Def.NumberOfUses}");
                __instance.UsesLeftCounter.gameObject.SetActive(true);
            }
            else
            {
                __instance.UsesLeftCounter.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateSpecialAbility
    {
        static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateSpecialAbility:POSTFIX- entered.");
            Mod.Log.Debug?.Log($"ActivateCommandAbility for teamGUID: {teamGUID}  targetPos: {targetPosition}");

            if (!String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id) && __instance.Ability.Def.Description.Id.Contains("_UDS_"))
            {
                // Temporarily add the ability into the team's command ability. This is needed to keep the cooldown and turns synced,
                //      but we don't want this messing up other added command abilities so make it transient for this call
                Team team = __instance.Combat.Teams.Find((Team x) => x.GUID == teamGUID);

                Mod.Log.Debug?.Log($"Adding ability {__instance.Ability.Def.Description.Id} to team command ability and ActivatedTeamAbility");
                team.CommandAbilities.Add(__instance.Ability);
                ModState.ActivatedTeamAbility = __instance.Ability;

                MessageCenterMessage messageCenterMessage = new AbilityInvokedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, targetPosition, Vector3.zero)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                messageCenterMessage = new AbilityConfirmedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, targetPosition, Vector3.zero)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                __instance.DisableButton();
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDActionButton), "ActivateCommandAbility")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3), typeof(Vector3) })]
    static class CombatHUDActionButton_ActivateSpecialAbility_2
    {
        static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 positionA, Vector3 positionB)
        {
            Mod.Log.Trace?.Log("==== Ability_ActivateSpecialAbility:POSTFIX- entered.");
            Mod.Log.Debug?.Log($"ActivateCommandAbility for teamGUID: {teamGUID}  positionA: {positionA}  positionB: {positionB}");

            if (!String.IsNullOrEmpty(__instance?.Ability?.Def?.Description?.Id) && __instance.Ability.Def.Description.Id.Contains("_UDS_"))
            {
                // Temporarily add the ability into the team's command ability. This is needed to keep the cooldown and turns synced,
                //      but we don't want this messing up other added command abilities so make it transient for this call
                Team team = __instance.Combat.Teams.Find((Team x) => x.GUID == teamGUID);

                Mod.Log.Debug?.Log($"Adding ability {__instance.Ability.Def.Description.Id} to team command ability and ActivatedTeamAbility");
                team.CommandAbilities.Add(__instance.Ability);
                ModState.ActivatedTeamAbility = __instance.Ability;

                MessageCenterMessage messageCenterMessage = new AbilityInvokedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, positionA, positionB)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                messageCenterMessage = new AbilityConfirmedMessage(teamGUID, teamGUID, __instance.Ability.Def.Id, positionA, positionB)
                {
                    IsNetRouted = true
                };
                __instance.Combat.MessageCenter.PublishMessage(messageCenterMessage);

                __instance.DisableButton();
            }
        }
    }
}

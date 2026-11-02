using System.Linq;
using Content.Goobstation.Shared.ModSuits.Components;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    private void SpawnStartingModules(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.ModulesContainer.ContainedEntities.Count != 0)
            return;

        var theme = _proto.Index(ent.Comp.Theme);
        var coords = Transform(ent).Coordinates;

        foreach (var proto in theme.InbuiltModules.Concat(ent.Comp.InbuiltModules))
        {
            var module = PredictedSpawnAttachedTo(proto, coords);
            if (!TryComp<ModModuleComponent>(module, out var moduleComp))
            {
                PredictedQueueDel(module);
                continue;
            }

            moduleComp.Removable = false;
            InstallModule(ent, (module, moduleComp));
        }

        foreach (var proto in ent.Comp.StartingModules)
        {
            var module = PredictedSpawnAttachedTo(proto, coords);
            if (!TryComp<ModModuleComponent>(module, out var moduleComp) || !CanInstallModule(ent, (module, moduleComp), out _, true))
            {
                Log.Warning($"Starting module {proto} could not be installed into {ToPrettyString(ent)}");
                PredictedQueueDel(module);
                continue;
            }

            InstallModule(ent, (module, moduleComp));
        }
    }

    public int GetComplexity(Entity<ModSuitComponent> ent)
    {
        var complexity = 0;
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (TryComp<ModModuleComponent>(module, out var moduleComp))
                complexity += moduleComp.Complexity;
        }

        return complexity;
    }

    public bool CanInstallModule(Entity<ModSuitComponent> ent, Entity<ModModuleComponent> module, out string? reason, bool ignorePanel = false)
    {
        reason = null;

        if (ent.Comp.State != ModSuitState.Inactive)
        {
            reason = "modsuit-deactivate-first";
            return false;
        }

        if (!ignorePanel && !CanAccessInternals(ent))
        {
            reason = "modsuit-open-panel-first";
            return false;
        }

        var theme = _proto.Index(ent.Comp.Theme);
        if (_whitelist.IsWhitelistFail(theme.AllowedModules, module) || _whitelist.IsWhitelistPass(theme.BlacklistedModules, module))
        {
            reason = "modsuit-module-not-allowed";
            return false;
        }

        if (GetComplexity(ent) + module.Comp.Complexity > theme.ComplexityMax)
        {
            reason = "modsuit-module-too-complex";
            return false;
        }

        foreach (var slot in module.Comp.RequiredParts)
        {
            if (!ent.Comp.PartEntities.ContainsKey(slot))
            {
                reason = "modsuit-module-missing-part";
                return false;
            }
        }

        var proto = MetaData(module).EntityPrototype?.ID;
        foreach (var installed in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (proto != null && MetaData(installed).EntityPrototype?.ID == proto)
            {
                reason = "modsuit-module-duplicate";
                return false;
            }

            if (_whitelist.IsWhitelistPass(module.Comp.Incompatible, installed)
                || TryComp<ModModuleComponent>(installed, out var installedComp) && _whitelist.IsWhitelistPass(installedComp.Incompatible, module))
            {
                reason = "modsuit-module-incompatible";
                return false;
            }
        }

        return true;
    }

    public bool TryInstallModule(Entity<ModSuitComponent> ent, EntityUid moduleUid, EntityUid? user)
    {
        if (!TryComp<ModModuleComponent>(moduleUid, out var moduleComp))
            return false;

        var module = (moduleUid, moduleComp);
        if (!CanInstallModule(ent, module, out var reason))
        {
            if (reason != null)
                PopupFail(ent, user, reason);
            return false;
        }

        if (!InstallModule(ent, module))
            return false;

        if (user != null)
        {
            _popup.PopupClient(Loc.GetString("modsuit-module-installed", ("module", moduleUid)), ent, user.Value);
            _audio.PlayPredicted(ent.Comp.ModuleInsertSound, ent, user.Value);
        }

        return true;
    }

    private bool InstallModule(Entity<ModSuitComponent> ent, Entity<ModModuleComponent> module)
    {
        if (!_container.Insert(module.Owner, ent.Comp.ModulesContainer))
            return false;

        module.Comp.Suit = ent;
        Dirty(module);
        EnsureModuleAction(module);

        var ev = new ModModuleInstalledEvent(ent);
        RaiseLocalEvent(module, ref ev);

        if (ent.Comp.Wearer is { } wearer)
            GrantModuleAction(module, wearer);

        if (module.Comp.Type == ModModuleType.Passive && ShouldPassiveBeActive(ent, module.Comp))
            SetModuleActive(ent, module, true);

        RefreshDrain(ent);
        Dirty(ent);
        return true;
    }

    public bool TryRemoveModule(Entity<ModSuitComponent> ent, EntityUid moduleUid, EntityUid? user)
    {
        if (!TryComp<ModModuleComponent>(moduleUid, out var moduleComp) || !ent.Comp.ModulesContainer.Contains(moduleUid))
            return false;

        if (!moduleComp.Removable)
        {
            PopupFail(ent, user, "modsuit-module-not-removable");
            return false;
        }

        if (ent.Comp.State != ModSuitState.Inactive)
        {
            PopupFail(ent, user, "modsuit-deactivate-first");
            return false;
        }

        if (!CanAccessInternals(ent))
        {
            PopupFail(ent, user, "modsuit-open-panel-first");
            return false;
        }

        var module = (moduleUid, moduleComp);
        if (moduleComp.Active)
            SetModuleActive(ent, module, false);

        if (ent.Comp.Wearer is { } wearer)
            RemoveModuleAction(module, wearer);

        _container.Remove(moduleUid, ent.Comp.ModulesContainer);
        moduleComp.Suit = null;
        Dirty(moduleUid, moduleComp);

        var ev = new ModModuleRemovedEvent(ent);
        RaiseLocalEvent(moduleUid, ref ev);

        if (user != null)
        {
            _hands.PickupOrDrop(user.Value, moduleUid);
            _audio.PlayPredicted(ent.Comp.ModuleRemoveSound, ent, user.Value);
        }

        RefreshDrain(ent);
        Dirty(ent);
        return true;
    }

    private bool ShouldPassiveBeActive(Entity<ModSuitComponent> ent, ModModuleComponent module)
    {
        if ((module.AllowFlags & ModModuleAllowFlags.Inactive) != 0)
            return true;

        return ent.Comp.State == ModSuitState.Active && ent.Comp.Powered;
    }

    private void ActivatePassiveModules(Entity<ModSuitComponent> ent)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities.ToArray())
        {
            if (TryComp<ModModuleComponent>(module, out var moduleComp)
                && moduleComp.Type == ModModuleType.Passive
                && !moduleComp.Active
                && ShouldPassiveBeActive(ent, moduleComp))
                SetModuleActive(ent, (module, moduleComp), true);
        }
    }

    private void DeactivateAllModules(Entity<ModSuitComponent> ent, bool powerLoss)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities.ToArray())
        {
            if (!TryComp<ModModuleComponent>(module, out var moduleComp) || !moduleComp.Active)
                continue;

            if ((moduleComp.AllowFlags & ModModuleAllowFlags.Inactive) != 0
                && (!powerLoss || moduleComp.IdlePower <= 0f && moduleComp.ActivePower <= 0f))
                continue;

            SetModuleActive(ent, (module, moduleComp), false);
        }
    }

    public bool TryUseModule(EntityUid moduleUid, EntityUid user)
    {
        if (!TryComp<ModModuleComponent>(moduleUid, out var moduleComp)
            || moduleComp.Suit is not { } suitUid
            || !TryComp<ModSuitComponent>(suitUid, out var suitComp))
            return false;

        var suit = (suitUid, suitComp);
        var module = (moduleUid, moduleComp);

        switch (moduleComp.Type)
        {
            case ModModuleType.Passive:
                return false;
            case ModModuleType.Usable:
                if (!CanUseModule(suit, module, user))
                    return false;

                var used = new ModModuleUsedEvent(suitUid, user);
                RaiseLocalEvent(moduleUid, ref used);
                if (!used.Handled)
                    return false;

                TryUseCharge(suit, moduleComp.UsePower);
                StartCooldown(module);
                return true;
            default:
                if (moduleComp.Active)
                {
                    SetModuleActive(suit, module, false);
                    return true;
                }

                if (!CanUseModule(suit, module, user))
                    return false;

                var attempt = new ModModuleActivateAttemptEvent(suitUid, user);
                RaiseLocalEvent(moduleUid, ref attempt);
                if (attempt.Cancelled)
                {
                    if (attempt.Reason != null)
                        PopupFail(suit, user, attempt.Reason);
                    return false;
                }

                if (moduleComp.Type == ModModuleType.Active)
                    DeselectActiveModules(suit, moduleUid);

                TryUseCharge(suit, moduleComp.UsePower);
                StartCooldown(module);
                SetModuleActive(suit, module, true);
                return true;
        }
    }

    private bool CanUseModule(Entity<ModSuitComponent> suit, Entity<ModModuleComponent> module, EntityUid user)
    {
        var flags = module.Comp.AllowFlags;

        if ((flags & ModModuleAllowFlags.Unworn) == 0 && suit.Comp.Wearer != user)
        {
            PopupFail(suit, user, "modsuit-module-not-worn");
            return false;
        }

        if ((flags & ModModuleAllowFlags.Inactive) == 0 && suit.Comp.State != ModSuitState.Active)
        {
            PopupFail(suit, user, "modsuit-module-suit-inactive");
            return false;
        }

        if ((flags & ModModuleAllowFlags.Incapacitated) == 0 && !_actionBlocker.CanInteract(user, null))
            return false;

        if (_timing.CurTime < module.Comp.NextUse)
        {
            PopupFail(suit, user, "modsuit-module-cooldown");
            return false;
        }

        var needsPower = module.Comp.UsePower > 0f || module.Comp.ActivePower > 0f;
        if (needsPower && (!suit.Comp.Powered || !HasCharge(suit, module.Comp.UsePower)))
        {
            PopupFail(suit, user, "modsuit-no-power");
            return false;
        }

        return true;
    }

    private void StartCooldown(Entity<ModModuleComponent> module)
    {
        if (module.Comp.Cooldown <= TimeSpan.Zero)
            return;

        module.Comp.NextUse = _timing.CurTime + module.Comp.Cooldown;
        Dirty(module);

        if (module.Comp.ActionEntity is { } action)
            _actions.SetCooldown(action, module.Comp.Cooldown);
    }

    private void DeselectActiveModules(Entity<ModSuitComponent> suit, EntityUid except)
    {
        foreach (var other in suit.Comp.ModulesContainer.ContainedEntities.ToArray())
        {
            if (other != except
                && TryComp<ModModuleComponent>(other, out var otherComp)
                && otherComp.Type == ModModuleType.Active
                && otherComp.Active)
                SetModuleActive(suit, (other, otherComp), false);
        }
    }

    public void SetModuleActive(Entity<ModSuitComponent> suit, Entity<ModModuleComponent> module, bool active)
    {
        if (module.Comp.Active == active)
            return;

        module.Comp.Active = active;
        Dirty(module);

        if (module.Comp.ActionEntity is { } action)
            _actions.SetToggled(action, active);

        RaiseUpdated(suit);

        if (active)
        {
            var ev = new ModModuleActivatedEvent(suit, suit.Comp.Wearer);
            RaiseLocalEvent(module, ref ev);
        }
        else
        {
            var ev = new ModModuleDeactivatedEvent(suit, suit.Comp.Wearer);
            RaiseLocalEvent(module, ref ev);
        }

        RefreshDrain(suit);
    }

    public bool TryGetModuleSuit(EntityUid moduleUid, out Entity<ModSuitComponent> suit)
    {
        suit = default;
        if (!TryComp<ModModuleComponent>(moduleUid, out var moduleComp)
            || moduleComp.Suit is not { } suitUid
            || !TryComp<ModSuitComponent>(suitUid, out var suitComp))
            return false;

        suit = (suitUid, suitComp);
        return true;
    }

    public EntityUid? GetPart(Entity<ModSuitComponent> suit, string slot)
    {
        return suit.Comp.PartEntities.TryGetValue(slot, out var part) ? part : null;
    }

    [SubscribeLocalEvent]
    private void OnModuleAction(Entity<ModModuleComponent> ent, ref ModModuleActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        TryUseModule(ent, args.Performer);
    }

    private void GrantModuleActions(Entity<ModSuitComponent> ent, EntityUid wearer)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (TryComp<ModModuleComponent>(module, out var moduleComp))
                GrantModuleAction((module, moduleComp), wearer);
        }
    }

    private void RemoveModuleActions(Entity<ModSuitComponent> ent, EntityUid wearer)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (TryComp<ModModuleComponent>(module, out var moduleComp))
                RemoveModuleAction((module, moduleComp), wearer);
        }
    }

    [SubscribeLocalEvent]
    private void OnModuleMapInit(Entity<ModModuleComponent> ent, ref MapInitEvent args)
    {
        EnsureModuleAction(ent);
    }

    private void EnsureModuleAction(Entity<ModModuleComponent> module)
    {
        if (module.Comp.Type == ModModuleType.Passive || module.Comp.Action is not { } actionProto)
            return;

        if (module.Comp.ActionEntity != null
            || !_actionContainer.EnsureAction(module, ref module.Comp.ActionEntity, actionProto))
            return;

        var action = module.Comp.ActionEntity.Value;
        _metaData.SetEntityName(action, Name(module));
        _metaData.SetEntityDescription(action, Description(module));
        _actions.SetEntityIcon(action, module.Owner);
        Dirty(module);
    }

    private void GrantModuleAction(Entity<ModModuleComponent> module, EntityUid wearer)
    {
        if (_timing.ApplyingState
            || module.Comp.Type == ModModuleType.Passive
            || !module.Comp.Pinned
            || _actions.GetAction(module.Comp.ActionEntity, false) is not { } action
            || action.Comp.AttachedEntity == wearer)
            return;

        _actions.AddAction(wearer, action.Owner, module.Owner);
        _actions.SetToggled(action.AsNullable(), module.Comp.Active);
    }

    private void RemoveModuleAction(Entity<ModModuleComponent> module, EntityUid wearer)
    {
        if (_timing.ApplyingState
            || _actions.GetAction(module.Comp.ActionEntity, false) is not { } action
            || action.Comp.AttachedEntity != wearer)
            return;

        _actions.RemoveProvidedAction(wearer, module, action);
    }

    public void TogglePinned(Entity<ModSuitComponent> suit, Entity<ModModuleComponent> module)
    {
        module.Comp.Pinned = !module.Comp.Pinned;
        Dirty(module);
        RaiseUpdated(suit);

        if (suit.Comp.Wearer is not { } wearer)
            return;

        if (module.Comp.Pinned)
            GrantModuleAction(module, wearer);
        else
            RemoveModuleAction(module, wearer);
    }
}

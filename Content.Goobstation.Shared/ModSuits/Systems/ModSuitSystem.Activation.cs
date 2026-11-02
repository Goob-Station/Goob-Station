using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    public bool TryToggleActivation(Entity<ModSuitComponent> ent, EntityUid? user)
    {
        switch (ent.Comp.State)
        {
            case ModSuitState.Inactive:
                return TryStartActivation(ent, user);
            case ModSuitState.Active:
                StartDeactivation(ent);
                return true;
            default:
                PopupFail(ent, user, ent.Comp.State == ModSuitState.Activating
                    ? "modsuit-already-activating"
                    : "modsuit-already-deactivating");
                return false;
        }
    }

    public bool TryStartActivation(Entity<ModSuitComponent> ent, EntityUid? user)
    {
        if (ent.Comp.State != ModSuitState.Inactive || ent.Comp.Wearer == null)
            return false;

        if (ent.Comp.DeployQueue.Count > 0)
        {
            PopupFail(ent, user, "modsuit-deploying");
            return false;
        }

        if (!AllPartsDeployed(ent))
        {
            PopupFail(ent, user, "modsuit-deploy-all-first");
            return false;
        }

        if (HasOpenPanel(ent))
        {
            PopupFail(ent, user, "modsuit-close-panel-first");
            return false;
        }

        if (!HasCharge(ent, GetThemeDrain(ent)))
        {
            PopupFail(ent, user, "modsuit-no-power");
            return false;
        }

        var theme = _proto.Index(ent.Comp.Theme);
        ent.Comp.State = ModSuitState.Activating;
        ent.Comp.ActivationQueue = new List<string>(ent.Comp.PartEntities.Keys);
        ent.Comp.NextActivationStep = _timing.CurTime + theme.ActivationStepTime;
        Dirty(ent);
        RaiseUpdated(ent);
        _actions.SetToggled(ent.Comp.ActivateActionEntity, true);

        if (user != null)
            _popup.PopupClient(Loc.GetString("modsuit-activating"), ent, user.Value);

        return true;
    }

    public void StartDeactivation(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.State != ModSuitState.Active)
            return;

        DeactivateAllModules(ent, false);

        var theme = _proto.Index(ent.Comp.Theme);
        var queue = new List<string>(ent.Comp.PartEntities.Keys);
        queue.Reverse();
        ent.Comp.State = ModSuitState.Deactivating;
        ent.Comp.ActivationQueue = queue;
        ent.Comp.NextActivationStep = _timing.CurTime + theme.ActivationStepTime;
        Dirty(ent);
        RaiseUpdated(ent);
        _actions.SetToggled(ent.Comp.ActivateActionEntity, false);

        RefreshDrain(ent);
        RefreshSpeed(ent);
        UpdatePowerAlert(ent);
    }

    private void UpdateActivation()
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State is not (ModSuitState.Activating or ModSuitState.Deactivating)
                || now < comp.NextActivationStep
                || !ShouldStep(comp))
                continue;

            var ent = (uid, comp);
            if (comp.Wearer == null)
            {
                ForceDeactivate(ent, true);
                continue;
            }

            var sealing = comp.State == ModSuitState.Activating;
            if (comp.ActivationQueue.Count > 0)
            {
                var slot = comp.ActivationQueue[0];
                comp.ActivationQueue.RemoveAt(0);

                if (comp.PartEntities.TryGetValue(slot, out var part) && TryComp<ModPartComponent>(part, out var partComp))
                {
                    if (sealing && !IsPartDeployed(ent, slot))
                    {
                        ForceDeactivate(ent, false);
                        continue;
                    }

                    SetPartSealed((part, partComp), ent, sealing, false);
                }

                comp.NextActivationStep = now + _proto.Index(comp.Theme).ActivationStepTime;
                Dirty(uid, comp);
                RaiseUpdated(uid);
                continue;
            }

            if (sealing)
                FinishActivation(ent, false);
            else
                FinishDeactivation(ent, false);
        }
    }

    private void FinishActivation(Entity<ModSuitComponent> ent, bool silent)
    {
        ent.Comp.State = ModSuitState.Active;
        ent.Comp.ActivationQueue.Clear();
        ent.Comp.Powered = GetBattery(ent) is { } battery && _battery.GetCharge(battery.AsNullable()) > 0f;
        Dirty(ent);
        RaiseUpdated(ent);
        _actions.SetToggled(ent.Comp.ActivateActionEntity, true);

        _appearance.SetData(ent, ModSuitVisuals.Sealed, true);

        if (!silent && ent.Comp.Wearer is { } wearer)
        {
            _audio.PlayPredicted(ent.Comp.ActivateSound, ent, wearer);
            _popup.PopupClient(Loc.GetString("modsuit-activated"), wearer, wearer);
        }

        if (ent.Comp.Powered)
            ActivatePassiveModules(ent);

        RefreshDrain(ent);
        RefreshSpeed(ent);
        UpdatePowerAlert(ent);
    }

    private void FinishDeactivation(Entity<ModSuitComponent> ent, bool silent)
    {
        ent.Comp.State = ModSuitState.Inactive;
        ent.Comp.ActivationQueue.Clear();
        Dirty(ent);
        RaiseUpdated(ent);
        _actions.SetToggled(ent.Comp.ActivateActionEntity, false);

        _appearance.SetData(ent, ModSuitVisuals.Sealed, false);

        if (!silent && ent.Comp.Wearer is { } wearer)
        {
            _audio.PlayPredicted(ent.Comp.DeactivateSound, ent, wearer);
            _popup.PopupClient(Loc.GetString("modsuit-deactivated"), wearer, wearer);
        }

        RefreshDrain(ent);
        RefreshSpeed(ent);
        UpdatePowerAlert(ent);
    }

    public void ForceDeactivate(Entity<ModSuitComponent> ent, bool silent)
    {
        if (ent.Comp.State == ModSuitState.Inactive)
            return;

        DeactivateAllModules(ent, false);

        foreach (var part in ent.Comp.PartEntities.Values)
        {
            if (TryComp<ModPartComponent>(part, out var partComp) && partComp.Sealed)
                SetPartSealed((part, partComp), ent, false, true);
        }

        FinishDeactivation(ent, silent);
    }

    public void ForceActivate(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.State == ModSuitState.Active || !AllPartsDeployed(ent))
            return;

        foreach (var part in ent.Comp.PartEntities.Values)
        {
            if (TryComp<ModPartComponent>(part, out var partComp) && !partComp.Sealed)
                SetPartSealed((part, partComp), ent, true, true);
        }

        FinishActivation(ent, true);
    }

    private void BreakSeal(Entity<ModSuitComponent> ent)
    {
        var wearer = ent.Comp.Wearer;
        ForceDeactivate(ent, true);

        if (wearer == null)
            return;

        _popup.PopupClient(Loc.GetString("modsuit-seal-broken"), wearer.Value, wearer.Value, PopupType.LargeCaution);
        _audio.PlayPredicted(ent.Comp.SealBrokenSound, ent, wearer.Value);
    }

    private void SetPartSealed(Entity<ModPartComponent> part, Entity<ModSuitComponent> suit, bool sealing, bool silent)
    {
        if (part.Comp.Sealed == sealing)
            return;

        _hideLayer.SetWearerLayersHidden(part, false);

        if (TryComp<ModSealedComponentsComponent>(part, out var sealedComps))
        {
            if (sealing)
                GrantComponents(part, sealedComps.Components);
            else
                RevokeComponents(part, sealedComps.Components);
        }

        part.Comp.Sealed = sealing;
        Dirty(part);

        _appearance.SetData(part, ModSuitVisuals.Sealed, sealing);
        _hideLayer.SetWearerLayersHidden(part, true);

        if (silent || suit.Comp.Wearer is not { } wearer)
            return;

        _audio.PlayPredicted(part.Comp.SealSound, wearer, wearer);
        _popup.PopupClient(Loc.GetString(sealing ? part.Comp.SealUpPopup : part.Comp.SealDownPopup,
                ("part", part.Owner)),
            wearer,
            wearer,
            sealing ? PopupType.Small : PopupType.SmallCaution);
    }

    public void GrantComponents(EntityUid target, ComponentRegistry registry)
    {
        if (!TerminatingOrDeleted(target))
            EntityManager.AddComponents(target, registry);
    }

    public void RestoreComponent(EntityUid part, string componentName)
    {
        if (TerminatingOrDeleted(part))
            return;

        var restore = new ComponentRegistry();
        if (TryComp<ModPartComponent>(part, out var partComp)
            && partComp.Sealed
            && TryComp<ModSealedComponentsComponent>(part, out var sealedComps)
            && sealedComps.Components.TryGetValue(componentName, out var sealedEntry))
            restore[componentName] = sealedEntry;
        else if (MetaData(part).EntityPrototype is { } proto && proto.Components.TryGetValue(componentName, out var protoEntry))
            restore[componentName] = protoEntry;

        if (restore.Count > 0)
            EntityManager.AddComponents(part, restore);
    }

    public void RevokeComponents(EntityUid target, ComponentRegistry registry)
    {
        if (TerminatingOrDeleted(target))
            return;

        EntityManager.RemoveComponents(target, registry);

        if (MetaData(target).EntityPrototype is not { } proto)
            return;

        var restore = new ComponentRegistry();
        foreach (var name in registry.Keys)
        {
            if (proto.Components.TryGetValue(name, out var entry))
                restore[name] = entry;
        }

        if (restore.Count > 0)
            EntityManager.AddComponents(target, restore);
    }
}

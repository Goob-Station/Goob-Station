using Content.Goobstation.Common.Flash;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Actions;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Flash;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract partial class SharedAugmentedEyesightSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentedEyesightComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AugmentedEyesightComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<AugmentedEyesightComponent, ActionAugmentedEyesightEvent>(OnToggleVision);

        SubscribeLocalEvent<AugmentedEyesightComponent, CheckFlashVulnerable>(OnFlashVulnerableCheck);
        SubscribeLocalEvent<AugmentedEyesightComponent, FlashAttemptEvent>(OnFlashAttempt);
        SubscribeLocalEvent<AugmentedEyesightComponent, FlashDurationMultiplierEvent>(OnGetFlashMultiplier);
        SubscribeLocalEvent<AugmentedEyesightComponent, GetEyeProtectionEvent>(OnGetEyeProtection);
    }

    private void OnMapInit(Entity<AugmentedEyesightComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);

        SetVision(ent);
    }

    private void OnShutdown(Entity<AugmentedEyesightComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

        SetVision(ent, true);
    }

    #region Event Handlers

    private void OnToggleVision(Entity<AugmentedEyesightComponent> ent, ref ActionAugmentedEyesightEvent args)
    {
        ent.Comp.Enabled = !ent.Comp.Enabled;
        Dirty(ent);

        SetVision(ent);

        args.Handled = true;
    }

    private void OnFlashVulnerableCheck(Entity<AugmentedEyesightComponent> ent, ref CheckFlashVulnerable args)
    {
        args.Vulnerable = !ent.Comp.Enabled;
    }

    private void OnFlashAttempt(Entity<AugmentedEyesightComponent> ent, ref FlashAttemptEvent args)
    {
        args.Cancelled = ent.Comp.Enabled;
    }

    private void OnGetFlashMultiplier(Entity<AugmentedEyesightComponent> ent, ref FlashDurationMultiplierEvent args)
    {
        if (!ent.Comp.Enabled)
            args.Multiplier *= ent.Comp.FlashMultiplier;
    }

    private void OnGetEyeProtection(Entity<AugmentedEyesightComponent> ent, ref GetEyeProtectionEvent args)
    {
        if (ent.Comp.Enabled)
            args.Protection += ent.Comp.EyeProtectionTime;
    }

    #endregion

    #region Helper Methods

    protected virtual void SetVision(Entity<AugmentedEyesightComponent> ent, bool? state = null)
    {
        // go to AugmentedEyesightSystem for the logic
    }
    #endregion
}

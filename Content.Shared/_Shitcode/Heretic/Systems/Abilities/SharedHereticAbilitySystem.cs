using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeBlade();
        SubscribeRust();

        SubscribeLocalEvent<HereticComponent, EventHereticShadowCloak>(OnShadowCloak);
    }

    private void OnShadowCloak(Entity<HereticComponent> ent, ref EventHereticShadowCloak args)
    {
        if (!TryComp(ent, out StatusEffectsComponent? status))
            return;

        if (TryComp(ent, out ShadowCloakedComponent? shadowCloaked))
        {
            _status.TryRemoveStatusEffect(ent, args.Status, status, false);
            RemCompDeferred(ent.Owner, shadowCloaked);
            args.Handled = true;
            return;
        }

        // TryUseAbility only if we are not cloaked so that we can uncloak without focus
        // Ideally you should uncloak when losing focus but whatever
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;
        _status.TryAddStatusEffect<ShadowCloakedComponent>(ent, args.Status, args.Lifetime, true, status);
    }

    protected bool TryUseAbility(EntityUid ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (!TryComp<HereticComponent>(ent, out var hereticComp) || !actionComp.RequireMagicItem ||
            hereticComp.Ascended)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        var ev = new CheckMagicItemEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.Handled)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        // Almost all of the abilites are serverside anyway
        if (_net.IsServer)
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);

        return false;
    }

    protected virtual void SpeakAbility(EntityUid ent, HereticActionComponent args) {}
}

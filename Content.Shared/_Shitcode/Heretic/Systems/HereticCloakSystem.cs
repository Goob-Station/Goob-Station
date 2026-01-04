using Content.Shared._Shitcode.Heretic.Components.StatusEffects;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class HereticCloakSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCloakedStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<HereticCloakedStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

        SubscribeLocalEvent<StatusEffectContainerComponent, HereticLostFocusEvent>(OnLoseFocus);
    }

    private void OnLoseFocus(Entity<StatusEffectContainerComponent> ent, ref HereticLostFocusEvent args)
    {
        if (!_status.TryEffectsWithComp<HereticCloakedStatusEffectComponent>(ent, out var effects))
            return;

        foreach (var effect in effects)
        {
            if (effect.Comp1.LoseFocusMessage is { } message)
                _popup.PopupPredicted(Loc.GetString(message), ent, ent);

            if (effect.Comp1.RequiresFocus)
                PredictedQueueDel(effect.Owner);
        }
    }

    private void OnRemove(Entity<HereticCloakedStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred(args.Target, _compFact.GetRegistration(ent.Comp.Component).Type);

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.UncloakSound, args.Target);
    }

    private void OnApply(Entity<HereticCloakedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var reg = _compFact.GetRegistration(ent.Comp.Component);
        if (HasComp(args.Target, reg.Type))
            return;

        var comp = _compFact.GetComponent(reg);
        EntityManager.AddComponent(args.Target, comp);

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.CloakSound, args.Target);
    }
}

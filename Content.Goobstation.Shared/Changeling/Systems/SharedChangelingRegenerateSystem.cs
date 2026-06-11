using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract partial class SharedChangelingRegenerateSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<BloodstreamComponent> _bloodQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingRegenerateComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingRegenerateComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChangelingRegenerateComponent, ChangelingRegenerateEvent>(OnRegenerateAction);

        _bloodQuery = GetEntityQuery<BloodstreamComponent>();
        _bodyQuery = GetEntityQuery<BodyComponent>();
    }

    private void OnMapInit(Entity<ChangelingRegenerateComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<ChangelingRegenerateComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    private void OnRegenerateAction(Entity<ChangelingRegenerateComponent> ent, ref ChangelingRegenerateEvent args)
    {
        var missingLimbs = false;

        if (_bodyQuery.TryComp(ent, out var bodyComp))
        {
            var partsCount = _body.GetBodyChildren(ent, bodyComp).Count();
            var bodyProto = _proto.Index(bodyComp.Prototype);

            if (bodyProto != null)
                missingLimbs = bodyProto.Slots.Count > partsCount;

            RegenerateChangelingBody(ent, bodyComp);
        }

        if (_bloodQuery.TryComp(ent, out var bloodComp))
        {
            _blood.TryModifyBleedAmount((ent, bloodComp), -bloodComp.BleedAmount);
            _blood.TryModifyBloodLevel((ent, bloodComp), bloodComp.BloodMaxVolume);
        }

        if (missingLimbs)
            _audio.PlayPredicted(ent.Comp.RegenSound, ent, ent);

        _popup.PopupClient(Loc.GetString(missingLimbs ? ent.Comp.LimbRegenPopup : ent.Comp.RegenPopup), ent, ent);

        args.Handled = true;
    }

    protected virtual void RegenerateChangelingBody(Entity<ChangelingRegenerateComponent> ent, BodyComponent bodyComp)
    {
        // go to ChangelingRegenerateSystem for logic
    }
}

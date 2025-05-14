using System.Linq;
using Content.Goobstation.Shared.MantisBlades;
using Content.Server.Emp;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.MantisBlades;

public sealed class MantisBladesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RightMantisBladeUserComponent, ToggleRightMantisBladeEvent>(OnToggleRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ToggleLeftMantisBladeEvent>(OnToggleLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentInit>(OnInitRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentInit>(OnInitLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, EmpPulseEvent>(OnEmpRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, EmpPulseEvent>(OnEmpLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, EmpDisabledRemoved>(OnEmpDisabledRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, EmpDisabledRemoved>(OnEmpDisabledLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentShutdown>(OnShutdownRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentShutdown>(OnShutdownLeft);

        SubscribeLocalEvent<MantisBladeArmComponent, ExaminedEvent>(OnExamined);
    }

    private bool ToggleBlade<T>(EntityUid ent) where T : Component, IMantisBladeUserComponent
    {
        if (!TryComp<T>(ent, out var comp))
            return false;

        if (comp.DisabledByEmp)
        {
            _popup.PopupEntity(Loc.GetString("mantis-blade-emp"), ent, ent);
            return false;
        }

        var location = typeof(T) == typeof(RightMantisBladeUserComponent)
            ? HandLocation.Right
            : HandLocation.Left;

        var hand = _hands.EnumerateHands(ent).FirstOrDefault(hand => hand.Location == location);
        if (hand == null)
            return false;

        var activeItem = hand.HeldEntity;
        if (activeItem.HasValue && activeItem == comp.BladeUid)
        {
            Del(activeItem);
            comp.BladeUid = null;
            _audio.PlayPvs(comp.RetractSound, ent);
            return true;
        }

        var newBlade = Spawn(comp.BladeProto, Transform(ent).Coordinates);
        if (!_hands.TryPickup(ent, newBlade, hand.Name))
        {
            Del(newBlade);
            _popup.PopupEntity(Loc.GetString("mantis-blade-hand-busy"), ent, ent);
            return false;
        }

        _audio.PlayPvs(comp.ExtendSound, ent);
        comp.BladeUid = newBlade;
        return true;
    }

    private void OnToggleRight(EntityUid uid, RightMantisBladeUserComponent component, ToggleRightMantisBladeEvent args)
    {
        args.Handled = ToggleBlade<RightMantisBladeUserComponent>(uid);
    }

    private void OnToggleLeft(EntityUid uid, LeftMantisBladeUserComponent component, ToggleLeftMantisBladeEvent args)
    {
        args.Handled = ToggleBlade<LeftMantisBladeUserComponent>(uid);
    }

    private void OnInitRight(Entity<RightMantisBladeUserComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);
    }

    private void OnInitLeft(Entity<LeftMantisBladeUserComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);
    }

    private void OnEmpRight(EntityUid uid, RightMantisBladeUserComponent comp, EmpPulseEvent args)
    {
        comp.DisabledByEmp = true;
    }

    private void OnEmpLeft(EntityUid uid, LeftMantisBladeUserComponent comp, EmpPulseEvent args)
    {
        comp.DisabledByEmp = true;
    }
    private void OnEmpDisabledRight(EntityUid uid, RightMantisBladeUserComponent comp, EmpDisabledRemoved args)
    {
        comp.DisabledByEmp = false;
    }
    private void OnEmpDisabledLeft(EntityUid uid, LeftMantisBladeUserComponent comp, EmpDisabledRemoved args)
    {
        comp.DisabledByEmp = false;
    }

    private void OnShutdownRight(Entity<RightMantisBladeUserComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        _actions.RemoveAction(ent.Comp.ActionUid);
    }

    private void OnShutdownLeft(Entity<LeftMantisBladeUserComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        _actions.RemoveAction(ent.Comp.ActionUid);
    }

    private void OnExamined(EntityUid uid, MantisBladeArmComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mantis-blade-arm-examine"));
    }
}

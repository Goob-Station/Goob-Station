// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Cyberware;
using Content.Goobstation.Shared.MantisBlades;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Hands.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.PairedExtendable.Systems;

public sealed class MantisBladesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly CyberneticsSystem _cybernetics = default!;
    [Dependency] private readonly PairedExtendableSystem _pairedExtendable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartAddedEvent>(OnAttach);
        SubscribeLocalEvent<MantisBladeArmComponent, ToggleMantisBladeEvent>(OnToggle);
        SubscribeLocalEvent<MantisBladeArmComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartRemovedEvent>(OnDetach);
        SubscribeLocalEvent<MantisBladeArmComponent, CyberwareChangedEvent>(OnCyberwareChanged);
    }

    private void OnInit(Entity<MantisBladeArmComponent> ent, ref ComponentInit args) => AddAction(ent);

    private void OnAttach(Entity<MantisBladeArmComponent> ent, ref BodyPartAddedEvent args) => AddAction(ent);

    private void AddAction(Entity<MantisBladeArmComponent> ent)
    {
        if (Exists(ent.Comp.ActionUid)
            || !TryComp<BodyPartComponent>(ent, out var part)
            || part.Body == null)
            return;

        ent.Comp.ActionUid = _actions.AddAction(part.Body.Value, ent.Comp.ActionProto, ent);
    }

    private void OnToggle(Entity<MantisBladeArmComponent> ent, ref ToggleMantisBladeEvent args)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part)
        || part.Body == null)
            return;

        if (!_cybernetics.IsEnabled(ent))
            return;

        var handLocation = part.Symmetry switch
        {
            BodyPartSymmetry.Left => HandLocation.Left,
            BodyPartSymmetry.Right => HandLocation.Right,
            BodyPartSymmetry.None => HandLocation.Middle,
            _ => throw new ArgumentOutOfRangeException(),
        };

        args.Handled = _pairedExtendable.ToggleExtendable(part.Body.Value,
            ent.Comp.BladeProto,
            handLocation,
            out ent.Comp.BladeUid,
            ent.Comp.BladeUid);

        if (!args.Handled)
            return;

        _audio.PlayPvs(ent.Comp.BladeUid == null ? ent.Comp.RetractSound : ent.Comp.ExtendSound, ent);
        _cybernetics.SetActive(ent.Owner, ent.Comp.BladeUid != null);
    }

    private void OnCyberwareChanged(Entity<MantisBladeArmComponent> ent, ref CyberwareChangedEvent args)
    {
        var enabled = _cybernetics.IsEnabled(ent);

        if (!enabled && ent.Comp.BladeUid != null)
        {
            Del(ent.Comp.BladeUid);
            ent.Comp.BladeUid = null;
            _audio.PlayPvs(ent.Comp.RetractSound, ent);
            _cybernetics.SetActive(ent.Owner, false);
        }

        AddAction(ent);
        _actions.SetEnabled(ent.Comp.ActionUid, enabled);
    }

    private void OnShutdown(Entity<MantisBladeArmComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        Del(ent.Comp.ActionUid);
    }

    private void OnDetach(Entity<MantisBladeArmComponent> ent, ref BodyPartRemovedEvent args)
    {
        Del(ent.Comp.BladeUid);
        Del(ent.Comp.ActionUid);
        ent.Comp.BladeUid = null;
        ent.Comp.ActionUid = null;
        _cybernetics.SetActive(ent.Owner, false);
    }
}

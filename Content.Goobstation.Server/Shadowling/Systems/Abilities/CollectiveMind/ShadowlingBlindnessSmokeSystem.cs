// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Blindness Smoke ability logic.
/// The performer outputs a smoke that heals all Shadowlings and Thralls, but blinds anyone else.
/// </summary>
public sealed class ShadowlingBlindnessSmokeSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingBlindnessSmokeComponent, BlindnessSmokeEvent>(OnBlindnessSmoke);
        SubscribeLocalEvent<ShadowlingBlindnessSmokeComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingBlindnessSmokeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingBlindnessSmokeComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingBlindnessSmokeComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnBlindnessSmoke(EntityUid uid, ShadowlingBlindnessSmokeComponent comp, BlindnessSmokeEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(uid);
        var worldPos = _transform.GetMapCoordinates(uid, xform);

        var solution = new Solution(comp.Reagent, comp.ReagentQuantity);
        var foamEnt = Spawn("Smoke", worldPos);

        _smoke.StartSmoke(foamEnt, solution, comp.Duration, comp.SpreadAmount);

        _audio.PlayPvs(comp.BlindnessSound, uid, AudioParams.Default.WithVolume(-1f));
        _actions.StartUseDelay((args.Action.Owner, args.Action.Comp));
        args.Handled = true;
    }
}

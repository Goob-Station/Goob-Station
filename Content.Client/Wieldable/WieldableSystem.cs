// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Content.Shared.Camera;
using Content.Shared.Hands;
using Content.Shared.Movement.Components;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Client.Timing;

namespace Content.Client.Wieldable;

public sealed class WieldableSystem : SharedWieldableSystem
{
    [Dependency] private readonly EyeCursorOffsetSystem _eyeOffset = default!;
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursorOffsetRequiresWieldComponent, ItemUnwieldedEvent>(OnEyeOffsetUnwielded);
        SubscribeLocalEvent<CursorOffsetRequiresWieldComponent, HeldRelayedEvent<GetEyeOffsetRelayedEvent>>(OnGetEyeOffset);
    }

    public void OnEyeOffsetUnwielded(Entity<CursorOffsetRequiresWieldComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (!TryComp(entity.Owner, out EyeCursorOffsetComponent? cursorOffsetComp))
            return;

        if (_gameTiming.IsFirstTimePredicted)
            cursorOffsetComp.CurrentPosition = Vector2.Zero;
    }

    public void OnGetEyeOffset(Entity<CursorOffsetRequiresWieldComponent> entity, ref HeldRelayedEvent<GetEyeOffsetRelayedEvent> args)
    {
        if (!TryComp(entity.Owner, out WieldableComponent? wieldableComp))
            return;

        if (!wieldableComp.Wielded)
            return;

        var offset = _eyeOffset.OffsetAfterMouse(entity.Owner, null);
        if (offset == null)
            return;

        args.Args.Offset += offset.Value;
    }
}
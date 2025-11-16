// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Input;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.GrabReleaseBind;

/// <summary>
/// This handle binding the resist grab key
/// </summary>
public sealed class GrabReleaseBindSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ResistGrab,
                InputCmdHandler.FromDelegate(HandleResistGrab, handle: false, outsidePrediction: false))
            .Register<GrabReleaseBindSystem>();
    }

    private void HandleResistGrab(ICommonSession? session)
    {
        if (session?.AttachedEntity == null || !TryComp<PullableComponent>(session.AttachedEntity, out var pullable))
            return;

        _pullingSystem.TryStopPull(session.AttachedEntity.Value, pullable, session.AttachedEntity.Value);
    }
}

// SPDX-FileCopyrightText: 2020 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2020 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2020 Manel Navola <6786088+ManelNavola@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Manel Navola <ManelNavola@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.DoAfter;

/// <summary>
/// Handles events that need to happen after a certain amount of time where the event could be cancelled by factors
/// such as moving.
/// </summary>
public sealed class DoAfterSystem : SharedDoAfterSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new DoAfterOverlay(EntityManager, _prototype, GameTiming, _player));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<DoAfterOverlay>();
    }

#pragma warning disable RA0028 // No base call in overriden function
    public override void Update(float frameTime)
#pragma warning restore RA0028 // No base call in overriden function
    {
        // Currently this only predicts do afters initiated by the player.

        // TODO maybe predict do-afters if the local player is the target of some other players do-after? Specifically
        // ones that depend on the target not moving, because the cancellation of those do afters should be readily
        // predictable by clients.

        var playerEntity = _player.LocalEntity;

        if (!TryComp(playerEntity, out ActiveDoAfterComponent? active))
            return;

        if (_metadata.EntityPaused(playerEntity.Value))
            return;

        var time = GameTiming.CurTime;
        var comp = Comp<DoAfterComponent>(playerEntity.Value);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var handsQuery = GetEntityQuery<HandsComponent>();
        Update(playerEntity.Value, active, comp, time, xformQuery, handsQuery);
    }

    /// <summary>
    /// Try to find an active do-after being executed by the local player.
    /// </summary>
    /// <param name="entity">The entity the do after must be targeting (<see cref="DoAfterArgs.Target"/>)</param>
    /// <param name="doAfter">The found do-after.</param>
    /// <param name="event">The event to be raised on the found do-after when it completes.</param>
    /// <param name="progress">The progress of the found do-after, from 0 to 1.</param>
    /// <typeparam name="T">The type of event that must be raised by the found do-after.</typeparam>
    /// <returns>True if a do-after was found.</returns>
    public bool TryFindActiveDoAfter<T>(
        EntityUid entity,
        [NotNullWhen(true)] out Shared.DoAfter.DoAfter? doAfter,
        [NotNullWhen(true)] out T? @event,
        out float progress)
        where T : DoAfterEvent
    {
        var playerEntity = _player.LocalEntity;

        doAfter = null;
        @event = null;
        progress = default;

        if (!TryComp(playerEntity, out ActiveDoAfterComponent? active))
            return false;

        if (_metadata.EntityPaused(playerEntity.Value))
            return false;

        var comp = Comp<DoAfterComponent>(playerEntity.Value);

        var time = GameTiming.CurTime;

        foreach (var candidate in comp.DoAfters.Values)
        {
            if (candidate.Cancelled)
                continue;

            if (candidate.Args.Target != entity)
                continue;

            if (candidate.Args.Event is not T candidateEvent)
                continue;

            @event = candidateEvent;
            doAfter = candidate;
            var elapsed = time - doAfter.StartTime;
            progress = (float) Math.Min(1, elapsed.TotalSeconds / doAfter.Args.Delay.TotalSeconds);

            return true;
        }

        return false;
    }
}
// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Drunk;
using Content.Shared.StatusEffectNew;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Drunk;

public sealed class DrunkSystem : SharedDrunkSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private DrunkOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrunkStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<DrunkStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);

        SubscribeLocalEvent<DrunkStatusEffectComponent, StatusEffectRelayedEvent<LocalPlayerAttachedEvent>>(OnPlayerAttached);
        SubscribeLocalEvent<DrunkStatusEffectComponent, StatusEffectRelayedEvent<LocalPlayerDetachedEvent>>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnStatusApplied(Entity<DrunkStatusEffectComponent> entity, ref StatusEffectAppliedEvent args)
    {
        if (!_overlayMan.HasOverlay<DrunkOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnStatusRemoved(Entity<DrunkStatusEffectComponent> entity, ref StatusEffectRemovedEvent args)
    {
        if (Status.HasEffectComp<DrunkStatusEffectComponent>(args.Target))
            return;

        if (_player.LocalEntity != args.Target)
            return;

        _overlay.CurrentBoozePower = 0;
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<DrunkStatusEffectComponent> entity, ref StatusEffectRelayedEvent<LocalPlayerAttachedEvent> args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<DrunkStatusEffectComponent> entity, ref StatusEffectRelayedEvent<LocalPlayerDetachedEvent> args)
    {
        _overlay.CurrentBoozePower = 0;
        _overlayMan.RemoveOverlay(_overlay);
    }
}
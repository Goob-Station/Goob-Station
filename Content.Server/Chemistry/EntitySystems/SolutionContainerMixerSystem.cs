// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Power.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Power;

namespace Content.Server.Chemistry.EntitySystems;

/// <inheritdoc/>
public sealed class SolutionContainerMixerSystem : SharedSolutionContainerMixerSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SolutionContainerMixerComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnPowerChanged(Entity<SolutionContainerMixerComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            StopMix(ent);
    }

    protected override bool HasPower(Entity<SolutionContainerMixerComponent> entity)
    {
        return this.IsPowered(entity, EntityManager);
    }
}
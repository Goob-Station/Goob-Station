// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the main werewolf logic
/// </summary>
public sealed class SharedWerewolfSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfComponent, EatenFuryEvent>(OnFuryEaten);
        SubscribeLocalEvent<WerewolfComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnFuryEaten(Entity<WerewolfComponent> ent, ref EatenFuryEvent args) =>
        AdjustFury(ent.Comp, args.FuryToGrant);

    private void OnPolymorphed(Entity<WerewolfComponent> ent, ref PolymorphedEvent args)
    {
        // transfer fury to new entity (I HATE POLYMORPHS)
        if (!TryComp<WerewolfComponent>(args.NewEntity, out var werewolf))
            return;
        werewolf.Fury = ent.Comp.Fury;

        // for updating the fury in the ui
        Dirty(args.NewEntity, werewolf);
    }

    /// <summary>
    /// Adjusts the fury of the target. Provide positive value to increase, or negative value to decrease said fury.
    /// </summary>
    /// <param name="component"></param> The component of the werewolf
    /// <param name="Fury"></param> The fury to increase/decrease to the werewolf
    private void AdjustFury(WerewolfComponent component, int Fury) =>
        component.Fury += Fury;
}

// SPDX-FileCopyrightText: 2022 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <luanrcomets2@gmail,com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Network;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// This handles permanent blindness, both the examine and the actual effect.
/// </summary>
public sealed class PermanentBlindnessSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly BlindableSystem _blinding = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PermanentBlindnessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PermanentBlindnessComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PermanentBlindnessComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<PermanentBlindnessComponent> blindness, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && blindness.Comp.Blindness == 0)
        {
            args.PushMarkup(Loc.GetString("permanent-blindness-trait-examined", ("target", Identity.Entity(blindness, EntityManager))));
        }
        else if (args.IsInDetailsRange && !_net.IsClient && blindness.Comp.Blindness == 4) /// Goobstation
        {
            args.PushMarkup(Loc.GetString("poor-vision-trait-examined", ("target", Identity.Entity(blindness, EntityManager))));
        }  
    }

    private void OnShutdown(Entity<PermanentBlindnessComponent> blindness, ref ComponentShutdown args)
    {
        if (!TryComp<BlindableComponent>(blindness.Owner, out var blindable))
            return;

        if (blindable.MinDamage != 0)
        {
            _blinding.SetMinDamage((blindness.Owner, blindable), 0);
        }
    }

    private void OnMapInit(Entity<PermanentBlindnessComponent> blindness, ref MapInitEvent args)
    {
        if(!TryComp<BlindableComponent>(blindness.Owner, out var blindable))
            return;

        if (blindness.Comp.Blindness != 0)
            _blinding.SetMinDamage((blindness.Owner, blindable), blindness.Comp.Blindness);
        else
        {
            var maxMagnitudeInt = (int) BlurryVisionComponent.MaxMagnitude;
            _blinding.SetMinDamage((blindness.Owner, blindable), maxMagnitudeInt);
        }
    }
}
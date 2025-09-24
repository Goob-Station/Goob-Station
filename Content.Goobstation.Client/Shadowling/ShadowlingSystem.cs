// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Systems;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Shadowling;

/// <summary>
/// This handles status icons for slings and thralls
/// This also handles alerts
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, GetStatusIconsEvent>(GetThrallIcon);
        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(GetShadowlingIcon);
    }

    private void GetThrallIcon(Entity<ThrallComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<ShadowlingComponent>(ent))
            return;

        var iconProto = _prototype.Index(ent.Comp.StatusIcon);
        args.StatusIcons.Add(iconProto);
    }

    private void GetShadowlingIcon(Entity<ShadowlingComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}

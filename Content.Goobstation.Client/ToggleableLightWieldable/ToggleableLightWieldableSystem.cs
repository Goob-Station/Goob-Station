// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Toggleable;
using Content.Shared.Hands;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.ToggleableLightWieldable;

public sealed class ToggleableLightWieldableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleableLightWieldableComponent, GetInhandVisualsEvent>(OnGetHeldVisuals, after: new[] { typeof(ToggleableLightVisualsSystem) });
    }

    private void OnGetHeldVisuals(Entity<ToggleableLightWieldableComponent> ent, ref GetInhandVisualsEvent args)
    {
        if (!TryComp(ent, out WieldableComponent? wieldable))
            return;

        var location = args.Location.ToString().ToLowerInvariant();
        var layer = args.Layers.FirstOrNull(x => x.Item1 == location)?.Item2;
        var layerWielded = args.Layers.FirstOrNull(x => x.Item1 == $"wielded-{location}")?.Item2;

        if (layer == null || layerWielded == null)
            return;

        layer.Visible = !wieldable.Wielded;
        layerWielded.Visible = wieldable.Wielded;
    }
}
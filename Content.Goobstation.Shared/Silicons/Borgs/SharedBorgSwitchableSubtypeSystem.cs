// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Silicons.Borgs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Silicons.Borgs;

public abstract class SharedBorgSwitchableSubtypeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, ComponentInit>(OnComponentInit);
    }

    private void OnMapInit(Entity<BorgSwitchableSubtypeComponent> ent, ref MapInitEvent args) =>
        UpdateVisuals(ent);

    private void OnComponentInit(Entity<BorgSwitchableSubtypeComponent> ent, ref ComponentInit args) =>
        UpdateVisuals(ent);

    protected virtual void SetAppearanceFromSubtype(Entity<BorgSwitchableSubtypeComponent> ent, ProtoId<BorgSubtypePrototype> subtype) { }

    protected void UpdateVisuals(Entity<BorgSwitchableSubtypeComponent> ent)
    {
        if (ent.Comp.BorgSubtype == null)
            return;
        SetAppearanceFromSubtype(ent, ent.Comp.BorgSubtype.Value);
    }
}

// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Clothing;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Clothing.Systems;

public sealed class SharedAltClothingLayerSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AltClothingLayerComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
        SubscribeLocalEvent<AltClothingLayerComponent, GetActualMapLayerEvent>(OnGetActualMapLayer);
        SubscribeLocalEvent<AltClothingLayerComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
        SubscribeLocalEvent<AltClothingLayerComponent, GotUnequippedEvent>(OnUnequip,
            after: new[] { typeof(ClothingSystem) });
    }

    private void OnAfterAutoHandleState(Entity<AltClothingLayerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _item.VisualsChanged(ent);
    }

    private void OnGetActualMapLayer(Entity<AltClothingLayerComponent> ent, ref GetActualMapLayerEvent args)
    {
        if (ent.Comp.AltStyle)
            args.MapLayer = args.MapLayer.Replace(ent.Comp.DefaultLayer, ent.Comp.AltLayer);
    }

    private void OnUnequip(Entity<AltClothingLayerComponent> ent, ref GotUnequippedEvent args)
    {
        if (!ent.Comp.AltStyle)
            return;

        ent.Comp.AltStyle = false;
        Dirty(ent);
    }

    private void OnVerb(Entity<AltClothingLayerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp(ent, out ClothingComponent? component) || component.InSlot == null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                ent.Comp.AltStyle = !ent.Comp.AltStyle;
                Dirty(ent);
                _item.VisualsChanged(ent);
            },
            Text = ent.Comp.AltStyle ? Loc.GetString(ent.Comp.ChangeToDefaultMessage) : Loc.GetString(ent.Comp.ChangeToAltMessage),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
            Priority = 1,
        };

        args.Verbs.Add(verb);
    }
}

// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ghetto;

public sealed class TagGrantSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TagGrantComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, TagGrantComponent component, UseInHandEvent args)
    {
        if (args.Handled || component.Uses <= 0)
            return;

        if (_tagSystem.HasTag(args.User, component.Tag))
        {
            _popup.PopupEntity(Loc.GetString("tag-grant-invalid-tag"), args.User, args.User);
            args.Handled = true;
            return;
        }

        if (!_proto.HasIndex(component.Tag))
        {
            _popup.PopupEntity(Loc.GetString("tag-grant-invalid-tag"), args.User, args.User);
            return;
        }

        _tagSystem.AddTag(args.User, component.Tag);

        if (!string.IsNullOrEmpty(component.Popup))
            _popup.PopupEntity(Loc.GetString(component.Popup), args.User, args.User);

        component.Uses--;
        args.Handled = true;
    }
}

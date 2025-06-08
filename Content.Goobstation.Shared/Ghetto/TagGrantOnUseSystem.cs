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

/// <summary>
/// Handles the logic for granting tags when items with TagGrantOnUseComponent are activated.
/// </summary>
public sealed class TagGrantOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TagGrantOnUseComponent, UseInHandEvent>(OnUseInHand);
    }
    private void OnUseInHand(EntityUid uid, TagGrantOnUseComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;
        if (component.Uses <= 0)
            return;

        if (!_proto.HasIndex(component.Tag))
        {
            _popup.PopupEntity(Loc.GetString("tag-grant-invalid-tag"), args.User, args.User);
            return;
        }
        if (_tagSystem.HasTag(args.User, component.Tag))
        {
            _popup.PopupEntity(Loc.GetString("tag-grant-invalid-tag"), args.User, args.User);
            args.Handled = true;
            return;
        }
        _tagSystem.AddTag(args.User, component.Tag);

        if (component.Popup is {} popup && !string.IsNullOrWhiteSpace(popup))
            _popup.PopupEntity(Loc.GetString(popup), args.User, args.User);

        // Decrement uses if not infinite
        if (component.Uses.HasValue)
            component.Uses--;

        args.Handled = true;
    }
}

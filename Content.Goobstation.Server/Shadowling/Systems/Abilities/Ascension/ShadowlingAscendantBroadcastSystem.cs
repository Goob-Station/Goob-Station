// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Administration;
using Content.Server.Popups;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Ascendant Broadcast ability.
/// It broadcasts (via a large red popup) a message to everyone.
/// </summary>
public sealed class ShadowlingAscendantBroadcastSystem : EntitySystem
{
    [Dependency] private readonly QuickDialogSystem _dialogSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendantBroadcastComponent, AscendantBroadcastEvent>(OnBroadcast);
    }

    private void OnBroadcast(EntityUid uid, ShadowlingAscendantBroadcastComponent component, AscendantBroadcastEvent args)
    {
        if (!TryComp<ActorComponent>(args.Performer, out var actor))
            return;

        _dialogSystem.OpenDialog(actor.PlayerSession, component.Title, "Message", (string message) =>
        {
            if (actor.PlayerSession.AttachedEntity == null)
                return;

            var query = EntityQueryEnumerator<ActorComponent>();
            while (query.MoveNext(out var ent, out _))
            {
                if (ent == uid)
                    continue;
                _popupSystem.PopupEntity(message, ent, ent, PopupType.LargeCaution);
            }
            _popupSystem.PopupEntity(Loc.GetString("shadowling-ascendant-broadcast-dialog"), uid, uid, PopupType.Medium);
        });
    }
}

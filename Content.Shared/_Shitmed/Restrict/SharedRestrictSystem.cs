// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;

namespace Content.Shared._Shitmed.Restrict;
public sealed partial class SharedRestrictSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RestrictByUserTagComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<RestrictByUserTagComponent, InteractionAttemptEvent>(OnAttemptInteract);
    }

    private void OnAttemptInteract(Entity<RestrictByUserTagComponent> ent, ref InteractionAttemptEvent args)
    {
        if (!_tagSystem.HasAllTags(args.Uid, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.Uid, ent.Comp.DoesntContain))
        {
            args.Cancelled = true;
            if (ent.Comp.Messages.Count != 0)
                _popup.PopupClient(Loc.GetString(_random.Pick(ent.Comp.Messages)), args.Uid);
        }
    }

    private void OnAttemptMelee(Entity<RestrictByUserTagComponent> ent, ref AttemptMeleeEvent args)
    {
        if(!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            args.Cancelled = true;
            if(ent.Comp.Messages.Count != 0)
                args.Message = Loc.GetString(_random.Pick(ent.Comp.Messages));
        }
    }
}
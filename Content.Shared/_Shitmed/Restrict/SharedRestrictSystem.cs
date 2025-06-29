// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Restrict;
public sealed partial class SharedRestrictSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RestrictInteractionByUserTagComponent, BeforeRangedInteractEvent>(OnAttemptInteract);
        SubscribeLocalEvent<RestrictMeleeByUserTagComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<RestrictGunshotsByUserTagComponent, ShotAttemptedEvent>(OnAttemptGunshot);
    }

    private void OnAttemptInteract(Entity<RestrictInteractionByUserTagComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            if (ent.Comp.Messages.Count != 0)
                _popup.PopupClient(Loc.GetString(_random.Pick(ent.Comp.Messages)), args.User);

            args.Handled = true;
        }
    }

    private void OnAttemptMelee(Entity<RestrictMeleeByUserTagComponent> ent, ref AttemptMeleeEvent args)
    {
        if(!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            if(ent.Comp.Messages.Count != 0)
                args.Message = Loc.GetString(_random.Pick(ent.Comp.Messages));

            args.Cancelled = true;
        }
    }

    private void OnAttemptGunshot(Entity<RestrictGunshotsByUserTagComponent> ent, ref ShotAttemptedEvent args)
    {
        if(!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            var time = _timing.CurTime;

            if(ent.Comp.Messages.Count != 0 && time > ent.Comp.LastPopup + TimeSpan.FromSeconds(1))
            {
                ent.Comp.LastPopup = time;
                _popup.PopupClient(Loc.GetString(_random.Pick(ent.Comp.Messages)), args.User);
            }

            args.Cancel();
        }
    }
}
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Skinnable;
using Content.Server.DoAfter;
using Content.Server.Kitchen.Components;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Skinnable;

public sealed partial class SkinnableSystem : SharedSkinnableSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly PopupSystem _popups = null!;
    [Dependency] private readonly AudioSystem _audio = null!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SkinnableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<SkinnableComponent, SkinningDoAfterEvent>(OnSkinningDoAfter);
    }

    private void OnGetVerbs(Entity<SkinnableComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess
        || !args.CanInteract
        || !args.CanComplexInteract
        || !TryComp<SharpComponent>(args.Using, out _)
        || ent.Comp.Skinned)
            return;

        var target = ent;
        var performer = args.User;
        var arguments = args;
        InteractionVerb verb = new()
        {
            Act = () => { StartSkinning(performer, target, arguments); },
            Text = Loc.GetString("skin-verb"),
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Mobs/Animals/monkey.rsi"), "monkey_skinned"),
            Priority = 1,
        };

        args.Verbs.Add(verb);

    }

    private void StartSkinning(EntityUid performer, Entity<SkinnableComponent> target, GetVerbsEvent<InteractionVerb> args)
    {
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            performer,
            target.Comp.SkinningDoAfterDuation,
            new SkinningDoAfterEvent(),
            target,
            target,
            args.Using
            )
        {
            BreakOnMove = true,
            NeedHand = true,
            BlockDuplicate = true,
            BreakOnDropItem = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        _audio.PlayPvs(target.Comp.SkinSound, target);
        var popup = Loc.GetString("skinning-start", ("target", target), ("performer", performer));
        _popups.PopupEntity(popup, target, PopupType.LargeCaution);

    }

    private void OnSkinningDoAfter(Entity<SkinnableComponent> target, ref SkinningDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !args.Target.HasValue)
            return;

        target.Comp.Skinned = true;
        ChangeVisuals(target);

        _damageable.TryChangeDamage(target, target.Comp.DamageOnSkinned);
    }

}

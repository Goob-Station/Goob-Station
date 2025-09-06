using Content.Shared.Paper;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Mobs;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Server.Forensics;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Grudge;

/// <summary>
/// This handles...
/// </summary>
public sealed class GrudgeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly DamageableSystem _damageSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogs = default!;
    [Dependency] private readonly NameModifierSystem _nameModifierSystem = default!;
    [Dependency] private readonly ForensicsSystem _forensic = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<TargetOfGrudgeComponent, MobStateChangedEvent>(OnMobstateChange);// when Target Dies
        SubscribeLocalEvent<BookOfGrudgesComponent, PaperAfterWriteEvent>(OnPaperAfterWrite); // when name is added
        SubscribeLocalEvent<BookOfGrudgesComponent, MeleeHitEvent>(OnMeleeHit); // on attack / damage calculation
        SubscribeLocalEvent<TargetOfGrudgeComponent, DamageModifyEvent>(OnDamaged);
        //TODO: If book is destroyed remove all Grudges connected to it.
        //TODO: when inspected tell how manny gudges are writen in book.
    }

    private void OnPaperAfterWrite(Entity<BookOfGrudgesComponent> ent, ref PaperAfterWriteEvent args)
    {
        // if the entity is not a paper, we don't do anything
        if (!TryComp<PaperComponent>(ent.Owner, out var paper))
            return;

        var content = paper.Content;

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var showPopup = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var parts = line.Split(',', 2, StringSplitOptions.RemoveEmptyEntries);

            var name = parts[0].Trim();

            if (!CheckIfEligible(name, ent, out var uid))
                continue;

            // Compiler will complain if we don't check for null here.
            if (uid is not { } realUid)
                continue;

            if(HasComp<TargetOfGrudgeComponent>(realUid))
                continue;

            EnsureComp<TargetOfGrudgeComponent>(realUid, out var targetComp);

            ent.Comp.Names.Add(name.ToLower());

            showPopup = true;

            targetComp.Book = ent;

            NameAdded(ent, args.Actor);

            _popupSystem.PopupEntity(Loc.GetString("book-of-grudges-target"), ent.Owner, realUid, PopupType.Large);
            //Dirty(realUid, targetComp); // crashes compiler

            _adminLogs.Add(LogType.Chat,
                LogImpact.Low,
                $"{ToPrettyString(args.Actor)} has written {ToPrettyString(realUid)}'s name the book of grudges.");

        }

        //Dirty(ent); // crashes compiler

        if (showPopup)
            _popupSystem.PopupEntity(Loc.GetString("book-of-grudges-name-added"), ent.Owner, args.Actor, PopupType.Large);
    }

    private void OnMeleeHit(Entity<BookOfGrudgesComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            if(HasComp<TargetOfGrudgeComponent>(hit))
                args.BonusDamage = args.BaseDamage * ent.Comp.GrudgeBookDamageModifier;
        }
    }

    private void OnDamaged(Entity<TargetOfGrudgeComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage = args.OriginalDamage * ent.Comp.Book.Comp.GrudgeCurseModifier;
    }

    private void OnMobstateChange(Entity<TargetOfGrudgeComponent> ent, ref MobStateChangedEvent args)
    {
        if(args.NewMobState == MobState.Dead)
        {
            if (ent.Comp.Book.Comp.Names.Remove(_nameModifierSystem.GetBaseName(ent.Owner).ToLower()))
                _popupSystem.PopupEntity(Loc.GetString("book-of-grudges-removed"), ent.Comp.Book.Owner, PopupType.Large);

            //TODO: Find name in book and line it out

            RemCompDeferred<TargetOfGrudgeComponent>(ent);//Target is dead, grudge is settled
        }

    }

    private bool CheckIfEligible(string name, Entity<BookOfGrudgesComponent> ent, [NotNullWhen(true)] out EntityUid? entityUid)
    {
        if (ent.Comp.Names.Contains(name.ToLower()))
        {
            entityUid = null;
            return false;

        }

        EntityUid? uid;
        MobStateComponent? mob;

        if (ent.Comp.MetaGrudge)
        {
            if (!TryFindPlayerByName(name, out uid) ||
                !TryComp<MobStateComponent>(uid, out mob))
            {
                entityUid = null;
                return false;
            }
        }
        else
        {
            if (!TryFindEntityByName(name, out uid) ||
                !TryComp<MobStateComponent>(uid, out mob))
            {
                entityUid = null;
                return false;
            }
        }

        if (uid is not { } realUid)
        {
            entityUid = null;
            return false;
        }

        if (mob.CurrentState == MobState.Dead)
        {
            entityUid = null;
            return false;
        }

        entityUid = uid;
        return true;
    }

    private bool TryFindEntityByName(string name, [NotNullWhen(true)] out EntityUid? entityUid)
    {
        var query = EntityQueryEnumerator<HumanoidAppearanceComponent>();

        while (query.MoveNext(out var uid, out _))
        {
            if (!_nameModifierSystem.GetBaseName(uid).Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;

            entityUid = uid;
            return true;
        }

        entityUid = null;
        return false;
    }

    private bool TryFindPlayerByName(string name, [NotNullWhen(true)] out EntityUid? entityUid)
    {
        var query = EntityQueryEnumerator<ActorComponent>();

        while (query.MoveNext(out var uid, out var actor))
        {
            if (!actor.PlayerSession.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                continue;

            entityUid = uid;
            return true;
        }

        entityUid = null;
        return false;
    }

    private void NameAdded(Entity<BookOfGrudgesComponent> book, EntityUid writer)
    {
        // take damage from adding a new name and more damage from adding more names prevents adding to manny names.
        _damageSystem.TryChangeDamage(writer, book.Comp.Damage * book.Comp.Names.Count, true);
        _forensic.TransferDna(book, writer); // transfer blood dna to the book
    }
}

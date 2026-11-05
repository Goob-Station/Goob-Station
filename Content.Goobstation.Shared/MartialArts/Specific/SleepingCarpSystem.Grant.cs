using Content.Goobstation.Shared.CustomFactionIcons;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Goobstation.Shared.Projectiles;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusIcon;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class SleepingCarpSystem
{
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private IGameTiming _timing = default!;

    private ProtoId<NpcFactionPrototype> _factionGrant = "Dragon";
    private ProtoId<FactionIconPrototype> _factionIconGrant = "SleepingCarpFaction";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SleepingCarpGnasherComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.ResetTime)
                continue;

            comp.ConsecutiveGnashes = 0;
            Dirty(uid, comp);
        }
    }
    public bool GrantSleepingCarp(Entity<MeleeWeaponComponent?> ent)
    {
        if (!_martialArts.TryGrantMartialArt(ent.Owner, _proto.Index(_sleepingCarp), []))
            return false;

        _faction.AddFaction(ent.Owner, _factionGrant);
        var userFactionIcons = EnsureComp<CustomFactionIconsComponent>(ent);
        userFactionIcons.FactionIcons.Add(_factionIconGrant);

        // only remove melee dodge if they couldnt already dodge melee
        if (!TryComp(ent, out AutoDodgeComponent? autoDodge))
        {
            autoDodge = EnsureComp<AutoDodgeComponent>(ent);
            autoDodge.DodgeMelee = false;
        }
        autoDodge.DodgeRanged = true;
        Dirty(ent, autoDodge);

        return true;
    }

    public bool AdvanceSleepingCarpLearning(Entity<MeleeWeaponComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        var ev = new BeforeMartialArtLearningKnowledgeAdded(_proto.Index(_sleepingCarp));
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return false;

        var student = new Entity<SleepingCarpStudentComponent>(ent, EnsureComp<SleepingCarpStudentComponent>(ent));

        if (_timing.CurTime < student.Comp.UseAgainTime)
        {
            _popup.PopupEntity(
                Loc.GetString("carp-scroll-waiting"),
                ent,
                student,
                PopupType.MediumCaution);
            return false;
        }

        student.Comp.CurrentStage++;
        student.Comp.UseAgainTime = _timing.CurTime + student.Comp.UseDelay;

        if (student.Comp.CurrentStage < student.Comp.LearnStage)
        {
            _popup.PopupEntity(
                Loc.GetString("carp-scroll-advance"),
                ent,
                ent,
                PopupType.Medium);
        }
        else
        {
            _popup.PopupEntity(
                Loc.GetString("carp-scroll-complete"),
                ent,
                student,
                PopupType.LargeCaution);
        }

        return true;
    }

    [SubscribeLocalEvent]
    private void OnGrantSleepingCarp(Entity<GrantSleepingCarpComponent> ent, ref UseInHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (ent.Comp.MaximumMasters <= ent.Comp.CurrentMasters)
        {
            _popup.PopupEntity(Loc.GetString("cqc-fail-used", ("manual", Identity.Entity(ent, EntityManager))),
            args.User,
            args.User);
            return;
        }

        if (!AdvanceSleepingCarpLearning(ent.Owner))
            return;

        if (!GrantSleepingCarp(ent.Owner))
            return;

        ent.Comp.CurrentMasters++;
    }
}

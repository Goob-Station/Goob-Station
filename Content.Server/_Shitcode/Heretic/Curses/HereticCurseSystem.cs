using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Heretic.EntitySystems;
using Content.Server.Medical;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Shitcode.Heretic.Curses;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics.Components;
using Content.Shared.Hands;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Verbs;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Shitcode.Heretic.Curses;

public sealed partial class HereticCurseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;

    [Dependency] private readonly SharedSolutionContainerSystem _soln = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly HereticRitualSystem _ritual = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRitualRuneComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        SubscribeLocalEvent<HereticCurseProviderComponent, CurseSelectedMessage>(OnCurseSelected);
        SubscribeLocalEvent<HereticCurseProviderComponent, HandDeselectedEvent>(OnHandDeselected);
        SubscribeLocalEvent<HereticCurseProviderComponent, GotUnequippedHandEvent>(OnHandUnequipped);
        SubscribeLocalEvent<HereticCurseProviderComponent, ItemToggledEvent>(OnToggle);

        InitializeCurses();
    }

    private void OnToggle(Entity<HereticCurseProviderComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated)
            CloseUi(ent);
    }

    private void OnHandDeselected(Entity<HereticCurseProviderComponent> ent, ref HandDeselectedEvent args)
    {
        CloseUi(ent);
    }

    private void OnHandUnequipped(Entity<HereticCurseProviderComponent> ent, ref GotUnequippedHandEvent args)
    {
        CloseUi(ent);
    }

    private void CloseUi(EntityUid uid)
    {
        _ui.CloseUi(uid, HereticCurseUiKey.Key);
    }

    private void OnCurseSelected(Entity<HereticCurseProviderComponent> ent, ref CurseSelectedMessage args)
    {
        if (!HasComp<HereticComponent>(args.Actor) && !HasComp<GhoulComponent>(args.Actor))
        {
            CloseUi(ent);
            return;
        }

        if (!ent.Comp.CursePrototypes.TryGetValue(args.Curse, out var curseData))
        {
            CloseUi(ent);
            return;
        }

        if (!TryGetEntity(args.Victim, out var victim))
        {
            CloseUi(ent);
            return;
        }

        if (!_toggle.IsActivated(ent.Owner))
        {
            CloseUi(ent);
            return;
        }

        var look = _lookup.GetEntitiesInRange<HereticRitualRuneComponent>(args.Actor.ToCoordinates(),
            3f,
            LookupFlags.StaticSundries);

        if (look.FirstOrNull() is not { } rune)
        {
            _popup.PopupEntity(Loc.GetString("feast-of-owls-eui-far-away"), args.Actor, args.Actor);
            CloseUi(ent);
            return;
        }

        if (!Exists(victim) || victim.Value == args.Actor || !TryComp(victim.Value, out DnaComponent? dna) ||
            dna.DNA == null || _status.HasStatusEffect(victim.Value, ent.Comp.CursedStatusEffect))
        {
            CurseCrewmember(ent, rune, args.Actor, false);
            return;
        }

        if (!CanCurse(victim.Value))
        {
            CurseCrewmember(ent, rune, args.Actor, false);
            return;
        }

        var dnaDict = GetDnaDict(rune, dna.DNA);

        if (!dnaDict.TryGetValue(dna.DNA, out var tuple))
        {
            _popup.PopupEntity(Loc.GetString("heretic-curse-provider-no-dna"), args.Actor, args.Actor);
            CurseCrewmember(ent, rune, args.Actor, false);
            return;
        }

        var (amount, set) = tuple;
        var totalTime =
            curseData.Time * GetBloodCurseMultiplier(amount, ent.Comp.MaxBloodAmount, ent.Comp.MaxBloodMultiplier);

        if (!curseData.Silent)
        {
            _popup.PopupEntity(Loc.GetString("heretic-curse-provider-cursed"),
                victim.Value,
                victim.Value,
                PopupType.LargeCaution);
        }

        _log.Add(LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(args.Actor)} has cursed {ToPrettyString(victim.Value)} with {args.Curse}");

        _status.TryUpdateStatusEffectDuration(victim.Value, args.Curse, totalTime);
        _status.TryUpdateStatusEffectDuration(victim.Value,
            ent.Comp.CursedStatusEffect,
            totalTime + ent.Comp.CurseDelay);

        _ritual.RitualSuccess(rune, args.Actor);

        foreach (var source in set)
        {
            if (TryComp(source, out PuddleComponent? puddle))
            {
                if (!_soln.ResolveSolution(source, puddle.SolutionName, ref puddle.Solution))
                    continue;

                puddle.Solution.Value.Comp.Solution.Contents.RemoveAll(x =>
                    x.Reagent.EnsureReagentData().Any(y => y is DnaData dnaData && dnaData.DNA == dna.DNA));

                if (puddle.Solution.Value.Comp.Solution.Contents.Count == 0)
                    QueueDel(source);
                else
                {
                    puddle.Solution.Value.Comp.Solution.Volume =
                        puddle.Solution.Value.Comp.Solution.Contents.Select(x => x.Quantity).Sum();
                    puddle.Solution.Value.Comp.Solution.HeatCapacityDirty = true;
                }
            }
            else if (TryComp(source, out ForensicsComponent? forensics))
            {
                forensics.DNAs.RemoveWhere(x => x.Item1 == dna.DNA);
                Dirty(source, forensics);
            }
        }

        CurseCrewmember(ent, rune, args.Actor, false);
    }

    private void OnGetVerbs(Entity<HereticRitualRuneComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!HasComp<HereticComponent>(args.User) && !HasComp<GhoulComponent>(args.User))
            return;

        if (!TryComp(args.Using, out HereticCurseProviderComponent? provider))
            return;

        var item = args.Using.Value;

        if (!_toggle.IsActivated(item))
            return;

        var user = args.User;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("heretic-curse-provider-curse"),
            Icon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/book_morbus.rsi"), "icon-on"),
            Act = () => CurseCrewmember((item, provider), ent, user, true),
        };

        args.Verbs.Add(verb);
    }

    private void CurseCrewmember(Entity<HereticCurseProviderComponent> provider,
        EntityUid rune,
        EntityUid user,
        bool popup)
    {
        var targets = GetTargets(provider, rune, user);
        if (targets.Count == 0)
        {
            CloseUi(provider);
            if (popup)
                _popup.PopupEntity(Loc.GetString("heretic-curse-provider-no-dna"), user, user);
            return;
        }

        _ui.TryOpenUi(provider.Owner, HereticCurseUiKey.Key, user);
        _ui.SetUiState(provider.Owner, HereticCurseUiKey.Key, new PickCurseVictimState(targets));
    }

    private HashSet<CurseData> GetTargets(Entity<HereticCurseProviderComponent> provider,
        EntityUid rune,
        EntityUid user)
    {
        var set = new HashSet<CurseData>();
        var dnaDict = GetDnaDict(rune);

        if (dnaDict.Count == 0)
            return set;

        var query = EntityQueryEnumerator<DnaComponent, HumanoidAppearanceComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var dna, out _, out var mobState))
        {
            if (uid == user || dna.DNA == null)
                continue;

            if (!dnaDict.TryGetValue(dna.DNA, out var tuple))
                continue;

            var multiplier =
                GetBloodCurseMultiplier(tuple.Item1, provider.Comp.MaxBloodAmount, provider.Comp.MaxBloodMultiplier);

            if (!CanCurse((uid, mobState)))
                multiplier = 0f;

            var time = TryComp(uid, out StatusEffectContainerComponent? container) &&
                       _status.TryGetTime(uid, provider.Comp.CursedStatusEffect, out var tuple2, container)
                ? tuple2.EndEffectTime ?? TimeSpan.MaxValue
                : TimeSpan.Zero;

            set.Add(new CurseData(GetNetEntity(uid), Name(uid), multiplier, time));
        }

        return set;
    }

    private Dictionary<string, (float, HashSet<EntityUid>)> GetDnaDict(EntityUid rune, string? lookDna = null)
    {
        var forensicsQuery = GetEntityQuery<ForensicsComponent>();
        var puddleQuery = GetEntityQuery<PuddleComponent>();

        Dictionary<string, (float, HashSet<EntityUid>)> dnaDict = new();
        var look = _lookup.GetEntitiesInRange(rune, 1.5f, LookupFlags.Uncontained);
        foreach (var ent in look)
        {
            if (puddleQuery.TryComp(ent, out var puddle))
            {
                foreach (var (dna, amount) in GetPuddleData(ent, puddle, lookDna))
                {
                    if (!dnaDict.TryGetValue(dna, out var tuple))
                        dnaDict[dna] = (amount, [ent]);
                    else
                    {
                        tuple.Item2.Add(ent);
                        dnaDict[dna] = (tuple.Item1 + amount, tuple.Item2);
                    }
                }
                continue;
            }

            if (!forensicsQuery.TryComp(ent, out var forensics))
                continue;

            foreach (var dna in forensics.DNAs)
            {
                if (lookDna != null && dna.Item1 != lookDna)
                    continue;

                if (!dnaDict.TryGetValue(dna.Item1, out var tuple))
                    dnaDict[dna.Item1] = (0f, [ent]);
                else
                    tuple.Item2.Add(ent);
            }
        }

        return dnaDict;
    }

    private IEnumerable<(string, float)> GetPuddleData(EntityUid uid, PuddleComponent puddle, string? lookDna)
    {
        if (!_soln.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution))
            yield break;

        foreach (var reagent in puddle.Solution.Value.Comp.Solution.Contents)
        {
            foreach (var data in reagent.Reagent.EnsureReagentData())
            {
                if (data is not DnaData dna)
                    continue;

                if (lookDna != null && dna.DNA != lookDna)
                    continue;

                yield return (dna.DNA, reagent.Quantity.Float());
            }
        }
    }

    private bool CanCurse(Entity<MobStateComponent?> uid)
    {
        return !_mobState.IsDead(uid) && !HasComp<HereticComponent>(uid) && !HasComp<GhoulComponent>(uid) &&
               !HasComp<WizardComponent>(uid) && !HasComp<ApprenticeComponent>(uid);
    }

    private static float GetBloodCurseMultiplier(float amount, float maxAmount, float maxMultiplier)
    {
        DebugTools.Assert(maxMultiplier >= 1f);
        DebugTools.Assert(maxAmount >= 0f);
        var multiplier = 1f + (maxMultiplier - 1f) / (1f + MathF.Exp(maxAmount / 2f - amount));
        multiplier = Math.Clamp(multiplier, 1f, maxMultiplier);
        return MathF.Round(multiplier, 1, MidpointRounding.ToPositiveInfinity);
    }
}

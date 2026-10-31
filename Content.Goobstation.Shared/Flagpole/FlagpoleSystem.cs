using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Timing;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Flagpole;

public sealed partial class FlagpoleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private readonly HashSet<Entity<MindShieldComponent>> _targets = new();
    private readonly Dictionary<(EntityUid Target, string Group), (EntityUid Source, TimeSpan Expiry)> _claims = new();
    private readonly List<(EntityUid Target, string Group)> _expired = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlagpoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FlagpoleComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<FlagpoleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnMapInit(Entity<FlagpoleComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Interval;
        Dirty(ent);
    }

    private void OnMeleeHit(Entity<FlagpoleComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.MeleeDamage.Empty)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<MindShieldComponent>(hit))
                continue;

            _damageable.TryChangeDamage(hit, ent.Comp.MeleeDamage, ent.Comp.IgnoreResistances, origin: args.User);
        }
    }

    private void OnGetVerbs(Entity<FlagpoleComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.UseRange <= 0f)
            return;

        var user = args.User;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("effect-on-mindshielded-propaganda"),
            Disabled = _useDelay.IsDelayed(ent.Owner),
            Act = () => Inspire(ent, user),
        });
    }

    private void Inspire(Entity<FlagpoleComponent> ent, EntityUid user)
    {
        if (!_useDelay.TryResetDelay(ent.Owner, true))
            return;

        if (ent.Comp.UseSpeech is { } speech)
        {
            var dataset = _proto.Index(speech);
            _chat.TrySendInGameICMessage(user, Loc.GetString(_random.Pick(dataset.Values)), InGameICChatType.Speak, false);
        }

        if (ent.Comp.ApplyOnCrit.Empty)
            return;

        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(user).Coordinates, ent.Comp.UseRange, _targets);

        foreach (var target in _targets)
        {
            if (!_mobState.IsCritical(target))
                continue;

            _damageable.TryChangeDamage(target.Owner, ent.Comp.ApplyOnCrit, ent.Comp.IgnoreResistances, origin: user);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<FlagpoleComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime >= comp.NextPrune)
            {
                comp.NextPrune = curTime + comp.PruneInterval;
                PruneClaims(curTime);
            }

            if (comp.Range <= 0f || curTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = curTime + comp.Interval;
            Dirty(uid, comp);

            ApplyArea((uid, comp));
        }
    }

    private void ApplyArea(Entity<FlagpoleComponent> ent)
    {
        var curTime = _timing.CurTime;

        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, _targets);

        foreach (var target in _targets)
        {
            if (_mobState.IsDead(target))
                continue;

            if (!TryClaim(ent, target.Owner, curTime))
            {
                continue;
            }

            if (!ent.Comp.Damage.Empty)
            {
                _damageable.TryChangeDamage(target.Owner, ent.Comp.Damage, ent.Comp.IgnoreResistances, origin: ent.Owner);
            }

            foreach (var (effect, duration) in ent.Comp.StatusEffects)
            {
                _statusEffects.TryAddStatusEffectDuration(target.Owner, effect, duration);
            }
        }
    }

    private bool TryClaim(Entity<FlagpoleComponent> ent, EntityUid target, TimeSpan curTime)
    {
        if (ent.Comp.AllowMultipleSources)
        {
            return true;
        }

        var key = (target, ent.Comp.StackGroup);

        if (_claims.TryGetValue(key, out var claim) && claim.Source != ent.Owner && curTime < claim.Expiry)
        {
            return false;
        }

        _claims[key] = (ent.Owner, curTime + ent.Comp.Interval * 2);
        return true;
    }

    private void PruneClaims(TimeSpan curTime)
    {
        _expired.Clear();

        foreach (var (key, claim) in _claims)
        {
            if (curTime >= claim.Expiry)
            {
                _expired.Add(key);
            }
        }

        foreach (var key in _expired)
        {
            _claims.Remove(key);
        }
    }
}

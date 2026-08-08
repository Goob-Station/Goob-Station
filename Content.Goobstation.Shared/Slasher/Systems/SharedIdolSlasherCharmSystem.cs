using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// The Slasher Idols lovestruck conversion. 
/// </summary>
public abstract class SharedIdolSlasherCharmSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementModStatusSystem _movemod = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherIdolComponent, DamageModifyEvent>(OnIdolDamageModify);
        SubscribeLocalEvent<MobStateComponent, DamageChangedEvent>(OnMobDamaged);
    }

    /// <summary>
    /// What happens when the charmed victim attacks their idol.
    /// </summary>
    private void OnIdolDamageModify(Entity<SlasherIdolComponent> ent, ref DamageModifyEvent args)
    {
        if (args.Origin is not { } origin
            || !TryComp<LovestruckComponent>(origin, out var lovestruck)
            || !IsCharmed(origin, lovestruck)
            || lovestruck.Idol != ent.Owner)
            return;

        args.Damage *= 1f - lovestruck.IdolDamageReduction;

        if (_net.IsClient || _timing.CurTime < lovestruck.NextApology)
            return;

        _chat.TrySendInGameICMessage(origin, Loc.GetString(_random.Pick(lovestruck.ApologyLines), ("name", Name(ent.Owner))), InGameICChatType.Speak, false);
        lovestruck.NextApology = _timing.CurTime + lovestruck.ApologyCooldown;
        Dirty(origin, lovestruck);
    }

    /// <summary>
    /// What happens when the victim gets attacked by an idol.
    /// </summary>
    private void OnMobDamaged(Entity<MobStateComponent> ent, ref DamageChangedEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !args.DamageIncreased
            || args.DamageDelta == null
            || args.Origin is not { } origin
            || !CanConvert(origin, ent.Owner))
            return;

        var lovestruck = EnsureComp<LovestruckComponent>(ent.Owner);
        if (lovestruck.Idol != origin)
        {
            lovestruck.Idol = origin;
            lovestruck.Accumulated = 0f;
        }

        lovestruck.Accumulated += args.DamageDelta.GetTotal().Float();
        Dirty(ent.Owner, lovestruck);

        if (lovestruck.Accumulated >= lovestruck.ConversionThreshold)
            TryConvert(origin, ent.Owner);
    }

    public bool TryConvert(EntityUid idol, EntityUid victim)
    {
        if (!CanConvert(idol, victim))
            return false;

        var lovestruck = EnsureComp<LovestruckComponent>(victim);
        lovestruck.Idol = idol;
        lovestruck.Accumulated = 0f;
        lovestruck.NextCheer = _timing.CurTime + _random.Next(lovestruck.MinCheerInterval, lovestruck.MaxCheerInterval);
        Dirty(victim, lovestruck);

        _statusEffects.TryAddStatusEffectDuration(victim, lovestruck.LovestruckStatusEffect, lovestruck.LovestruckDuration);

        _popup.PopupEntity(Loc.GetString("slasher-lovestruck-victim", ("idol", idol)), victim, victim, PopupType.LargeCaution);
        return true;
    }

    private bool CanConvert(EntityUid idol, EntityUid victim)
    {
        return idol != victim
               && HasComp<SlasherIdolComponent>(idol)
               && (!TryComp<LovestruckComponent>(victim, out var existing) || !IsCharmed(victim, existing))
               && !HasComp<SlasherComponent>(victim)
               && HasComp<HumanoidAppearanceComponent>(victim);
    }

    private bool IsCharmed(EntityUid victim, LovestruckComponent comp)
    {
        return _statusEffects.HasStatusEffect(victim, comp.LovestruckStatusEffect);
    }

    public void AnnounceAscension(EntityUid idol)
    {
        var query = EntityQueryEnumerator<LovestruckComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Idol != idol || !IsCharmed(uid, comp))
                continue;

            SendRadioCheer(uid, Loc.GetString(_random.Pick(comp.CheerLines), ("name", Name(idol))));
        }
    }

    protected virtual void SendRadioCheer(EntityUid source, string message)
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<LovestruckComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextCheer || !IsCharmed(uid, comp))
                continue;

            if (comp.Idol is not { } idol
                || TerminatingOrDeleted(idol)
                || !_mobState.IsAlive(uid))
                continue;

            if (!Transform(uid).Coordinates.TryDistance(EntityManager, Transform(idol).Coordinates, out var dist)
                || dist > comp.CheerRange)
                continue;

            _movemod.TryUpdateMovementSpeedModDuration(uid, comp.CharmEffect, TimeSpan.FromSeconds(comp.CheerDuration), 0.05f, 0.05f);

            if (_net.IsClient)
                continue;

            _chat.TrySendInGameICMessage(uid, Loc.GetString(_random.Pick(comp.CheerLines), ("name", Name(idol))), InGameICChatType.Speak, false);

            comp.NextCheer = curTime + _random.Next(comp.MinCheerInterval, comp.MaxCheerInterval);
            Dirty(uid, comp);
        }
    }
}

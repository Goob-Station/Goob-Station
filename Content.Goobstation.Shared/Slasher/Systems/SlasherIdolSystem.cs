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
/// Handles the idol slashers charming
/// </summary>
public abstract class SlasherIdolSystem : EntitySystem
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
        SubscribeLocalEvent<SlasherIdolComponent, SlasherSoulStealDoAfterEvent>(OnSoulStolen);
        SubscribeLocalEvent<MobStateComponent, DamageChangedEvent>(OnMobDamaged);
        SubscribeLocalEvent<SlasherAscendedEvent>(OnSlasherAscended);
    }

    private void OnIdolDamageModify(Entity<SlasherIdolComponent> ent, ref DamageModifyEvent args)
    {
        if (args.Origin is not { } fan
            || !TryComp<CharmedComponent>(fan, out var charmed)
            || charmed.Idol != ent.Owner
            || !IsCharmed(fan, charmed))
            return;

        args.Damage *= 1f - charmed.IdolDamageModifier;

        if (_net.IsClient || _timing.CurTime < charmed.NextApology)
            return;

        Say(fan, _random.Pick(charmed.ApologyLines), ent.Owner);
        charmed.NextApology = _timing.CurTime + charmed.ApologyCooldown;
        Dirty(fan, charmed);
    }

    private void OnSoulStolen(Entity<SlasherIdolComponent> ent, ref SlasherSoulStealDoAfterEvent args)
    {
        if (!args.Cancelled && args.Args.Target is { } victim)
            TryCharm(ent.Owner, victim);
    }

    private void OnMobDamaged(Entity<MobStateComponent> ent, ref DamageChangedEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !args.DamageIncreased
            || args.DamageDelta == null
            || args.Origin is not { } idol
            || !CanCharm(idol, ent.Owner))
            return;

        var charmed = EnsureComp<CharmedComponent>(ent.Owner);
        if (charmed.Idol != idol)
        {
            charmed.Idol = idol;
            charmed.Accumulated = 0f;
        }

        charmed.Accumulated += args.DamageDelta.GetTotal().Float();
        Dirty(ent.Owner, charmed);

        if (charmed.Accumulated >= charmed.ConversionThreshold)
            TryCharm(idol, ent.Owner);
    }

    private void OnSlasherAscended(SlasherAscendedEvent args)
    {
        if (!HasComp<SlasherIdolComponent>(args.Slasher))
            return;

        var query = EntityQueryEnumerator<CharmedComponent>();
        while (query.MoveNext(out var fan, out var comp))
        {
            if (comp.Idol == args.Slasher && IsCharmed(fan, comp))
                SendRadioCheer(fan, CheerLine(comp, args.Slasher));
        }
    }

    public bool TryCharm(EntityUid idol, EntityUid victim)
    {
        if (!CanCharm(idol, victim))
            return false;

        var charmed = EnsureComp<CharmedComponent>(victim);
        charmed.Idol = idol;
        charmed.Accumulated = 0f;
        charmed.NextCheer = _timing.CurTime + NextCheerDelay(charmed);
        Dirty(victim, charmed);

        _statusEffects.TryAddStatusEffectDuration(victim, charmed.CharmedStatusEffect, charmed.CharmedDuration);

        if (_net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("slasher-charmed-victim", ("idol", idol)),
                victim,
                victim,
                PopupType.LargeCaution);
        }

        return true;
    }

    private bool CanCharm(EntityUid idol, EntityUid victim)
    {
        return idol != victim
               && HasComp<SlasherIdolComponent>(idol)
               && HasComp<HumanoidAppearanceComponent>(victim)
               && !HasComp<SlasherComponent>(victim)
               && !(TryComp<CharmedComponent>(victim, out var existing) && IsCharmed(victim, existing));
    }

    private bool IsCharmed(EntityUid victim, CharmedComponent comp)
    {
        return _statusEffects.HasStatusEffect(victim, comp.CharmedStatusEffect);
    }

    protected virtual void SendRadioCheer(EntityUid fan, string message)
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CharmedComponent, TransformComponent>();
        while (query.MoveNext(out var fan, out var comp, out var xform))
        {
            if (curTime < comp.NextCheer
                || comp.Idol is not { } idol
                || TerminatingOrDeleted(idol)
                || !IsCharmed(fan, comp)
                || !_mobState.IsAlive(fan)
                || !xform.Coordinates.TryDistance(EntityManager, Transform(idol).Coordinates, out var distance)
                || distance > comp.CheerRange)
                continue;

            comp.NextCheer = curTime + NextCheerDelay(comp);
            Dirty(fan, comp);

            _movemod.TryUpdateMovementSpeedModDuration(fan, comp.CheerSlowdown, comp.CheerDuration, 0.05f, 0.05f);

            if (_net.IsServer)
                Say(fan, CheerLine(comp, idol));
        }
    }

    private TimeSpan NextCheerDelay(CharmedComponent comp)
    {
        return _random.Next(comp.MinCheerInterval, comp.MaxCheerInterval);
    }

    private string CheerLine(CharmedComponent comp, EntityUid idol)
    {
        return Loc.GetString(_random.Pick(comp.CheerLines), ("name", Name(idol)));
    }

    private void Say(EntityUid fan, LocId line, EntityUid idol)
    {
        Say(fan, Loc.GetString(line, ("name", Name(idol))));
    }

    private void Say(EntityUid fan, string message)
    {
        _chat.TrySendInGameICMessage(fan, message, InGameICChatType.Speak, false);
    }
}

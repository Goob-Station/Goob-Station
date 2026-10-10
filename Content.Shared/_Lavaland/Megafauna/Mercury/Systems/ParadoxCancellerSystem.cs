using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// On use in hand, save current position and current health values, then start a timer.
/// Once the timer finishes, return to saved position and return to whatever health values were on use of item.
/// Will techically "hurt" you if you healed during the rewind timer.
/// </summary>
public sealed class ParadoxCancellerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ParadoxCancellerComponent, UseInHandEvent>(OnUseInHand);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ParadoxCancellerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.RewindDeadline is null || _timing.CurTime < comp.RewindDeadline.Value)
                continue;

            DoRewind(uid, comp);
        }
    }

    private void OnUseInHand(EntityUid uid, ParadoxCancellerComponent comp, UseInHandEvent args)
    {
        if (args.Handled || comp.RewindDeadline is not null)
            return;

        if (!TryComp<DamageableComponent>(args.User, out var damageable))
            return;

        comp.HeldBy = args.User;

        comp.SavedPosition = Transform(args.User).Coordinates;
        comp.SavedDamage = new DamageSpecifier(damageable.Damage);
        comp.RewindDeadline = _timing.CurTime + TimeSpan.FromSeconds(comp.RewindTime);

        if (comp.MarkerPrototype is not null)
        {
            comp.MarkerEntity = PredictedSpawnAtPosition(comp.MarkerPrototype.Value, comp.SavedPosition.Value);
        }

        _audio.PlayPredicted(comp.StartSound, args.User, args.User, AudioParams.Default.WithVolume(-5f));
        _popup.PopupPredicted(Loc.GetString("paradox-canceller-start"), args.User, args.User, PopupType.Medium);

        // add trail comp
        if (comp.Trail is not null)
        {
            EntityManager.AddComponents(uid, comp.Trail);
        }

        args.Handled = true;
    }

    private void DoRewind(EntityUid uid, ParadoxCancellerComponent comp)
    {
        comp.RewindDeadline = null;

        if (comp.MarkerEntity.HasValue && _net.IsServer)
        {
            QueueDel(comp.MarkerEntity.Value);
        }
        comp.MarkerEntity = null;

        // remove trail
        if (comp.Trail is not null)
        {
            foreach (var (name, _) in comp.Trail)
            {
                var type = _factory.GetComponent(name).GetType();
                RemCompDeferred(uid, type);
            }
        }

        if (comp.HeldBy is not { } holder || !TryComp<DamageableComponent>(holder, out var damageable))
        {
            comp.SavedPosition = null;
            comp.SavedDamage = null;
            comp.HeldBy = null;
            return;
        }

        comp.HeldBy = null;

        // Only teleport if item is still being held
        var xform = Transform(uid);
        if (xform.ParentUid != holder)
        {
            comp.SavedPosition = null;
            comp.SavedDamage = null;
            return;
        }

        if (comp.SavedPosition.HasValue)
        {
            _transform.SetCoordinates(holder, comp.SavedPosition.Value);
        }

        if (comp.SavedDamage is not null)
        {
            var delta = new DamageSpecifier();

            foreach (var (type, saved) in comp.SavedDamage.DamageDict)
            {
                var current = damageable.Damage.DamageDict.TryGetValue(type, out var cur) ? cur : FixedPoint2.Zero;

                if (saved - current != FixedPoint2.Zero)
                {
                    delta.DamageDict[type] = saved - current;
                }
            }

            foreach (var (type, current) in damageable.Damage.DamageDict)
            {
                if (!comp.SavedDamage.DamageDict.ContainsKey(type) && current != FixedPoint2.Zero)
                {
                    delta.DamageDict[type] = -current;
                }
            }

            if (delta.DamageDict.Count > 0)
            {
                _damageable.TryChangeDamage(holder, delta, ignoreResistances: true);
            }
        }

        _audio.PlayPredicted(comp.RewindSound, holder, holder);
        _popup.PopupPredicted(Loc.GetString("paradox-canceller-rewind"), holder, holder, PopupType.LargeCaution);

        comp.SavedPosition = null;
        comp.SavedDamage = null;
    }
}

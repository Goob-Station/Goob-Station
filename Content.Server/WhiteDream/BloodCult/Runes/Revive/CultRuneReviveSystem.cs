using System.Linq;
using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;

namespace Content.Server.WhiteDream.BloodCult.Runes.Revive;

public sealed class CultRuneReviveSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;

    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuneReviveComponent, TryInvokeCultRuneEvent>(OnReviveRuneInvoked);
    }

    private void OnReviveRuneInvoked(Entity<CultRuneReviveComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        var chargesProvider = EnsureReviveRuneChargesProvider(ent);
        if (chargesProvider is null)
        {
            _popup.PopupEntity(Loc.GetString("cult-revive-rune-no-charges"), args.User, args.User);
            args.Cancel();
            return;
        }

        var possibleTargets = _cultRune.GetTargetsNearRune(ent,
            ent.Comp.ReviveRange,
            entity =>
                !HasComp<DamageableComponent>(entity) ||
                !HasComp<MobThresholdsComponent>(entity) ||
                !HasComp<MobStateComponent>(entity) ||
                _mobState.IsAlive(entity)
        );

        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-no-targets"), args.User, args.User);
            args.Cancel();
            return;
        }

        var victim = possibleTargets.First();

        if (chargesProvider.Charges == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-revive-rune-no-charges"), args.User, args.User);
            args.Cancel();
            return;
        }

        Revive(victim, args.User, ent);
    }

    public void AddCharges(EntityUid ent, int charges)
    {
        var chargesProvider = EnsureReviveRuneChargesProvider(ent);
        if (chargesProvider is null)
            return;

        chargesProvider.Charges += charges;
    }

    private void Revive(EntityUid target, EntityUid user, Entity<CultRuneReviveComponent> rune)
    {
        var chargesProvider = EnsureReviveRuneChargesProvider(rune);
        if (chargesProvider is null)
            return;

        chargesProvider.Charges--;

        var deadThreshold = _threshold.GetThresholdForState(target, MobState.Dead);
        _damageable.TryChangeDamage(target, rune.Comp.Healing);

        if (!TryComp<DamageableComponent>(target, out var damageable) || damageable.TotalDamage > deadThreshold)
            return;

        _mobState.ChangeMobState(target, MobState.Critical, origin: user);
        if (!_mind.TryGetMind(target, out _, out var mind))
        {
            // if the mind is not found in the body, try to find the original cultist mind
            if (TryComp<BloodCultistComponent>(target, out var cultist) && cultist.OriginalMind != null)
                mind = cultist.OriginalMind.Value;
        }

        if (mind?.Session is not { } playerSession || mind.CurrentEntity == target)
            return;

        // notify them they're being revived.
        _eui.OpenEui(new ReturnToBodyEui(mind, _mind), playerSession);
    }

    private ReviveRuneChargesProviderComponent? EnsureReviveRuneChargesProvider(EntityUid ent)
    {
        var mapUid = Transform(ent).MapUid;
        return !mapUid.HasValue ? null : EnsureComp<ReviveRuneChargesProviderComponent>(mapUid.Value);
    }
}

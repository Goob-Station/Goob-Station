using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._EE.Shadowling;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicon.Components;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.GameObjects;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Sonic Screech ability logic.
/// Sonic Screech "confuses" and "deafens" (flash effect + tinnitus sound) nearby people, damages windows, and stuns silicons/borgs. All in one pack!
/// </summary>
public sealed class ShadowlingSonicScreechSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingSonicScreechComponent, SonicScreechEvent>(OnSonicScreech);
    }

    private void OnSonicScreech(EntityUid uid, ShadowlingSonicScreechComponent component, SonicScreechEvent args)
    {
        _actions.StartUseDelay(args.Action);
        _popups.PopupEntity(Loc.GetString("shadowling-sonic-screech-complete"), uid, uid, PopupType.Medium);
        _audio.PlayPvs(component.ScreechSound, uid);

        var effectEnt = Spawn(component.SonicScreechEffect, _transform.GetMapCoordinates(uid));
        _transform.SetParent(effectEnt, uid);

        foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (_tag.HasTag(entity, component.WindowTag) &&
                TryComp<DamageableComponent>(entity, out var damageableComponent))
            {
                _damageable.TryChangeDamage(entity, component.WindowDamage, true, damageable: damageableComponent);
                continue;
            }

            if (!HasComp<MobStateComponent>(entity))
                continue;

            if (HasComp<ThrallComponent>(entity) ||
                HasComp<ShadowlingComponent>(entity))
                continue;

            if (HasComp<SiliconComponent>(entity))
            {
                _stun.TryParalyze(entity, component.SiliconStunTime, false);
                continue;
            }

            if (HasComp<HumanoidAppearanceComponent>(entity))
                EntityManager.SpawnAtPosition(component.ProtoFlash, Transform(entity).Coordinates);
        }
    }
}

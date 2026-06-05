using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Components;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class InfusedItemSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedRatvarianLanguageSystem _language = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedMansusGraspSystem _grasp = default!;
    [Dependency] private readonly SharedHereticSystem _heretic = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusInfusedComponent, ExaminedEvent>(OnInfusedExamine);
        SubscribeLocalEvent<MansusInfusedComponent, InteractHandEvent>(OnInfusedInteract);
        SubscribeLocalEvent<MansusInfusedComponent, MeleeHitEvent>(OnInfusedMeleeHit,
            after: new[] { typeof(SharedHereticBladeSystem) });
        SubscribeLocalEvent<MansusInfusedComponent, ComponentStartup>(OnInfusedStartup);
        SubscribeLocalEvent<MansusInfusedComponent, ComponentShutdown>(OnInfusedShutdown);
    }

    private void OnInfusedExamine(Entity<MansusInfusedComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mansus-infused-item-examine"));
    }

    private void OnInfusedInteract(Entity<MansusInfusedComponent> ent, ref InteractHandEvent args)
    {
        var target = args.User;

        if (_heretic.IsHereticOrGhoul(target))
            return;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(5f));
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        _hands.TryDrop(target, Transform(target).Coordinates);
        SpendInfusionCharges(ent);
    }

    private void OnInfusedMeleeHit(Entity<MansusInfusedComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!_heretic.TryGetHereticComponent(args.User, out var heretic, out _))
            return;

        var success = false;
        foreach (var target in args.HitEntities)
        {
            if (target == args.User)
                continue;

            if (!HasComp<StatusEffectsComponent>(target) && !HasComp<MobStateComponent>(target))
                continue;

            if (!_grasp.TryApplyGraspEffectAndMark(args.User, heretic, target, null, out _))
                continue;

            success = true;
        }

        if (success)
            SpendInfusionCharges(ent);
    }

    private void SpendInfusionCharges(Entity<MansusInfusedComponent> ent)
    {
        if (_net.IsClient)
            return;

        ent.Comp.AvailableCharges -= 1;
        if (ent.Comp.AvailableCharges <= 0)
            RemComp(ent.Owner, ent.Comp);
    }

    private void OnInfusedStartup(Entity<MansusInfusedComponent> ent, ref ComponentStartup args)
    {
        _appearance.SetData(ent, InfusedBladeVisuals.Infused, true);
        _item.SetHeldPrefix(ent, ent.Comp.HeldPrefix);
    }

    private void OnInfusedShutdown(Entity<MansusInfusedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _appearance.SetData(ent, InfusedBladeVisuals.Infused, false);
        _item.SetHeldPrefix(ent, null);
    }
}

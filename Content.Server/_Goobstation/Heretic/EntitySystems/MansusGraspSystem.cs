using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Heretic.Components;
using Content.Server.Speech.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._White.Standing;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class MansusGraspSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);

        SubscribeLocalEvent<MansusInfusedComponent, ExaminedEvent>(OnInfusedExamine);
        SubscribeLocalEvent<MansusInfusedComponent, InteractHandEvent>(OnInfusedInteract);
        SubscribeLocalEvent<MansusInfusedComponent, MeleeHitEvent>(OnInfusedMeleeHit);
        // todo add more mansus infused item interactions
    }

    private void OnAfterInteract(Entity<MansusGraspComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        if (!TryComp<HereticComponent>(args.User, out var hereticComp))
        {
            QueueDel(ent);
            args.Handled = true;
            return;
        }

        var target = (EntityUid) args.Target;

        if ((TryComp<HereticComponent>(args.Target, out var th) && th.CurrentPath == ent.Comp.Path))
            return;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _chat.TrySendInGameICMessage(args.User, Loc.GetString("heretic-speech-mansusgrasp"), InGameICChatType.Speak, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(3f), true);
            _stamina.TakeStaminaDamage(target, 80f);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        // upgraded grasp
        if (hereticComp.CurrentPath != null)
        {
            if (hereticComp.PathStage >= 2)
                ApplyGraspEffect(args.User, target, hereticComp.CurrentPath!);

            if (hereticComp.PathStage >= 4 && HasComp<StatusEffectsComponent>(target))
            {
                var markComp = EnsureComp<HereticCombatMarkComponent>(target);
                markComp.Path = hereticComp.CurrentPath;
            }
        }

        hereticComp.MansusGraspActive = false;
        QueueDel(ent);
        args.Handled = true;
    }

    private void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        var tags = ent.Comp.Tags;

        if (!args.CanReach
        || !args.ClickLocation.IsValid(EntityManager)
        || !TryComp<HereticComponent>(args.User, out var heretic) // not a heretic - how???
        || !heretic.MansusGraspActive // no grasp - not special
        || HasComp<ActiveDoAfterComponent>(args.User) // prevent rune shittery
        || !tags.Contains("Write") || !tags.Contains("Pen")) // not a pen
            return;

        // remove our rune if clicked
        if (args.Target != null && HasComp<HereticRitualRuneComponent>(args.Target))
        {
            // todo: add more fluff
            QueueDel(args.Target);
            return;
        }

        // spawn our rune
        var rune = Spawn("HereticRuneRitualDrawAnimation", args.ClickLocation);
        _transform.AttachToGridOrMap(rune);
        var dargs = new DoAfterArgs(EntityManager, args.User, 14f, new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            CancelDuplicate = false,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        // delete the animation rune regardless
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            _transform.AttachToGridOrMap(Spawn("HereticRuneRitual", ev.Coords));
    }

    public void ApplyGraspEffect(EntityUid performer, EntityUid target, string path)
    {
        if (!TryComp<HereticComponent>(performer, out var heretic))
            return;

        switch (path)
        {
            case "Ash":
                {
                    var timeSpan = TimeSpan.FromSeconds(5f);
                    _statusEffect.TryAddStatusEffect(target, TemporaryBlindnessSystem.BlindingStatusEffect, timeSpan, false, TemporaryBlindnessSystem.BlindingStatusEffect);
                    break;
                }

            case "Blade":
                {
                    if (heretic.PathStage >= 7 && HasComp<ItemComponent>(target))
                    {
                        // empowering blades and shit
                        var infusion = EnsureComp<MansusInfusedComponent>(target);
                        infusion.AvailableCharges = infusion.MaxCharges;
                        break;
                    }

                    // ultra stun if the person is looking away or laying down
                    var degrees = Transform(target).LocalRotation.Degrees - Transform(performer).LocalRotation.Degrees;
                    if (HasComp<LayingDownComponent>(target) // laying down
                    || (degrees >= 160 && degrees <= 210)) // looking back
                        _stamina.TakeStaminaDamage(target, 110f, immediate: true);
                    break;
                }

            case "Lock":
                {
                    if (!TryComp<DoorComponent>(target, out var door))
                        break;

                    if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                        _door.SetBoltsDown((target, doorBolt), false);

                    _door.StartOpening(target, door);
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hereticknock.ogg"), target);
                    break;
                }

            case "Flesh":
                {
                    if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState == Shared.Mobs.MobState.Dead)
                    {
                        var ghoul = EnsureComp<GhoulComponent>(target);
                        ghoul.BoundHeretic = performer;
                    }
                    break;
                }

            case "Void":
                {
                    if (TryComp<TemperatureComponent>(target, out var temp))
                        _temperature.ForceChangeTemperature(target, temp.CurrentTemperature - 20f, temp);
                    _statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(8), false);
                    break;
                }

            default:
                return;
        }
    }

    #region Infused items

    private void OnInfusedExamine(Entity<MansusInfusedComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mansus-infused-item-examine"));
    }
    private void OnInfusedInteract(Entity<MansusInfusedComponent> ent, ref InteractHandEvent args)
    {
        var target = args.User;

        if (HasComp<HereticComponent>(target) || HasComp<GhoulComponent>(target))
            return;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryParalyze(target, TimeSpan.FromSeconds(5f), true);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        if (TryComp<HandsComponent>(target, out var hands))
            _hands.TryDrop(target, Transform(target).Coordinates, handsComp: hands);

        SpendInfusionCharges(ent, charges: ent.Comp.MaxCharges); // spend all because RCHTHTRTH
    }
    private void OnInfusedMeleeHit(Entity<MansusInfusedComponent> ent, ref MeleeHitEvent args)
    {
        args.BonusDamage += args.BaseDamage; // double it.
        SpendInfusionCharges(ent);
    }

    private void SpendInfusionCharges(Entity<MansusInfusedComponent> ent, float charges = -1)
    {
        ent.Comp.AvailableCharges -= 1;
        if (ent.Comp.AvailableCharges <= 0)
            RemComp<MansusInfusedComponent>(ent);
    }

    #endregion
}

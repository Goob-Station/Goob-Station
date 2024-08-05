using Content.Server.Chat.Systems;
using Content.Server.Heretic.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Hands;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Linguini.Bundle.Errors;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Heretic;

public sealed partial class MansusGraspSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;

    public void ApplyGraspEffect(EntityUid target, string path)
    {
        switch (path)
        {
            case "Ash":
                break;

            case "Lock":
                {
                    if (!TryComp<DoorComponent>(target, out var door))
                        break;

                    if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                        _door.SetBoltsDown((target, doorBolt), false);

                    _door.StartOpening(target, door);
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/Goobstation/Heretic/hereticknock.ogg"), target);
                    break;
                }
            default:
                return;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MansusGraspComponent, DropAttemptEvent>(OnDrop);

        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);
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
            return;
        }

        var target = (EntityUid) args.Target;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _chat.TrySendInGameICMessage(args.User, Loc.GetString("heretic-speech-mansusgrasp"), InGameICChatType.Speak, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(3f), true);
            _stamina.TakeStaminaDamage(target, 65f);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        // upgraded grasp
        if (hereticComp.CurrentPath != null)
        {
            if (hereticComp.PathStage >= 2)
                ApplyGraspEffect(target, hereticComp.CurrentPath!);
            if (hereticComp.PathStage >= 3 && HasComp<StatusEffectsComponent>(target))
            {
                var markComp = EnsureComp<HereticCombatMarkComponent>(target);
                markComp.Path = hereticComp.CurrentPath;
            }
        }

        hereticComp.MansusGraspActive = false;
        QueueDel(ent);
    }

    private void OnDrop(Entity<MansusGraspComponent> ent, ref DropAttemptEvent args)
    {
        if (TryComp<HereticComponent>(args.Uid, out var hereticComp))
            hereticComp.MansusGraspActive = false;
        QueueDel(ent);
    }


    private void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;
        if (!args.ClickLocation.IsValid(EntityManager))
            return;

        var tags = ent.Comp.Tags;
        if (!tags.Contains("Write") || !tags.Contains("Pen"))
            return;

        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (!heretic.MansusGraspActive)
            return;

        var rune = Spawn("HereticRuneRitualDrawAnimation", args.ClickLocation);
        var dargs = new DoAfterArgs(EntityManager, args.User, 14f, new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        // no matter if it's canceled or not, delete that anim rune
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            Spawn("HereticRuneRitual", ev.Coords);
    }
}

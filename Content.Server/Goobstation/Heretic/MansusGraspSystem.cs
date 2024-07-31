using Content.Server.Chat.Systems;
using Content.Server.Heretic.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MansusGraspComponent, DroppedEvent>(OnDrop);

        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);
    }

    public void OnAfterInteract(Entity<MansusGraspComponent> ent, ref AfterInteractEvent args)
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

        if (HasComp<StatusEffectsComponent>(args.Target))
        {
            var target = (EntityUid) args.Target;
            _chat.TrySendInGameICMessage(args.User, Loc.GetString("heretic-speech-mansusgrasp"), InGameICChatType.Speak, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(3.5f), true);
            _stamina.TakeStaminaDamage(target, 80f);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        hereticComp.MansusGraspActive = false;
        QueueDel(ent);
    }

    public void OnDrop(Entity<MansusGraspComponent> ent, ref DroppedEvent args)
    {
        if (TryComp<HereticComponent>(args.User, out var hereticComp))
            hereticComp.MansusGraspActive = false;
        QueueDel(ent);
    }


    public void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

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
        var dargs = new DoAfterArgs(EntityManager, args.User, 30f, new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            CancelDuplicate = true
        };
        args.Handled = true;
        _doAfter.TryStartDoAfter(dargs);
    }
    public void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        // no matter if it's canceled or not, delete that anim rune
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            Spawn("HereticRuneRitual", ev.Coords);
    }
}

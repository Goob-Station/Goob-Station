using Content.Goobstation.Shared.Silicon.Components;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Intellicard;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using static Content.Shared.Movement.Systems.SharedContentEyeSystem;

namespace Content.Goobstation.Shared.Silicon;

[Serializable, NetSerializable]
public sealed partial class IntellicardDoAfterEvent : SimpleDoAfterEvent;

public abstract partial class SharedIntellicardExtrasSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly NameModifierSystem _nameMod = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private static readonly EntProtoId DefaultAi = "StationAiBrain";
    private readonly ProtoId<ChatNotificationPrototype> _downloadChatNotificationPrototype = "IntellicardDownload";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntellicardComponent, AfterInteractEvent>(OnHolderInteract);
        SubscribeLocalEvent<IntellicardableMindComponent, IntellicardDoAfterEvent>(OnIntellicardDoAfter);
    }

    private void OnHolderInteract(Entity<IntellicardComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!TryComp(args.Target, out IntellicardableMindComponent? intellicardableMind))
            return;
        if (!TryComp(args.Used, out StationAiHolderComponent? cardAiHolder))
            return;

        // this only exists after the borg has been inhabited at least once, stock positronic brains dont have it...
        // so we need to create it if its not there already (arguably a bug because other brains do have it on spawn...)
        if (!HasComp<MindContainerComponent>(args.Target))
            AddComp<MindContainerComponent>(args.Target.Value);
        TryComp(args.Target, out MindContainerComponent? targetMindContainer);

        // because of the stupid ai holder thing, need to remove the brain if the ai mind is destroyed due to suicide or something
        // otherwise a braindead ai clogs the card without any clear indicator.
        EntityUid? cardBrain = _slots.GetItemOrNull(ent.Owner, "station_ai_mind_slot");
        if (TryComp<MindContainerComponent>(cardBrain, out MindContainerComponent? cardMindContainer)
            && cardMindContainer.Mind is null)
        {
            _popup.PopupClient(Loc.GetString("intellicard-extras-contained-missing"), args.User, args.User, PopupType.MediumCaution);
            QueueDel(cardBrain);
            args.Handled = true;
            return;
        }

        var cardHasAi = _slots.CanEject(ent.Owner, args.User, cardAiHolder.Slot);
        var brainHasAi = targetMindContainer!.Mind is { };

        if (cardHasAi && brainHasAi)
        {
            _popup.PopupClient(Loc.GetString("intellicard-extras-target-occupied"), args.User, args.User, PopupType.Medium);
            args.Handled = true;
            return;
        }
        if (!cardHasAi && !brainHasAi)
        {
            _popup.PopupClient(Loc.GetString("intellicard-extras-target-empty"), args.User, args.User, PopupType.Medium);
            args.Handled = true;
            return;
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            cardHasAi ? ent.Comp.UploadTime * intellicardableMind.UploadTimeFactor : ent.Comp.DownloadTime * intellicardableMind.DownloadTimeFactor,
            new IntellicardDoAfterEvent(),
            args.Target,
            ent.Owner)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            BreakOnDropItem = true,
            MultiplyDelay = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnIntellicardDoAfter(Entity<IntellicardableMindComponent> ent, ref IntellicardDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is null || _net.IsClient)
            return;

        EntityUid targetUid = ent.Owner;
        EntityUid cardUid = args.Target.Value;

        if (!TryComp(cardUid, out StationAiHolderComponent? targetHolder))
            return;

        EntityUid? cardBrain = _slots.GetItemOrNull(cardUid, "station_ai_mind_slot");

        TryComp<MindContainerComponent>(cardBrain, out MindContainerComponent? cardMindContainer);

        if (!TryComp(cardUid, out StationAiHolderComponent? cardAiHolder))
            return;

        if (!TryComp(targetUid, out MindContainerComponent? targetMindContainer))
            return;

        // get mind status of both
        var cardHasAi = _slots.CanEject(cardUid, args.User, cardAiHolder.Slot) && cardMindContainer is { } && cardMindContainer.Mind is { };
        var targetHasAi = targetMindContainer.Mind is { };

        // Card -> Brain
        // upload the mind from the positronic brain inside the card's StationAiHolder into the target brain
        if (cardHasAi && !targetHasAi)
        {
            if (cardMindContainer!.Mind is not { } cardMind)
                return;

            _metaData.SetEntityName(targetUid, _nameMod.GetBaseName(cardBrain!.Value));

            _mind.TransferTo(cardMind, targetUid, ghostCheckOverride: true);
            _mind.UnVisit(cardMind);

            QueueDel(cardBrain); // free up the empty brain

            _audio.PlayPvs(cardAiHolder.Slot.InsertSound, targetUid);

            args.Handled = true;
            return;
        }

        // Brain -> Card
        // create positronic brain for StationAiHolder and then download the mind from the target brain
        if (!cardHasAi && targetHasAi)
        {
            if (targetMindContainer.Mind is not { } targetMind)
                return;
            var newCardBrain = SpawnInContainerOrDrop(DefaultAi, cardUid, StationAiCoreComponent.Container);
            _metaData.SetEntityName(newCardBrain, _nameMod.GetBaseName(targetUid));

            _mind.TransferTo(targetMind, newCardBrain, ghostCheckOverride: true);
            _mind.UnVisit(targetMind);

            _audio.PlayPvs(cardAiHolder.Slot.InsertSound, cardUid);

            // for some godforsaken reason the fov gets fucked
            _eye.SetDrawFov(newCardBrain, true);

            args.Handled = true;
            return;
        }
    }
}

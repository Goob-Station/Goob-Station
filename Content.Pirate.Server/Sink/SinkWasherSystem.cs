using Content.Pirate.Server.Stains;
using Content.Pirate.Shared.Sink;
using Content.Pirate.Shared.Stains.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Pirate.Server.Sink;

/// <summary>
/// Washes stained held items or gloves/bare hands at sinks.
/// </summary>
public sealed class SinkWasherSystem : EntitySystem
{
    private const string GlovesSlot = "gloves";

    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly StainSystem _stains = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SinkWasherComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SinkWasherComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<SinkWasherComponent, SinkWashDoAfterEvent>(OnWashDoAfter);
    }

    private void OnInteractUsing(Entity<SinkWasherComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<StainableComponent>(args.Used) || !_stains.HasStain(args.Used))
            return;

        args.Handled = TryStartWash(ent, args.User, args.Used);
    }

    private void OnInteractHand(Entity<SinkWasherComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !HandsNeedWashing(args.User))
            return;

        args.Handled = TryStartWash(ent, args.User, args.User);
    }

    private bool TryStartWash(Entity<SinkWasherComponent> ent, EntityUid user, EntityUid target)
    {
        var started = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            user,
            ent.Comp.WashDuration,
            new SinkWashDoAfterEvent(),
            ent.Owner,
            target: target,
            used: ent.Owner)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        });

        if (started)
            _audio.PlayPvs(ent.Comp.WashSound, ent.Owner);

        return started;
    }

    private void OnWashDoAfter(Entity<SinkWasherComponent> ent, ref SinkWashDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        args.Handled = true;

        var target = args.Target.Value;
        var cleaned = target == args.User
            ? WashHands(args.User)
            : _stains.TryCleanStain(target);

        if (cleaned)
            _popup.PopupEntity(Loc.GetString("stain-cleaned"), target, args.User);
    }

    private bool WashHands(EntityUid user)
    {
        if (_inventory.TryGetSlotEntity(user, GlovesSlot, out var gloves) && _stains.HasStain(gloves.Value))
            return _stains.TryCleanStain(gloves.Value);

        return _stains.TryCleanBodyStain(user, SlotFlags.GLOVES);
    }

    private bool HandsNeedWashing(EntityUid user)
    {
        if (_inventory.TryGetSlotEntity(user, GlovesSlot, out var gloves) && _stains.HasStain(gloves.Value))
            return true;

        return TryComp<StainableComponent>(user, out var stainable) && (stainable.BodyStainSlots & SlotFlags.GLOVES) != 0;
    }
}

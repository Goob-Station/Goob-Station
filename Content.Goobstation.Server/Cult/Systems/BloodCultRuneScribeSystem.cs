using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using System.Numerics;

namespace Content.Goobstation.Server.Cult.Systems;

public sealed partial class BloodCultRuneScribeSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    // placeholder
    public static readonly SoundSpecifier ScribeSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_scribe.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuneScribeComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, BloodCultRuneScribeSelectRuneMessage>(OnSelectRuneMessage);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, RuneScribeDoAfter>(OnRuneScribeDoAfter);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, RuneScribeRemoveDoAfter>(OnRuneScribeRemoveDoAfter);
        SubscribeLocalEvent<BloodCultRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnUseInHand(Entity<BloodCultRuneScribeComponent> ent, ref UseInHandEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User)
        || HasComp<ActiveDoAfterComponent>(args.User))
            return;

        _ui.TryOpenUi(ent.Owner, EntityRadialMenuKey.Key, args.User);
    }

    private void OnSelectRuneMessage(Entity<BloodCultRuneScribeComponent> ent, ref BloodCultRuneScribeSelectRuneMessage args)
    {
        var da = new DoAfterArgs(EntityManager, args.Actor, 2.5f, new RuneScribeDoAfter(args.Rune), ent, ent)
        {
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
        };
        _doAfter.TryStartDoAfter(da);
        _aud.PlayPvs(ent.Comp.ScribeSound, ent, AudioParams.Default);
    }

    private void OnRuneScribeDoAfter(Entity<BloodCultRuneScribeComponent> ent, ref RuneScribeDoAfter args)
    {
        var rune = args.Cancelled ? ent.Comp.MalfRune : args.Rune;
        var xform = Transform(ent).Coordinates;
        var pos = xform.WithPosition(xform.Position.Floored() + new Vector2(0.5f, 0.5f));

        Spawn(rune, _xform.ToMapCoordinates(pos));
    }

    private void OnRuneScribeRemoveDoAfter(Entity<BloodCultRuneScribeComponent> ent, ref RuneScribeRemoveDoAfter args)
    {
        if (args.Cancelled) return;
        QueueDel(ent);
    }

    private void OnInteractUsing(Entity<BloodCultRuneComponent> ent, ref InteractUsingEvent args)
    {
        // can't use as a cultist
        if (!TryComp<BloodCultistComponent>(args.User, out var cultie)
        || !TryComp<BloodCultRuneScribeComponent>(args.Used, out var scribe))
            return;

        var da = new DoAfterArgs(EntityManager, args.User, 1f, new RuneScribeRemoveDoAfter(ent), ent, ent, args.Used)
        {
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
        };
        _doAfter.TryStartDoAfter(da);

        args.Handled = true;
    }
}

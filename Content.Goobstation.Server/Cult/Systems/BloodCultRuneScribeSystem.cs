using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Numerics;

namespace Content.Goobstation.Server.Cult.Systems;

public sealed partial class BloodCultRuneScribeSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;

    // placeholder
    public static readonly SoundSpecifier ScribeSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/bloodcult_scribe.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuneScribeComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, BloodCultRuneScribeSelectRuneMessage>(OnSelectRuneMessage);
        SubscribeLocalEvent<BloodCultRuneScribeComponent, RuneScribeDoAfter>(OnRuneScribeDoAfter);
        SubscribeLocalEvent<BloodCultRuneComponent, RuneScribeRemoveDoAfter>(OnRuneRemoveDoAfter);
        SubscribeLocalEvent<BloodCultRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnExamined(Entity<BloodCultRuneScribeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("rune-scribe-examine"));
    }

    private void OnUseInHand(Entity<BloodCultRuneScribeComponent> ent, ref UseInHandEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User)
        || HasComp<ActiveDoAfterComponent>(args.User))
            return;

        if (!_bloodCultRule.TryGetRule(out var rule))
            return;

        var tiers = ent.Comp.Runes.Where(q => q.Key >= rule!.Value.Comp.CurrentTier).ToList();
        var runes = new List<EntProtoId>();
        foreach (var tier in tiers) runes.AddRange(tier.Value);
        ent.Comp.KnownRunes = runes;

        _ui.TryOpenUi(ent.Owner, EntityRadialMenuKey.Key, args.User);
    }

    private void OnSelectRuneMessage(Entity<BloodCultRuneScribeComponent> ent, ref BloodCultRuneScribeSelectRuneMessage args)
    {
        _popup.PopupEntity(Loc.GetString("rune-scribe-start"), ent, ent);
        _aud.PlayPvs(new SoundCollectionSpecifier("ScalpelCut"), ent, AudioParams.Default);
        var da = new DoAfterArgs(EntityManager, args.Actor, 2.5f, new RuneScribeDoAfter(args.Rune), ent, ent)
        {
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
        };
        _doAfter.TryStartDoAfter(da);
    }

    private void OnRuneScribeDoAfter(Entity<BloodCultRuneScribeComponent> ent, ref RuneScribeDoAfter args)
    {
        var rune = args.Cancelled ? ent.Comp.MalfRune : args.Rune;
        var transform = Transform(args.User);
        var runeEntity = Spawn(rune, _xform.GetMapCoordinates(args.User));
        _xform.SetLocalPosition(runeEntity, transform.LocalPosition.Floored() + new Vector2(0.5f, 0.5f));
        _xform.SetLocalRotation(runeEntity, Angle.Zero); // fuck you
        _aud.PlayPvs(new SoundCollectionSpecifier("ScalpelCut"), ent, AudioParams.Default);
    }

    private void OnRuneRemoveDoAfter(Entity<BloodCultRuneComponent> ent, ref RuneScribeRemoveDoAfter args)
    {
        if (args.Cancelled) return;
        QueueDel(args.Target);
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

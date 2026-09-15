using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Server.Changeling;
using Content.Goobstation.Shared.SpecialAnimation;
using Content.Goobstation.Shared.Voidwalker;
using Content.Shared.Administration.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Polymorph.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stealth;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Voidwalker;
public sealed partial class VoidwalkerSystem : SharedVoidwalkerSystem
{
    [Dependency] private readonly DamageableSystem _damage = null!;
    [Dependency] private readonly DoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly ChangelingSystem _changeling = null!; // easier than remaking the code of two lines lol

    private readonly ResPath _mapPath = new("Maps/_Goobstation/Nonstations/voidwalkervoid.yml");
    private static Entity<MapComponent>? _theVoid;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, GridUidChangedEvent>(OnGridUidChanged);
        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);

        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<Shared.Voidwalker.Components.VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);

        SubscribeAbilities();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<Shared.Voidwalker.Components.VoidwalkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Check if spaced.
            if (curTime > comp.NextSpacedCheck)
            {
                UpdateSpacedStatus((uid, comp));
                comp.NextSpacedCheck = curTime + comp.SpacedCheckInterval;
            }

            // Healing tick
            if (curTime > comp.NextHealingTick
                && comp.IsInSpace)
            {
                if (comp.HealingWhenSpaced is { } healing)
                    _damage.TryChangeDamage(uid, healing);

                comp.NextHealingTick = curTime + comp.HealingTickInterval;
            }
        }
    }

    #region Event Handlers

    private void OnInit(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref MapInitEvent args)
    {
        // Load THE VOID map if not already loaded
        if (_theVoid == null
            && _mapLoader.TryLoadMap(_mapPath,
                out _theVoid,
                out _,
                new DeserializationOptions { InitializeMaps = true }))
            _map.SetPaused(_theVoid.Value.Comp.MapId, false);

        UpdateSpacedStatus(entity);
        _meta.AddFlag(entity, MetaDataFlags.ExtraTransformEvents);
    }

    private void OnCleanup(RoundRestartCleanupEvent args)
    {
        if (_theVoid is not null)
            QueueDel(_theVoid);

        _theVoid = null;
    }

    private void OnGridUidChanged(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref GridUidChangedEvent args) =>
        UpdateSpacedStatus(entity);

    private void OnExamined(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.UnsettleDoAfterId is not { } doAfterId)
            return;

        _doAfter.Cancel(entity, doAfterId);
        entity.Comp.UnsettleDoAfterId = null;

        var popup = Loc.GetString("voidwalker-unsettle-fail-looked-at");
        _popup.PopupEntity(popup, entity, entity, PopupType.MediumCaution);
    }

    private void OnGetVerbs(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;

        if (!args.CanInteract
            || !args.CanAccess)
            return;

        if (entity.Comp.IsInSpace
            && _changeling.IsHardGrabbed(target))
        {
            InnateVerb kidnapVerb = new()
            {
                Act = () => StartKidnap(entity, target),
                Text = Loc.GetString("voidwalker-kidnap-verb"),
                Message = Loc.GetString("voidwalker-kidnap-verb-text"),
                Icon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Actions/voidwalker.rsi"), "kidnap"),
                Priority = 1,
            };

            args.Verbs.Add(kidnapVerb);
        }

        if (_tag.HasTag(target, entity.Comp.WallTag)
            && !_tag.HasTag(target, entity.Comp.VoidedStructureTag))
        {
            InnateVerb convertWallVerb = new()
            {
                Act = () => StartConvertWall(entity, target),
                Text = Loc.GetString("voidwalker-convert-wall-verb"),
                Message = Loc.GetString("voidwalker-convert-wall-text"),
                Icon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Actions/voidwalker.rsi"), "kidnap"),
                Priority = 1,
            };

            args.Verbs.Add(convertWallVerb);
        }


    }

    /// <summary>
    /// We apply pressure immunity to a target being dragged by a voidwalker so they have time to kidnap them
    /// without them dying to pressure.
    /// </summary>
    private void OnPullStarted(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref PullStartedMessage args)
    {
        entity.Comp.EntityPulledWasSpaceImmune = HasComp<SpecialPressureImmunityComponent>(args.PulledUid);

        if (HasComp<AtmosExposedComponent>(args.PulledUid))
            EnsureComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    private void OnPullStopped(Entity<Shared.Voidwalker.Components.VoidwalkerComponent> entity, ref PullStoppedMessage args)
    {
        if (!entity.Comp.EntityPulledWasSpaceImmune)
            RemComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    #endregion


}

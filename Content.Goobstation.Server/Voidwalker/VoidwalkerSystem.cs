using System.Collections.Immutable;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Server.Changeling;
using Content.Goobstation.Shared.Voidwalker;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Shared.Administration.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stealth;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Traits.Assorted;
using Content.Shared.Verbs;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Voidwalker;
public sealed partial class VoidwalkerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = null!;
    [Dependency] private readonly ChatSystem _chat = null!;
    [Dependency] private readonly DamageableSystem _damage = null!;
    [Dependency] private readonly DoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = null!;
    [Dependency] private readonly SharedSlurredSystem _slurred = null!;
    [Dependency] private readonly SharedStaminaSystem _stamina = null!;
    [Dependency] private readonly SharedStunSystem _stun = null!;
    [Dependency] private readonly PolymorphSystem _polymorph = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedStealthSystem _stealth = null!;
    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly ThrowingSystem _throwing = null!;
    [Dependency] private readonly ChangelingSystem _changeling = null!; // easier than remaking the code of two lines lol

    /// <summary>
    /// If the voidwalker is within this much of a passed object, don't count it as being in space.
    /// This is to prevent being able to stand inside a passed object, since they have no atmosphere inside.
    /// If you can think of a better way to handle this, do tell me - delph
    /// </summary>
    private const float PassedObjectGraceRange = 1; //

    private readonly ResPath _mapPath = new("Maps/_Goobstation/Nonstations/voidwalkervoid.yml");
    private static Entity<MapComponent>? _theVoid;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        SubscribeLocalEvent<VoidwalkerComponent, GridUidChangedEvent>(OnGridUidChanged);
        SubscribeLocalEvent<VoidwalkerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<VoidwalkerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);

        SubscribeLocalEvent<VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);

        SubscribeAbilities();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<VoidwalkerComponent>();
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

    private void OnInit(Entity<VoidwalkerComponent> entity, ref MapInitEvent args)
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

    private void OnGridUidChanged(Entity<VoidwalkerComponent> entity, ref GridUidChangedEvent args) =>
        UpdateSpacedStatus(entity);

    private void OnExamined(Entity<VoidwalkerComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.UnsettleDoAfterId is not { } doAfterId)
            return;

        _doAfter.Cancel(entity, doAfterId);
        entity.Comp.UnsettleDoAfterId = null;

        var popup = Loc.GetString("voidwalker-unsettle-fail-looked-at");
        _popup.PopupEntity(popup, entity, entity, PopupType.MediumCaution);
    }

    private void OnGetVerbs(Entity<VoidwalkerComponent> entity, ref GetVerbsEvent<InnateVerb> args)
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
    private void OnPullStarted(Entity<VoidwalkerComponent> entity, ref PullStartedMessage args)
    {
        entity.Comp.EntityPulledWasSpaceImmune = HasComp<SpecialPressureImmunityComponent>(args.PulledUid);

        if (HasComp<AtmosExposedComponent>(args.PulledUid))
            EnsureComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    private void OnPullStopped(Entity<VoidwalkerComponent> entity, ref PullStoppedMessage args)
    {
        if (!entity.Comp.EntityPulledWasSpaceImmune)
            RemComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    #endregion

    #region Updating Spaced Status

    public void UpdateSpacedStatus(Entity<VoidwalkerComponent> entity)
    {
        var isInSpace = CheckInSpace(entity.Owner, entity.Comp);
        entity.Comp.IsInSpace = isInSpace;

        var ev = new VoidwalkerSpacedStatusChangedEvent(isInSpace);
        RaiseLocalEvent(entity, ref ev);
    }

    public bool CheckInSpace(EntityUid uid, VoidwalkerComponent? voidwalker = null)
    {
        var entityXform = Transform(uid);

        // Check if the voidwalker is standing inside a passed object.
        // is this hacky? Yes. Very.
        if (voidwalker is not null
            && TryComp<GlassPasserComponent>(uid, out var glassPasser))
            foreach (var (entityPassed, _) in glassPasser.EntitiesPassed)
                if (_transform.InRange(uid, entityPassed, PassedObjectGraceRange))
                    return false;

        // If the voidwalker is not on a grid, it is in space.
        if (entityXform.GridUid is not { } gridUid)
            return true;

        // If the voidwalker *is* on a grid, but the grid has no atmosphere; it is in space.
        var position = _transform.GetGridOrMapTilePosition(uid);
        var tileMixture = _atmos.GetTileMixture(gridUid, entityXform.MapUid, position);

        return tileMixture is null || tileMixture.Pressure <= 0;
    }

    #endregion


    #region Helpers

    public bool CanSeeVoidwalker(EntityUid target)
    {
        if (HasComp<PermanentBlindnessComponent>(target)
            || HasComp<TemporaryBlindnessComponent>(target))
            return false;

        return !TryComp<BlindableComponent>(target, out var blindable)
               || blindable.EyeDamage < blindable.MaxDamage;
    }

    public bool TryUseAbility(Entity<VoidwalkerComponent> voidwalker, BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        UpdateSpacedStatus(voidwalker);

        if (!TryComp<VoidwalkerActionComponent>(action.Action, out var voidwalkerAction))
            return false;

        if (voidwalkerAction.RequireInSpace
            && !voidwalker.Comp.IsInSpace)
        {
            var popup = Loc.GetString("voidwalker-action-fail-require-in-space");
            _popup.PopupEntity(popup, voidwalker, voidwalker);

            return false;
        }

        action.Handled = true;

        return true;
    }

    public bool TrySendToShadowRealm(EntityUid target)
    {
        var popup = Loc.GetString("voidwalker-kidnap-enter");
        _popup.PopupEntity(popup, target, target, PopupType.SmallCaution);

        if (!TryComp<MindContainerComponent>(target, out var targetMindContainer)
            || !targetMindContainer.HasMind)
            return false;

        var targetMind = Comp<MindComponent>(targetMindContainer.Mind.Value);
        targetMind.PreventGhosting = true;

        var spawnPoints = EntityManager
            .GetAllComponents(typeof(VoidedSpawnComponent))
            .ToImmutableList();

        if (spawnPoints.IsEmpty)
            return false;

        var newSpawn = _random.Pick(spawnPoints);
        var spawnTarget = Transform(newSpawn.Uid).Coordinates;

        _transform.SetCoordinates(target, spawnTarget);
        _rejuvenate.PerformRejuvenate(target);
        _stun.KnockdownOrStun(target, TimeSpan.FromSeconds(5), true);
        // need more sfx here later

        return true;
    }

    #endregion

}

using Content.Goobstation.Shared.Overlays;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Thrall;
using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Pinpointer;
using Content.Server.Polymorph.Systems;
using Content.Server.Station.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles the Ascension Egg system.
/// </summary>
public sealed class ShadowlingAscensionEggSystem : EntitySystem
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _globalSound = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscensionEggComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<ShadowlingAscensionEggComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<ShadowlingAscensionEggComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadowlingAscensionEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.StartTimer)
                continue;

            if (_timing.CurTime >= comp.NextUpdateTime - comp.AscendingEffectInterval && !comp.AscendingEffectAdded)
            {
                var effectEnt = Spawn(comp.AscendingEffect, _transform.GetMapCoordinates(uid));
                _transform.SetParent(effectEnt, uid);
                comp.AscendingEffectEntity = effectEnt;
                comp.AscendingEffectAdded = true;
            }

            if (_timing.CurTime < comp.NextUpdateTime)
                continue;

            DoAscend(uid, comp);
        }
    }

    private void OnGetVerbs(EntityUid uid, ShadowlingAscensionEggComponent component, GetVerbsEvent<Verb> args)
    {
        if (args.User != component.Creator)
            return;

        args.Verbs.Add(
            new Verb
            {
                Act = () => TryAscend(args.User, args.Target, component),
                Text = Loc.GetString(component.VerbName),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")) //todo: custom icon
            });
    }

    private void OnDestruction(EntityUid uid, ShadowlingAscensionEggComponent component, DestructionEventArgs args)
    {
        if (component.AscendingEffectEntity != null)
            QueueDel(component.AscendingEffectEntity);

        if (component.ShadowlingInsideEntity != null)
            QueueDel(component.ShadowlingInsideEntity);

        if (component.Creator == null
            || !component.StartTimer) // This indicates that the shadowling was inside the egg
            return;

        var shadowlingComp = EntityManager.GetComponent<ShadowlingComponent>(component.Creator.Value);
        _shadowling.OnPhaseChanged(component.Creator.Value, shadowlingComp, ShadowlingPhases.FailedAscension);
        component.StartTimer = false;
    }

    private void OnExamined(EntityUid uid, ShadowlingAscensionEggComponent component, ExaminedEvent args)
    {
        if (!component.StartTimer && component.Creator == args.Examiner)
            args.PushMarkup($"[color=red]{Loc.GetString("shadowling-ascension-start-warning")}[/color]");
    }

    private void TryAscend(EntityUid uid, EntityUid eggUid, ShadowlingAscensionEggComponent component)
    {
        if (!HasComp<ShadowlingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-ascension-not-shadowling"), uid, uid, PopupType.MediumCaution);
            return;
        }

        if (component.Creator == null)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-ascension-not-creator"), uid, uid, PopupType.MediumCaution);
            return;
        }

        // Don't ascend if another shadowling is ascending.
        // If one shadowling finishes ascension, the remaining
        // shadowlings will gain Ascendance powers too, either way.
        var entQuery = EntityQueryEnumerator<ShadowlingComponent>();
        while (entQuery.MoveNext(out _, out var sling))
        {
            if (sling.CurrentPhase == ShadowlingPhases.Ascension)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-ascension-already-ascended"), uid, uid, PopupType.MediumCaution);
                return;
            }

            if (sling.IsAscending)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-ascension-ascending"), uid,uid, PopupType.MediumCaution);
                return;
            }
        }

        // Start Ascension
        var shadowling = Comp<ShadowlingComponent>(uid);

        // Dont take damage during hatching
        //EnsureComp<GodmodeComponent>(uid);
        // NO. PLEASE NO. DON'T DO IT PLEASE I BEG YOU. PLEEEEASEEEEE AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA - Rouden
        // eat my mango
        _damageable.SetDamageModifierSetId(uid, "ShadowlingAscending");

        shadowling.IsAscending = true;
        component.StartTimer = true;
        component.NextUpdateTime = _timing.CurTime + component.UpdateInterval;

        _entityStorage.Insert(uid, eggUid);

        _audio.PlayPvs(component.AscensionEnterSound, eggUid, AudioParams.Default.WithVolume(-1f));

        var position = _transform.GetMapCoordinates(uid);
        var message = Loc.GetString(
            "shadowling-ascension-message",
            ("location", FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(position))));

        _chatSystem.DispatchStationAnnouncement(eggUid, message, "Central Command", false, null, Color.Red);

        var stationUid = _station.GetStationInMap(Transform(uid).MapID);
        if (stationUid != null)
            _alertLevel.SetLevel(stationUid.Value, "delta", true, true, true, true);

        var effectEnt = Spawn(component.ShadowlingInside, Transform(eggUid).Coordinates);
        component.ShadowlingInsideEntity = effectEnt;
    }

    private void DoAscend(EntityUid uid, ShadowlingAscensionEggComponent component)
    {
        if (component.ShadowlingInsideEntity != null)
            QueueDel(component.ShadowlingInsideEntity);

        if (component.Creator == null)
            return;

        component.StartTimer = false;

        var lights = EntityQueryEnumerator<PoweredLightComponent>();
        while (lights.MoveNext(out var light, out var lightComp))
        {
            _poweredLight.TryDestroyBulb(light, lightComp);
        }

        _entityStorage.OpenStorage(uid);
        _entityStorage.Remove(component.Creator.Value, uid);

        var shadowlings = new List<EntityUid>();
        var thralls = new List<EntityUid>();

        var query = EntityQueryEnumerator<ShadowlingComponent>();
        while (query.MoveNext(out var slingUid, out _))
        {
            shadowlings.Add(slingUid);
        }

        var queryThrall = EntityQueryEnumerator<ThrallComponent>();
        while (queryThrall.MoveNext(out var thrallUid, out _))
        {
            thralls.Add(thrallUid);
        }

        foreach (var sling in shadowlings)
        {
            var newUid = _polymorph.PolymorphEntity(sling, "ShadowlingAscendantPolymorph");

            if (newUid == null
                || !TryComp<ShadowlingComponent>(newUid.Value, out var ascendant))
                continue;

            ascendant.CurrentPhase = ShadowlingPhases.Ascension;
            _shadowling.OnPhaseChanged(newUid.Value, ascendant, ShadowlingPhases.Ascension);

            _actions.RemoveAction(ascendant.ActionHatchEntity);
        }

        var nightmareComps = _protoMan.Index("NightmareAbilities");
        foreach (var thrall in thralls)
        {
            if (HasComp<LesserShadowlingComponent>(thrall))
            {
                EntityManager.AddComponents(thrall, nightmareComps);
                RemComp<ShadowlingShadowWalkComponent>(thrall);
                continue; // Don't polymorph the lesser again
            }

            var newUid = _polymorph.PolymorphEntity(thrall, "ShadowPolymorph");

            if (newUid == null)
                continue;

            EntityManager.AddComponents(newUid.Value, nightmareComps);
            RemComp<ThrallGuiseComponent>(newUid.Value);
            RemComp<NightVisionComponent>(newUid.Value);
        }

        var message = Loc.GetString("shadowling-ascended-message");
        var sender = Loc.GetString("shadowling-destroy-engines-sender");
        _chatSystem.DispatchStationAnnouncement(
            uid,
            message,
            sender,
            false,
            new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/ascension.ogg"),
            Color.Red);

        // Begin Global Sound
        _globalSound.DispatchStationEventMusic(uid, component.AscensionTheme, StationEventMusicType.ShadowLing, AudioParams.Default.WithLoop(true));
    }
}

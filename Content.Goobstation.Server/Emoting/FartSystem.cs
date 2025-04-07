// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 jellygato <aly.jellygato@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Emoting;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Bible.Components;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Body.Systems;
using Content.Shared.Camera;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class FartSystem : SharedFartSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _rng = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoilSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;


    private readonly string[] _fartSounds = [
        "/Audio/Effects/Emotes/parp1.ogg",
        "/Audio/_Goobstation/Voice/Human/fart2.ogg",
        "/Audio/_Goobstation/Voice/Human/fart3.ogg",
        "/Audio/_Goobstation/Voice/Human/fart4.ogg",
    ];
    private readonly string[] _fartInhaleSounds = [
        "/Audio/_Goobstation/Voice/Human/fart2-reverse.ogg",
        "/Audio/_Goobstation/Voice/Human/fart3-reverse.ogg",
        "/Audio/_Goobstation/Voice/Human/fart4-reverse.ogg",
        "/Audio/_Goobstation/Voice/Human/parp1-reverse.ogg",
    ];
    private readonly string[] _superFartSounds = [
        "/Audio/_Goobstation/Voice/Human/fart2-long.ogg",
        "/Audio/_Goobstation/Voice/Human/fart3-long.ogg",
        "/Audio/_Goobstation/Voice/Human/fart4-long.ogg",
        "/Audio/_Goobstation/Voice/Human/parp1-long.ogg",
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<FartComponent, PostFartEvent>(OnBibleFart);
    }

    private void OnEmote(EntityUid uid, FartComponent component, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        if (args.Emote.ID == "Fart")
        {
            args.Handled = true;

            if (component.SuperFarted)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                return;
            }

            // Make sure we aren't in timeout
            if (component.FartTimeout)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-out-of-farts"), uid, uid);
                return;
            }

            // Handle our bools
            component.FartTimeout = true;

            if (component.FartInhale)
            {
                component.FartInhale = false;
                _popup.PopupEntity(Loc.GetString("emote-fart-inhale-disarm-notice"), uid, uid);
            }

            // Shuffle the fart sounds
            _rng.Shuffle(_fartSounds);

            // Play a fart sound
            _audio.PlayEntity(_fartSounds[0], Filter.Pvs(uid), uid, true);

            // Release ammonia into the air
            var tileMix = _atmos.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(Gas.Ammonia, component.MolesAmmoniaPerFart);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
            var ev = new PostFartEvent(uid);
            RaiseLocalEvent(uid, ev);
        }
        else if (args.Emote.ID == "FartInhale")
        {
            args.Handled = true;

            if (component.SuperFarted)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                return;
            }

            if (component.FartInhale)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-already-loaded"), uid, uid);
            }

            component.FartInhale = true;

            // Shuffle the fart sounds
            _rng.Shuffle(_fartInhaleSounds);

            // Play a fart sound
            _audio.PlayEntity(_fartInhaleSounds[0], Filter.Pvs(uid), uid, true);

            _popup.PopupEntity(Loc.GetString("emote-fart-inhale-notice"), uid, uid);
        }
        else if (args.Emote.ID == "FartSuper")
        {
            args.Handled = true;

            if (component.SuperFarted)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                return;
            }

            if (!component.FartInhale)
            {
                _popup.PopupEntity(Loc.GetString("emote-fart-not-loaded"), uid, uid);
                return;
            }

            // Handle bools
            component.FartTimeout = true;
            component.FartInhale = false;
            component.SuperFarted = true;

            // Shuffle the fart sounds
            _rng.Shuffle(_superFartSounds);

            // Play a fart sound
            _audio.PlayEntity(_superFartSounds[0], Filter.Pvs(uid), uid, true, AudioParams.Default.WithVolume(0f));

            // Screen shake
            var xformSystem = _entMan.System<SharedTransformSystem>();
            CameraShake(10f, xformSystem.GetMapCoordinates(uid), 0.75f);

            // Release ammonia into the air
            var tileMix = _atmos.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(Gas.Ammonia, component.MolesAmmoniaPerFart * 2);

            _entMan.SpawnEntity("Butt", xformSystem.GetMapCoordinates(uid));

            _popup.PopupEntity(Loc.GetString("emote-fart-super-fart"), uid, uid);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
            var ev = new PostFartEvent(uid, true);
            RaiseLocalEvent(uid, ev);
        }
    }

    private void CameraShake(float range, MapCoordinates epicenter, float totalIntensity)
    {
        var players = Filter.Empty();
        players.AddInRange(epicenter, range, _playerManager, EntityManager);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid uid)
                continue;

            var playerPos = _transformSystem.GetWorldPosition(player.AttachedEntity!.Value);
            var delta = epicenter.Position - playerPos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(0.01f, 0);

            var distance = delta.Length();
            var effect = 5 * MathF.Pow(totalIntensity, 0.5f) * (1 - distance / range);
            if (effect > 0.01f)
                _recoilSystem.KickCamera(uid, -delta.Normalized() * effect);
        }
    }

    /// <summary>
    ///     Bible fart
    /// </summary>
    private void OnBibleFart(Entity<FartComponent> ent, ref PostFartEvent args)
    {
        foreach (var near in _lookup.GetEntitiesInRange(ent, 0.4f, LookupFlags.Sundries | LookupFlags.Dynamic)){

            if (!HasComp<BibleComponent>(near))
                continue;

            var ev = new BibleFartSmiteEvent(GetNetEntity(near));
            RaiseNetworkEvent(ev);
            _bodySystem.GibBody(ent, splatModifier: 15);
            _audio.PlayEntity(ent.Comp.BibleSmiteSnd, Filter.Pvs(near), near, true);
            if (!ent.Comp.SuperFarted)
            {
                _rng.Shuffle(_fartSounds);
                _audio.PlayEntity(_fartSounds[0], Filter.Pvs(near), near, true); // Must replay it because gib body makes the original fart sound stop immediately
            }
            else
            {
                _rng.Shuffle(_superFartSounds);
                _audio.PlayEntity(_superFartSounds[0], Filter.Pvs(near), near, true, AudioParams.Default.WithVolume(0f));
            }
            var xformSystem = _entMan.System<SharedTransformSystem>();
            CameraShake(10f, xformSystem.GetMapCoordinates(near), 1.5f);
            return;
        }
    }
}
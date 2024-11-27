using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.Emoting;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Server.Player;
using Content.Shared.Camera;
using System.Numerics;

namespace Content.Server._Goobstation.Emoting;

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
            tileMix?.AdjustMoles(Gas.Ammonia, FartComponent.MolesAmmoniaPerFart);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
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
            tileMix?.AdjustMoles(Gas.Ammonia, FartComponent.MolesAmmoniaPerFart * 2);

            _entMan.SpawnEntity("Butt", xformSystem.GetMapCoordinates(uid));

            _popup.PopupEntity(Loc.GetString("emote-fart-super-fart"), uid, uid);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
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
}

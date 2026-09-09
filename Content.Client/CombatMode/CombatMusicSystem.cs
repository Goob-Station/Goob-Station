// SPDX-License-Identifier: AGPL-3.0-or-later

// Lets this system fade normal ambient music out before combat music begins.
using Content.Client.Audio;
// Provides the ambient-music volume setting used by the rest of the client.
using Content.Shared.CCVar;
// Provides the server-to-client event raised when the local player takes damage.
using Content.Shared.CombatMode;
// Provides the round-restart cleanup event used to reset music between rounds.
using Content.Shared.GameTicking;
// Provides the client audio player used to start, stop, and change the music volume.
using Robust.Client.Audio;
// Provides the event raised when the local player is detached from their current body.
using Robust.Client.Player;
// Provides sound paths and playback settings such as looping and volume.
using Robust.Shared.Audio;
// Provides the shared helper that converts the player's volume slider into decibels.
using Robust.Shared.Audio.Systems;
// Provides access to client configuration values.
using Robust.Shared.Configuration;
// Provides Filter.Local(), which makes the music play only for this player.
using Robust.Shared.Player;
// Provides the engine clock used for the three-second start delay.
using Robust.Shared.Timing;

namespace Content.Client.CombatMode;

/// <summary>
/// Starts local combat music after the player remains in combat mode for a short time,
/// then fades that music out when combat mode is disabled.
/// </summary>
public sealed class CombatMusicSystem : EntitySystem
{
    // Gives this system the current local player's combat-mode state and change event.
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    // Reuses the game's existing music fade helper instead of changing volume manually each frame.
    [Dependency] private readonly ContentAudioSystem _contentAudio = default!;
    // Starts and stops the global music stream for the local player.
    [Dependency] private readonly AudioSystem _audio = default!;
    // Reads the player's ambient-music volume preference.
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    // Reads simulation time so the delayed start remains consistent across frame rates.
    [Dependency] private readonly IGameTiming _timing = default!;

    // Waits this long before starting music so quick combat-mode taps stay silent.
    private static readonly TimeSpan CombatMusicDelay = TimeSpan.FromSeconds(3);
    // Keeps damage-triggered music active this long after the player's most recent damage.
    private static readonly TimeSpan DamageMusicDuration = TimeSpan.FromSeconds(10);
    // Takes this long to lower the music to silence after combat mode is disabled.
    private const float CombatMusicFadeOutTime = 2f;
    // Lowers the track itself before applying the player's music-volume preference.
    private const float CombatMusicBaseVolume = -8f;
    // Matches the fade helper's silent floor so a zero volume setting still fades safely.
    private const float SilentVolume = -32f;
    // Names the YAML sound collection that contains all available combat tracks.
    private const string CombatMusicCollection = "CombatMusic";
    // Lets the audio system randomly resolve one track from the configurable collection per playback.
    private static readonly SoundSpecifier CombatMusic = new SoundCollectionSpecifier(CombatMusicCollection);

    // Stores when the delayed music should start; null means that no start is pending.
    private TimeSpan? _combatMusicStartTime;
    // Stores when damage-triggered music may fade if the player does not enter combat mode.
    private TimeSpan? _damageMusicEndTime;
    // Stores the playing audio entity so it can be faded, stopped, or have its volume changed.
    private EntityUid? _combatMusicStream;
    // Retains a fading stream so a new damage trigger can stop it before starting another track.
    private EntityUid? _fadingCombatMusicStream;
    // Stores the ambient-music slider converted to the decibel value expected by the audio system.
    private float _musicVolume;

    /// <summary>
    /// Connects combat-mode and volume changes to the music controller.
    /// </summary>
    public override void Initialize()
    {
        // Runs the normal EntitySystem setup first.
        base.Initialize();

        // Allows the three-second timer to advance even outside predicted simulation updates.
        UpdatesOutsidePrediction = true;
        // Receives changes only for the local player's combat mode.
        _combatMode.LocalPlayerCombatModeUpdated += OnCombatModeUpdated;
        // Keeps combat music on the same volume slider as other in-round music.
        Subs.CVar(_configuration, CCVars.AmbientMusicVolume, OnMusicVolumeChanged, true);
        // Prevents ambient music from starting over the combat track while it is playing.
        SubscribeLocalEvent<PlayAmbientMusicEvent>(OnPlayAmbientMusic);
        // Clears combat music when the player leaves their current body.
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        // Clears stale timers and stream references before the next round begins.
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        // Starts or extends combat music when the server confirms that this player took damage.
        SubscribeNetworkEvent<CombatMusicDamageEvent>(OnCombatMusicDamage);
    }

    /// <summary>
    /// Advances the delayed start timer and starts music once the delay has elapsed.
    /// </summary>
    public override void Update(float frameTime)
    {
        // Lets the base EntitySystem perform its normal per-frame work.
        base.Update(frameTime);

        // Expires damage-triggered music after ten seconds without damage while combat mode is off.
        if (_damageMusicEndTime != null && _timing.CurTime >= _damageMusicEndTime.Value)
        {
            // Clears the expiry first so the fade happens only once.
            _damageMusicEndTime = null;

            // Leaves music running when combat mode itself still requires it.
            if (!_combatMode.IsInCombatMode())
                StopCombatMusic(true);
        }

        // Stops here when there is no delayed combat-mode start, or when its delay has not elapsed yet.
        if (_combatMusicStartTime == null || _timing.CurTime < _combatMusicStartTime.Value)
            return;

        // Clears the timer before attempting playback so this start is handled only once.
        _combatMusicStartTime = null;

        // Rechecks combat mode in case it changed without delivering another local update.
        if (!_combatMode.IsInCombatMode())
            return;

        // Starts one randomly selected track from the combat-music collection.
        StartCombatMusic();
    }

    /// <summary>
    /// Removes event subscriptions and immediately stops any remaining music during client shutdown.
    /// </summary>
    public override void Shutdown()
    {
        // Avoids retaining this system through the combat-mode event after it shuts down.
        _combatMode.LocalPlayerCombatModeUpdated -= OnCombatModeUpdated;
        // Stops immediately because a fade cannot continue after the system has shut down.
        StopCombatMusic(false);

        // Runs the normal EntitySystem shutdown last.
        base.Shutdown();
    }

    /// <summary>
    /// Schedules playback when combat begins, or cancels/fades playback when combat ends.
    /// </summary>
    private void OnCombatModeUpdated(bool enabled)
    {
        // Combat mode was disabled, so cancel a pending start and fade any active track out.
        if (!enabled)
        {
            // Ends any recent-damage window because the player explicitly left combat mode.
            _damageMusicEndTime = null;
            // Cancels a pending start and fades any playing stream over two seconds.
            StopCombatMusic(true);
            // No combat-on scheduling is needed for this update.
            return;
        }

        // Ignores duplicate combat-on updates when music is already playing or already scheduled.
        if (_combatMusicStream != null || _combatMusicStartTime != null)
            return;

        // Schedules music for three seconds from now instead of playing on a quick toggle.
        _combatMusicStartTime = _timing.CurTime + CombatMusicDelay;
    }

    /// <summary>
    /// Starts music immediately, or extends its lifetime, when the server reports player damage.
    /// </summary>
    private void OnCombatMusicDamage(CombatMusicDamageEvent args)
    {
        // Pushes automatic fade-out ten seconds beyond the most recent hit.
        _damageMusicEndTime = _timing.CurTime + DamageMusicDuration;
        // Damage should start music immediately instead of waiting on combat mode's delay.
        _combatMusicStartTime = null;

        // Keeps the current track playing while only refreshing its recent-damage lifetime.
        if (_combatMusicStream != null)
            return;

        // Selects and starts a fresh track when no combat music is currently active.
        StartCombatMusic();
    }

    /// <summary>
    /// Starts one random looping track from the combat-music sound collection.
    /// </summary>
    private void StartCombatMusic()
    {
        // Stops any older stream that is still finishing its fade to prevent two combat tracks overlapping.
        _fadingCombatMusicStream = _audio.Stop(_fadingCombatMusicStream);
        // Fades ordinary ambient music away before the combat track begins.
        _contentAudio.DisableAmbientMusic();

        // Resolves a random collection entry locally, loops it, and applies the player's music volume.
        var playback = _audio.PlayGlobal(
            CombatMusic,
            Filter.Local(),
            false,
            AudioParams.Default.WithLoop(true).WithVolume(GetCombatMusicVolume()));

        // Saves the stream entity when playback succeeds; null safely represents a failed start.
        _combatMusicStream = playback?.Entity;
    }

    /// <summary>
    /// Stops combat music when the local player no longer controls their current body.
    /// </summary>
    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent args)
    {
        // Stops immediately because detached players should not carry body-specific combat music onward.
        StopCombatMusic(false);
    }

    /// <summary>
    /// Resets combat music state when the client cleans up the finished round.
    /// </summary>
    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        // Stops immediately and clears references so combat music can start normally next round.
        StopCombatMusic(false);
    }

    /// <summary>
    /// Cancels delayed playback and either fades or immediately stops the active stream.
    /// </summary>
    private void StopCombatMusic(bool fadeOut)
    {
        // Ensures music cannot begin after combat, detachment, restart, or shutdown has ended it.
        _combatMusicStartTime = null;
        // Clears automatic damage expiry during explicit or lifecycle-driven cleanup.
        _damageMusicEndTime = null;

        // Uses the shared fade helper only for the normal combat-mode-off transition.
        if (fadeOut)
        {
            // Starts a fade only when an active stream exists; repeated stop events leave a fade in progress.
            if (_combatMusicStream != null)
            {
                _contentAudio.FadeOut(_combatMusicStream, duration: CombatMusicFadeOutTime);
                // Keeps the fading entity available so a rapid damage restart can stop it cleanly.
                _fadingCombatMusicStream = _combatMusicStream;
            }
        }
        // Stops directly for lifecycle cleanup, when there may be no future frames to finish a fade.
        else
        {
            _audio.Stop(_combatMusicStream);
            // Also stops a stream that may already have been fading when cleanup began.
            _fadingCombatMusicStream = _audio.Stop(_fadingCombatMusicStream);
        }

        // Releases our stream reference so a later combat session can start a fresh track.
        _combatMusicStream = null;
    }

    /// <summary>
    /// Applies live volume-setting changes to the currently playing combat track.
    /// </summary>
    private void OnMusicVolumeChanged(float gain)
    {
        // Converts the zero-to-one settings value into the decibel scale used by audio components.
        _musicVolume = SharedAudioSystem.GainToVolume(gain);

        // Applies the new value immediately when a combat track is already playing.
        if (_combatMusicStream != null)
            _audio.SetVolume(_combatMusicStream, GetCombatMusicVolume());
    }

    /// <summary>
    /// Combines the track and settings volumes without going below the fade helper's silent floor.
    /// </summary>
    private float GetCombatMusicVolume()
    {
        // Clamps muted audio to a finite value so fade-out math cannot receive negative infinity.
        return MathF.Max(CombatMusicBaseVolume + _musicVolume, SilentVolume);
    }

    /// <summary>
    /// Stops the normal ambience selector from layering another music track over combat music.
    /// </summary>
    private void OnPlayAmbientMusic(ref PlayAmbientMusicEvent args)
    {
        // Cancels only while a combat track exists, allowing ambience to resume after its fade begins.
        if (_combatMusicStream != null)
            args.Cancelled = true;
    }
}

using Content.Shared._Pirate.Cassette;
using Content.Shared._Pirate.CCVars;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Pirate.Cassette;

public sealed partial class CassetteSystem : SharedCassetteSystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private IAudioManager _audioManager = default!;
    [Dependency] private IComponentFactory _compFactory = default!;
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private IFileDialogManager _dialogs = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IResourceCache _resourceCache = default!;
    [Dependency] private IGameTiming _timing = default!;

    private float _gain;
    private readonly Dictionary<AudioStream, string> _names = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        Subs.CVar(_config, PirateVars.VolumeGainCassettes, SetGain, true);

        try
        {
            foreach (var entity in _prototype.EnumeratePrototypes<EntityPrototype>())
            {
                if (!entity.TryGetComponent<CassetteTapeComponent>(out var tape, _compFactory))
                    continue;

                foreach (var sound in tape.Songs)
                {
                    if (_audio.GetAudioPath(_audio.ResolveSound(sound)) is { } path)
                        _resourceCache.TryGetResource(new ResPath(path), out AudioResource? _);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error preloading cassette songs:\n{e}");
        }
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _names.Clear();
    }

    private void SetGain(float gain)
    {
        _gain = gain;
        if (_player.LocalEntity is not { } ent)
            return;

        var slots = _inventory.GetSlotEnumerator(ent);
        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } contained ||
                !TryComp(contained, out CassettePlayerComponent? player))
            {
                continue;
            }

            SetAudioGain(player.AudioStream);
            SetAudioGain(player.CustomAudioStream);
        }
    }

    protected override EntityUid? PlayCustomTrack(Entity<CassettePlayerComponent> player, Entity<CassetteTapeComponent> tape)
    {
        base.PlayCustomTrack(player, tape);
        if (tape.Comp.CustomTrack is not AudioStream stream)
            return null;

        if (!_timing.IsFirstTimePredicted)
            return null;

        if (!_names.TryGetValue(stream, out var name))
            return null;

        var audioParams = player.Comp.AudioParams.WithVolume(SharedAudioSystem.GainToVolume(_gain));
        return _audio.PlayGlobal(stream, new ResolvedPathSpecifier(name), audioParams)?.Entity;
    }

    protected override async void ChooseCustomTrack(Entity<CassetteTapeComponent> tape)
    {
        try
        {
            if (!_timing.IsFirstTimePredicted)
                return;

            var filters = new FileDialogFilters(new FileDialogFilters.Group("ogg"));
            await using var file = await _dialogs.OpenFile(filters);
            if (file == null)
                return;

            var audio = _audioManager.LoadAudioOggVorbis(file);
            tape.Comp.CustomTrack = audio;

            var name = $"/Audio/_Pirate/Cassette/_CustomUploads/upload_{_names.Count}.ogg";
            _resourceCache.CacheResource(name, new AudioResource(audio));
            _names[audio] = name;
        }
        catch (Exception e)
        {
            Log.Error($"Error choosing custom cassette track:\n{e}");
        }
    }

    private void SetAudioGain(EntityUid? audio)
    {
        if (!TryComp(audio, out AudioComponent? audioComp))
            return;

#pragma warning disable RA0002
        audioComp.Params = audioComp.Params with { Volume = SharedAudioSystem.GainToVolume(_gain) };
#pragma warning restore RA0002
    }
}

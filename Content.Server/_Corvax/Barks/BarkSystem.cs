using Content.Shared.Corvax.CorvaxVars;
using Content.Server.Chat.Systems;
using Content.Shared._Corvax.Speech.Synthesis;
using Content.Shared._Corvax.Speech.Synthesis.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._Corvax.Speech.Synthesis.System;

/// <summary>
/// Handles barks for entities.
/// </summary>
public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpeechSynthesisComponent, EntitySpokeEvent>(OnEntitySpoke);

        SubscribeNetworkEvent<RequestPreviewBarkEvent>(OnRequestPreviewBark);
    }

    private void OnEntitySpoke(EntityUid uid, SpeechSynthesisComponent comp, EntitySpokeEvent args)
    {
        if (comp.VoicePrototypeId is null ||
            !_prototypeManager.TryIndex<BarkPrototype>(comp.VoicePrototypeId, out var barkProto) ||
            !_configurationManager.GetCVar(CorvaxVars.BarksEnabled))
            return;

        bool isObfuscated = args.ObfuscatedMessage != null;
        var sourceEntity = _entityManager.GetNetEntity(uid);
        var soundPath = barkProto.SoundFiles[new Random().Next(barkProto.SoundFiles.Count)];
        RaiseNetworkEvent(new PlayBarkEvent(soundPath, sourceEntity, args.Message, comp.PlaybackSpeed, isObfuscated));
    }

    private async void OnRequestPreviewBark(RequestPreviewBarkEvent ev, EntitySessionEventArgs args)
    {
        if (string.IsNullOrEmpty(ev.BarkVoiceId) || !_prototypeManager.TryIndex<BarkPrototype>(ev.BarkVoiceId, out var barkProto)
            || !_configurationManager.GetCVar(CorvaxVars.BarksEnabled))
            return;

        var soundPath = barkProto.SoundFiles[new Random().Next(barkProto.SoundFiles.Count)];
        var soundSpecifier = new SoundPathSpecifier(soundPath);

        var audioParams = new AudioParams
        {
            Pitch = 1.0f,
            Volume = 4f,
            Variation = 0.125f
        };

        _audio.PlayGlobal(soundSpecifier, args.SenderSession, audioParams);
    }
}

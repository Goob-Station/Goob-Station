// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.MisandryBox;

// Now that's a mouthful
public sealed class CatEmoteSpamCountermeasureSystem : EntitySystem
{
    [Dependency] private readonly ThunderstrikeSystem _thunderstrike = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    private const float ClearInterval = 20.0f;
    private const float PitchModulo = 0.08f;
    private const int LowerBound = 2; // Shoo away any shits with server vv from killing everyone on 1 emote

    [ViewVariables(VVAccess.ReadWrite)]
    private int _hardEmoteThreshold = 20;

    [ViewVariables(VVAccess.ReadWrite)]
    private int _softThresholdVariance = 10;

    [ViewVariables(VVAccess.ReadWrite)]
    private float _postSoftThresholdProbability = 0.08f;

    [ViewVariables(VVAccess.ReadWrite)]
    private float _softThresholdRefreshCooldown = 34f;

    [ViewVariables(VVAccess.ReadOnly)]
    // ReSharper disable once UnusedMember.Local
    private int SoftThreshold => GetSoftThreshold();

    private int? _softThreshold;

    /// <summary>
    /// Ash offenders on proc? Tell them what they should do?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool DrasticMeasures = true;

    [ViewVariables(VVAccess.ReadOnly)]
    private Dictionary<EntityUid, int> _meowTracker = [];
    private float _timeSinceLastClear = 0f;

    private float _timeSinceLastRefresh = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechComponent, EmoteEvent>(OnEmoteEvent);
        SubscribeLocalEvent<SpeechComponent, EmoteSoundPitchShiftEvent>(OnGetPitchShiftEvent);
    }

    private void OnGetPitchShiftEvent(Entity<SpeechComponent> ent, ref EmoteSoundPitchShiftEvent ev)
    {
        var shift = GetCount(ent.Owner);
        ev.Pitch = shift * PitchModulo;
    }

    private int GetCount(EntityUid entity)
    {
        return _meowTracker.TryGetValue(entity, out var count) ? count : 0;
    }

    public override void Update(float frameTime)
    {
        _timeSinceLastClear += frameTime;
        _timeSinceLastRefresh += frameTime;

        if (_timeSinceLastClear >= ClearInterval)
        {
            _meowTracker.Clear();
            _timeSinceLastClear = 0f;
        }

        if (_timeSinceLastRefresh >= _softThresholdRefreshCooldown)
        {
            GetSoftThreshold(true);
            _timeSinceLastRefresh = 0f;
        }
    }

    private void OnEmoteEvent(Entity<SpeechComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.Category is EmoteCategory.Vocal or EmoteCategory.Farts && args.Voluntary)
            Add(ent.Owner);
    }

    private void Add(EntityUid uid)
    {
        if (!_meowTracker.TryGetValue(uid, out var count))
            count = 0;

        count++;
        _meowTracker[uid] = count;

        TryHardThresholdSmite(uid, count);

        TrySoftThresholdSmite(uid, count);
    }

    private void TryHardThresholdSmite(EntityUid uid, int count)
    {
        if (count >= _hardEmoteThreshold)
            Smite(uid);
    }

    private void TrySoftThresholdSmite(EntityUid uid, int count)
    {
        // This here has a very funny emergent possibility of getting changed FOR THE BEST mid-emote and smiting people
        var soft = GetSoftThreshold();

        if (count < soft)
            return;

        // This is ground control to major tom
        var steps = count - soft;
        // By default, this is 8% per step over. 10 over soft threshold is 80%.
        var chance = steps*_postSoftThresholdProbability;

        if (_rand.Prob(chance))
            Smite(uid, false);
    }

    private int GetSoftThreshold(bool refresh = false)
    {
        if (_softThreshold == null || refresh)
            _softThreshold = Math.Max(LowerBound, _hardEmoteThreshold * 3 / 4 - _rand.Next(0, _softThresholdVariance));

        return _softThreshold.Value;
    }

    /// <summary>
    /// Thunderstrike a mumu
    /// </summary>
    /// <param name="uid">Target entity</param>
    /// <param name="killOverride">Optional override for the kill parameter. If null, uses DrasticMeasures</param>
    private void Smite(EntityUid uid, bool? killOverride = null)
    {
        _thunderstrike.Smite(uid, kill: killOverride ?? DrasticMeasures);
    }
}

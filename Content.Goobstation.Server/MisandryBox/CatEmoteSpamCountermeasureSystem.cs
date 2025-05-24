// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;

namespace Content.Goobstation.Server.MisandryBox;

// Now that's a mouthful
// "ZE KOUNTERMEASURES!" t. turbotracker
public sealed class CatEmoteSpamCountermeasureSystem : EntitySystem
{
    [Dependency] private readonly ThunderstrikeSystem _thunderstrike = default!;

    private const float ClearInterval = 20.0f;
    private const float PitchModulo = 0.08f;

    [ViewVariables(VVAccess.ReadWrite)]
    private int _maxEmotes = 20;

    /// <summary>
    /// Ash offenders on proc? Tell them what they should do?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool DrasticMeasures = true;

    private Dictionary<EntityUid, int> _meowTracker = [];
    private float _timeSinceLastClear = 0f;

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

        if (!(_timeSinceLastClear >= ClearInterval))
            return;

        _meowTracker.Clear();
        _timeSinceLastClear = 0f;
    }

    private void OnEmoteEvent(Entity<SpeechComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.Category is EmoteCategory.Vocal or EmoteCategory.Farts)
            Add(ent.Owner);
    }

    private void Add(EntityUid uid)
    {
        if (!_meowTracker.TryGetValue(uid, out var count))
        {
            _meowTracker[uid] = 1;
            return;
        }

        _meowTracker[uid] = count + 1;

        if (_meowTracker[uid] >= _maxEmotes)
            Smite(uid);
    }

    private void Smite(EntityUid uid)
    {
        _thunderstrike.Smite(uid, kill: DrasticMeasures);
    }
}

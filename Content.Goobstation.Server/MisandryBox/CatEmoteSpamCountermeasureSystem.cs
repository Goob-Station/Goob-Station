// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server.Chat.Systems;
using Content.Shared.Speech;

namespace Content.Goobstation.Server.MisandryBox;

// Now that's a mouthful
public sealed class CatEmoteSpamCountermeasureSystem : EntitySystem
{
    [Dependency] private readonly ThunderstrikeSystem _thunderstrike = default!;

    private readonly string[] _emoteTags = new string[] { "FelinidEmotes", "VulpEmotes" };
    private const float ClearInterval = 15.0f;

    [ViewVariables(VVAccess.ReadWrite)]
    private int _maxEmotes = 5;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool DrasticMeasures = false;

    private Dictionary<EntityUid, int> _meowTracker = [];
    private float _timeSinceLastClear = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechComponent, EmoteEvent>(OnEmoteEvent);
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
        if (args.Emote.Whitelist?.Tags is null)
            return;

        foreach (var tag in args.Emote.Whitelist.Tags
                     .Where(tag => _emoteTags.Contains(tag.Id)))
        {
            Add(ent.Owner);
        }
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

// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server.Chat.Systems;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MisandryBox;

// Now that's a mouthful
public sealed class CatEmoteSpamCountermeasureSystem : EntitySystem
{
    [Dependency] private readonly ThunderstrikeSystem _thunderstrike = default!;

    private const string CatEmoteTag = "FelinidEmotes";

    [ViewVariables(VVAccess.ReadWrite)]
    private int _maxEmotes = 5;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool DrasticMeasures = false;

    private Dictionary<EntityUid, int> _meowTracker = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechComponent, EmoteEvent>(OnEmoteEvent);

        Robust.Shared.Timing.Timer.SpawnRepeating(TimeSpan.FromSeconds(15), ClearTable, CancellationToken.None);
    }

    public void ClearTable()
    {
        _meowTracker.Clear();
    }

    private void OnEmoteEvent(Entity<SpeechComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.Whitelist?.Tags is null)
            return;

        foreach (var tag in args.Emote.Whitelist.Tags
                     .Where(tag => tag == CatEmoteTag))
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

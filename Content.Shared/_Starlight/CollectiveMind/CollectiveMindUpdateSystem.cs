// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind;

public sealed class CollectiveMindUpdateSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly Dictionary<string, int> _nextWebId = new(); // Goobstation
    private readonly Dictionary<(string PrototypeId, int WebId), int> _nextMemberId = new(); // Goobstation

    // Goobstation - Start

    public void UpdateCollectiveMind(EntityUid uid, CollectiveMindComponent collective)
    {
        foreach (var prototype in _proto.EnumeratePrototypes<CollectiveMindPrototype>())
        {
            var hasChannel = collective.Channels.Contains(prototype.ID);
            var hasMembership = collective.WebMemberships.ContainsKey(prototype.ID);

            switch (hasChannel)
            {
                // Create new web if no web is specified.
                case true when !hasMembership:
                    CreateOrJoinWeb(uid, prototype.ID);
                    break;
                // Remove from web if channel is lost.
                case false when hasMembership:
                    collective.WebMemberships.Remove(prototype.ID);
                    break;
            }
        }
    }
    public void CreateOrJoinWeb(
        EntityUid uid,
        string prototypeId,
        int? specificWebId = null)
    {
        var collective = EnsureComp<CollectiveMindComponent>(uid);

        // If joining a new one, leave the previous web.
        collective.WebMemberships.Remove(prototypeId);

        var webId = specificWebId ?? GetNextWebId(prototypeId);
        var memberKey = (prototypeId, webId);

        var memberId = _nextMemberId.GetValueOrDefault(memberKey, 0);
        memberId++;
        _nextMemberId[memberKey] = memberId;

        collective.WebMemberships[prototypeId] = new CollectiveMindMembership
        {
            WebId = webId,
            MemberId = memberId,
        };

    }

    private int GetNextWebId(string prototypeId)
    {
        _nextWebId.TryAdd(prototypeId, 0);
        return ++_nextWebId[prototypeId];
    }
    // Goobstation - End
}

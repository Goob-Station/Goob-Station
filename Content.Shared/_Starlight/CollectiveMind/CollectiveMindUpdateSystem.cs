// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Shared._Starlight.CollectiveMind;

public sealed class CollectiveMindUpdateSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static Dictionary<string, int> _currentId = new();

    public void UpdateCollectiveMind(EntityUid uid, CollectiveMindComponent collective)
    {
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<CollectiveMindPrototype>())
        {
            if (!_currentId.ContainsKey(prototype.ID))
                _currentId[prototype.ID] = 0;

            bool hasChannel = collective.Channels.Contains(prototype.ID);
            bool alreadyAdded = collective.Minds.ContainsKey(prototype.ID);
            if (hasChannel && !alreadyAdded)
                collective.Minds.Add(prototype.ID, ++_currentId[prototype.ID]);
            else if (!hasChannel && alreadyAdded)
                collective.Minds.Remove(prototype.ID);
        }
    }

    public bool HasCollectiveMind(Entity<CollectiveMindComponent?> ent, ProtoId<CollectiveMindPrototype> channel) {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.Channels.Contains(channel);
    }

    public bool AddCollectiveMind(Entity<CollectiveMindComponent?> ent, ProtoId<CollectiveMindPrototype> channel, bool setDefault = false) {
        if (!Resolve(ent, ref ent.Comp, false))
            EnsureComp<CollectiveMindComponent>(ent, out ent.Comp);

        bool res = ent.Comp.Channels.Add(channel);
        if (setDefault)
            ent.Comp.DefaultChannel = channel;

        Dirty(ent);
        return res;
    }

    public bool RemoveCollectiveMind(Entity<CollectiveMindComponent?> ent, ProtoId<CollectiveMindPrototype> channel) {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        bool res = ent.Comp.Channels.Remove(channel);
        if (ent.Comp.DefaultChannel == channel)
            ent.Comp.DefaultChannel = null;

        Dirty(ent);
        return res;
    }
}

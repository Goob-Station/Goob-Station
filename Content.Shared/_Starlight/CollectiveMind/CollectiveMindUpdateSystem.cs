// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
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

            foreach (var component in prototype.RequiredComponents)
                if (EntityManager.HasComponent(uid, _componentFactory.GetRegistration(component).Type) && !collective.Minds.ContainsKey(prototype.ID))
                    collective.Minds.Add(prototype.ID, ++_currentId[prototype.ID]);
                else if (!EntityManager.HasComponent(uid, _componentFactory.GetRegistration(component).Type) && collective.Minds.ContainsKey(prototype.ID))
                    collective.Minds.Remove(prototype.ID);
            foreach (var tag in prototype.RequiredTags)
                if (_tag.HasTag(uid, tag) && !collective.Minds.ContainsKey(prototype.ID))
                    collective.Minds.Add(prototype.ID, ++_currentId[prototype.ID]);
                else if (!_tag.HasTag(uid, tag) && collective.Minds.ContainsKey(prototype.ID))
                    collective.Minds.Remove(prototype.ID);
        }
    }
}
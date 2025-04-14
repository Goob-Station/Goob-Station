// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Chemistry.Reaction;

[RegisterComponent]
public sealed partial class ReactiveComponent : Component
{
    /// <summary>
    ///     A dictionary of reactive groups -> methods that work on them.
    /// </summary>
    [DataField("groups", readOnly: true, serverOnly: true,
        customTypeSerializer:
        typeof(PrototypeIdDictionarySerializer<HashSet<ReactionMethod>, ReactiveGroupPrototype>))]
    public Dictionary<string, HashSet<ReactionMethod>>? ReactiveGroups;

    /// <summary>
    ///     Special reactions that this prototype can specify, outside of any that reagents already apply.
    ///     Useful for things like monkey cubes, which have a really prototype-specific effect.
    /// </summary>
    [DataField("reactions", true, serverOnly: true)]
    public List<ReactiveReagentEffectEntry>? Reactions;
}

[DataDefinition]
public sealed partial class ReactiveReagentEffectEntry
{
    [DataField("methods")]
    public HashSet<ReactionMethod> Methods = default!;

    [DataField("reagents", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<ReagentPrototype>))]
    public HashSet<string>? Reagents = null;

    [DataField("effects", required: true)]
    public List<EntityEffect> Effects = default!;

    [DataField("groups", readOnly: true, serverOnly: true,
        customTypeSerializer:typeof(PrototypeIdDictionarySerializer<HashSet<ReactionMethod>, ReactiveGroupPrototype>))]
    public Dictionary<string, HashSet<ReactionMethod>>? ReactiveGroups { get; private set; }
}
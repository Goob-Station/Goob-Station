// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class Polymorph : EventEntityEffect<Polymorph>
{
    /// <summary>
    ///     What polymorph prototype is used on effect
    /// </summary>
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<PolymorphPrototype>))]
    public string PolymorphPrototype { get; set; }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) // Goob edit
    {
        var entProto = prototype.Index<PolymorphPrototype>(PolymorphPrototype).Configuration.Entity;
        if (entProto == null)
            return null;
        return Loc.GetString("reagent-effect-guidebook-make-polymorph", ("chance", Probability), ("entityname", prototype.Index<EntityPrototype>(entProto.Value).Name));
    }
}

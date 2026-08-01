// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic.Prototypes;

[ImplicitDataDefinitionForInheritors]
public abstract partial class RitualCustomBehavior
{
    /// <param name="outstr">Output string in case something is missing</param>
    /// <returns>If the behavior was successful or not</returns>
    public abstract bool Execute(RitualData args, out string? outstr);

    /// <summary>
    ///     If the ritual is successful do *this*.
    /// </summary>
    /// <param name="args"></param>
    public abstract void Finalize(RitualData args);
}

public readonly record struct RitualData(
    EntityUid Performer,
    EntityUid Platform,
    Entity<HereticComponent> Mind,
    ProtoId<HereticRitualPrototype> RitualId,
    IEntityManager EntityManager,
    List<EntityUid>? Limited,
    int Limit);

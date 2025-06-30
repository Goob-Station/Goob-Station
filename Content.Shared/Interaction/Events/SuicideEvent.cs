// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Interaction.Events;

/// <summary>
///     Raised Directed at an entity to check whether they will handle the suicide.
/// </summary>
public sealed class SuicideEvent : HandledEntityEventArgs
{
    public SuicideEvent(EntityUid victim)
    {
        Victim = victim;
    }

    public DamageSpecifier? DamageSpecifier;
    public ProtoId<DamageTypePrototype>? DamageType;
    public EntityUid Victim { get; private set; }
}

public sealed class SuicideByEnvironmentEvent : HandledEntityEventArgs
{
    public SuicideByEnvironmentEvent(EntityUid victim)
    {
        Victim = victim;
    }

    public EntityUid Victim { get; set; }
}

public sealed class SuicideGhostEvent : HandledEntityEventArgs
{
    public SuicideGhostEvent(EntityUid victim)
    {
        Victim = victim;
    }

    public EntityUid Victim { get; set; }
    public bool CanReturnToBody;
}
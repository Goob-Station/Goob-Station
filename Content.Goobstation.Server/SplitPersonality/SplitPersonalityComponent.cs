// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SplitPersonality;

[RegisterComponent]
public sealed partial class SplitPersonalityComponent : Component
{
    /// <summary>
    /// Should the entity do startup flavor when receiving the component?
    /// This means the whole popup and falling to the ground.
    /// </summary>
    [DataField]
    public bool DoStartFlavor = true;

    /// <summary>
    /// The original, or main, mind of this entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OriginalMind;

    /// <summary>
    /// A list of every dummy entity containing a split personality attached to the host.
    /// Stored so they can be safely deleted.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid?> GhostRoleDummies = [];

    /// <summary>
    /// The container on the main entity that contains the dummy entities.
    /// We can't just store them in null-space, or at least this is easier :godo:
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container MindsContainer;

    /// <summary>
    /// The mind roles of this entity.
    /// Copied to the split personalities on creation.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> MindRoles = [];

    /// <summary>
    /// The objectives of this entity.
    /// Copied to the split personalities on creation.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Objectives = [];

    /// <summary>
    /// When the next swap attempt will take place.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextSwapAttempt = TimeSpan.Zero;

    /// <summary>
    /// How many additional minds to add to this entity.
    /// </summary>
    [DataField]
    public int AdditionalMindsCount = 1;

    /// <summary>
    /// Every SwapAttemptDelay amount of seconds, it has a percent chance to successfully swap.
    /// 1 being 100%, 0 being 0%
    /// </summary>
    [DataField]
    public float SwapProbability = 0.30f; // probably a good amount? idk tweak this if yo uwant

    /// <summary>
    /// The amount of seconds between every swap attempt
    /// </summary>
    [DataField]
    public TimeSpan SwapAttemptDelay = TimeSpan.FromSeconds(25);

    [DataField]
    public ProtoId<CollectiveMindPrototype> CollectiveMind = "SplitPersonality";
}

[RegisterComponent]
public sealed partial class SplitPersonalityDummyComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Host;
}

// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Bakke <luringens@protonmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization; // Goobstation
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared.Polymorph;

/// <summary>
/// Polymorphs generally describe any type of transformation that can be applied to an entity.
/// </summary>
[Prototype]
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class PolymorphPrototype : IPrototype, IInheritingPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<PolymorphPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [DataField(required: true)]
    public PolymorphConfiguration Configuration = new();

}

/// <summary>
/// Defines information about the polymorph
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial record PolymorphConfiguration
{
    /// <summary>
    /// What entity the polymorph will turn the target into
    /// must be in here because it makes no sense if it isn't
    /// </summary>
    [DataField]
    public EntProtoId? Entity;

    /// <summary>
    /// Additional entity to spawn when polymorphing/reverting.
    /// Gets parented to the entity polymorphed into.
    /// Useful for visual effects.
    /// </summary>
    [DataField(serverOnly: true)]
    public EntProtoId? EffectProto;

    /// <summary>
    /// The delay between the polymorph's uses in seconds
    /// Slightly weird as of right now.
    /// </summary>
    [DataField(serverOnly: true)]
    public int Delay = 60;

    /// <summary>
    /// The duration of the transformation in seconds
    /// can be null if there is not one
    /// </summary>
    [DataField(serverOnly: true)]
    public int? Duration;

    /// <summary>
    /// whether or not the target can transform as will
    /// set to true for things like polymorph spells and curses
    /// </summary>
    [DataField(serverOnly: true)]
    public bool Forced;

    /// <summary>
    /// Whether or not the entity transfers its damage between forms.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool TransferDamage = true;

    /// <summary>
    /// Whether or not the entity transfers its name between forms.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool TransferName;

    /// <summary>
    /// Whether or not the entity transfers its hair, skin color, hair color, etc.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool TransferHumanoidAppearance;

    /// <summary>
    /// Whether or not the entity transfers its inventory and equipment between forms.
    /// </summary>
    [DataField(serverOnly: true)]
    public PolymorphInventoryChange Inventory = PolymorphInventoryChange.None;

    /// <summary>
    /// Whether or not the polymorph reverts when the entity goes into crit.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool RevertOnCrit = true;

    /// <summary>
    /// Whether or not the polymorph reverts when the entity dies.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool RevertOnDeath = true;

    /// <summary>
    /// Whether or not the polymorph reverts when the entity is deleted.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool RevertOnDelete = true;

    /// <summary>
    /// Whether or not the polymorph reverts when the entity is eaten or fully sliced.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool RevertOnEat;

    /// <summary>
    /// If true, attempts to polymorph this polymorph will fail, unless
    /// <see cref="IgnoreAllowRepeatedMorphs"/> is true on the /new/ morph.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool AllowRepeatedMorphs;

    /// <summary>
    /// If true, this morph will succeed even when used on an entity
    /// that is already polymorphed with a configuration that has
    /// <see cref="AllowRepeatedMorphs"/> set to false. Helpful for
    /// smite polymorphs which should always succeed.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool IgnoreAllowRepeatedMorphs;

    /// <summary>
    /// The amount of time that should pass after this polymorph has ended, before a new one
    /// can occur.
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Cooldown = TimeSpan.Zero;

    /// <summary>
    ///     If not null, this sound will be played when being polymorphed into something.
    /// </summary>
    [DataField]
    public SoundSpecifier? PolymorphSound;

    /// <summary>
    ///     If not null, this sound will be played when being reverted from a polymorph.
    /// </summary>
    [DataField]
    public SoundSpecifier? ExitPolymorphSound;

    /// <summary>
    ///     If not null, this popup will be displayed when being polymorphed into something.
    /// </summary>
    [DataField]
    public LocId? PolymorphPopup = "polymorph-popup-generic";

    /// <summary>
    ///     If not null, this popup will be displayed when when being reverted from a polymorph.
    /// </summary>
    [DataField]
    public LocId? ExitPolymorphPopup = "polymorph-revert-popup-generic";

    /// <summary>
    /// Goobstation.
    /// If <see cref="Entity"/> is null, entity will be picked from this weighted random.
    /// Doesn't support polymorph actions.
    /// </summary>
    [DataField(serverOnly: true)]
    public ProtoId<WeightedRandomEntityPrototype>? Entities;

    /// <summary>
    /// Goobstation.
    /// If <see cref="Entity"/> and <see cref="Entities"/>> is null,
    /// weighted entity random will be picked from this weighted random.
    /// Doesn't support polymorph actions.
    /// </summary>
    [DataField(serverOnly: true)]
    public ProtoId<WeightedRandomPrototype>? Groups;

    /// <summary>
    /// Goobstation.
    /// Transfers these components on polymorph.
    /// Does nothing on revert.
    /// </summary>
    [DataField(serverOnly: true)]
    public HashSet<ComponentTransferData> ComponentsToTransfer = new()
    {
        new("LanguageKnowledge"),
        new("LanguageSpeaker"),
        new("Grammar"),
    };

    /// <summary>
    ///     Goobstation
    ///     Whether polymorphed entity should be able to move.
    /// </summary>
    [DataField]
    public bool AllowMovement = true;

    /// <summary>
    ///     Goobstation
    ///     Whether to show popup on polymorph revert.
    /// </summary>
    [DataField]
    public bool ShowPopup = true;

    /// <summary>
    ///     Goobstation
    ///     Whether to insert polymorphed entity into container or attach to grid or map.
    /// </summary>
    [DataField]
    public bool AttachToGridOrMap;

    /// <summary>
    ///     Goobstation
    ///     Skip revert action confirmation
    /// </summary>
    [DataField]
    public bool SkipRevertConfirmation;
}

public enum PolymorphInventoryChange : byte
{
    None,
    Drop,
    Transfer,
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class ComponentTransferData(string component, bool @override = true, bool mirror = false)
{
    [DataField(required: true)]
    public string Component = component;

    [DataField]
    public bool Override = @override;

    /// <summary>
    /// Whether we should copy the component data if false or just ensure it on a new entity if true
    /// </summary>
    [DataField]
    public bool Mirror = mirror;

    public ComponentTransferData() : this(string.Empty, true, false) { }
}

// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Emag.Systems;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;
using Content.Shared.Whitelist; // Shitmed - Starlight Abductors

namespace Content.Shared.Emag.Components;

[Access(typeof(EmagSystem))]
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EmagComponent : Component
{
    /// <summary>
    /// The tag that marks an entity as immune to emags
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public ProtoId<TagPrototype> EmagImmuneTag = "EmagImmune";

    /// <summary>
    /// What type of emag effect this device will do
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EmagType EmagType = EmagType.Interaction;

    /// <summary>
    /// What sound should the emag play when used
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public SoundSpecifier EmagSound = new SoundCollectionSpecifier("sparks");

    /// <summary>
    ///     Shitmed - Starlight Abductors: Entities that this EMAG works on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? ValidTargets;

    /// <summary>
    /// Goobstation - The text displayed when successful.
    /// </summary>
    [DataField]
    public LocId SuccessText = "emag-success";
}

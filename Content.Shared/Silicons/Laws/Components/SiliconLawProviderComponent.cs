// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Jajsha <101492056+Zap527@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <scarky0@onet.eu>
// SPDX-FileCopyrightText: 2024 Zachary Higgs <compgeek223@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Silicons.Laws.Components;

/// <summary>
/// This is used for an entity which grants laws to a <see cref="SiliconLawBoundComponent"/>
/// </summary>
[RegisterComponent, Access(typeof(SharedSiliconLawSystem))]
public sealed partial class SiliconLawProviderComponent : Component
{
    /// <summary>
    /// The id of the lawset that is being provided.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawsetPrototype> Laws = string.Empty;

    /// <summary>
    /// Lawset created from the prototype id.
    /// Cached when getting laws and modified during an ion storm event and when emagged.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SiliconLawset? Lawset;

    /// <summary>
    /// The sound that plays for the Silicon player
    /// when the law change is processed for the provider.
    /// </summary>
    [DataField]
    public SoundSpecifier? LawUploadSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");

    /// <summary>
    /// Whether this silicon is subverted by an ion storm or emag.
    /// </summary>
    [DataField]
    public bool Subverted = false;

}
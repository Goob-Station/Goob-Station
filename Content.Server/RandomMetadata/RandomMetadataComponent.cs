// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.RandomMetadata;

/// <summary>
///     Randomizes the description and/or the name for an entity by creating it from list of dataset prototypes or strings.
/// </summary>
[RegisterComponent]
public sealed partial class RandomMetadataComponent : Component
{
    [DataField("descriptionSegments")]
    public List<string>? DescriptionSegments;

    [DataField("nameSegments")]
    public List<string>? NameSegments;

    [DataField("nameSeparator")]
    public string NameSeparator = " ";

    [DataField("descriptionSeparator")]
    public string DescriptionSeparator = " ";
}
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace Content.PatreonParser;

public sealed class Attributes
{
    [JsonPropertyName("full_name")]
    public string FullName = default!;

    [JsonPropertyName("pledge_relationship_start")]
    public DateTime? PledgeRelationshipStart;

    [JsonPropertyName("title")]
    public string Title = default!;
}
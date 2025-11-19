// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using System.Text.Json.Serialization;

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

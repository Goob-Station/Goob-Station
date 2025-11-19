// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using System.Text.Json.Serialization;

namespace Content.PatreonParser;

public sealed class CurrentlyEntitledTiers
{
    [JsonPropertyName("data")]
    public List<TierData> Data = default!;
}

// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Myzumi <34660019+Myzumi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Whatstone <166147148+whatston3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Whatstone <whatston3@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.Json.Serialization;

namespace Content.Server.Discord;

// https://discord.com/developers/docs/resources/channel#message-object-message-structure
public struct WebhookPayload
{
    // Why is this here?
    // Why not make WebhookPayloadExtensions like a proper human being instead of shitting up what's not yours
    [JsonPropertyName("UserID")] // Frontier, this is used to identify the players in the webhook
    public Guid? UserID { get; set; }
    /// <summary>
    ///     The message to send in the webhook. Maximum of 2000 characters.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("embeds")]
    public List<WebhookEmbed>? Embeds { get; set; } = null;

    [JsonPropertyName("allowed_mentions")]
    public WebhookMentions AllowedMentions { get; set; } = new();

    public WebhookPayload()
    {
    }
}
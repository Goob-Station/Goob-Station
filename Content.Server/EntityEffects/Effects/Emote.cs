// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 āda <ss.adasts@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Chat; // Einstein Engines - Language
using Content.Shared.Chat.Prototypes;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Tries to force someone to emote (scream, laugh, etc). Still respects whitelists/blacklists and other limits unless specially forced.
/// </summary>
[UsedImplicitly]
public sealed partial class Emote : EntityEffect
{
    /// <summary>
    ///     The emote the entity will preform.
    /// </summary>
    [DataField("emote", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string EmoteId;

    /// <summary>
    ///     If the emote should be recorded in chat.
    /// </summary>
    [DataField]
    public bool ShowInChat;

    /// <summary>
    ///     If the forced emote will be listed in the guidebook.
    /// </summary>
    [DataField]
    public bool ShowInGuidebook;

    /// <summary>
    ///     If true, the entity will preform the emote even if they normally can't.
    /// </summary>
    [DataField]
    public bool Force = false;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!ShowInGuidebook)
            return null; // JUSTIFICATION: Emoting is mostly flavor, so same reason popup messages are not in here.

        return Loc.GetString("reagent-effect-guidebook-emote", ("chance", Probability), ("emote", EmoteId));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var chatSys = args.EntityManager.System<ChatSystem>();
        if (ShowInChat)
            chatSys.TryEmoteWithChat(args.TargetEntity, EmoteId, ChatTransmitRange.GhostRangeLimit, forceEmote: Force);
        else
            chatSys.TryEmoteWithoutChat(args.TargetEntity, EmoteId);
    }
}

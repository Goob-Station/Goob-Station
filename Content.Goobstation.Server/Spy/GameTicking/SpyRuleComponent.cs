// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Dataset;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Spy.GameTicking;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
[Access(typeof(SpyRuleSystem))]
public sealed partial class SpyRuleComponent : Component
{
    [ViewVariables]
    public readonly List<EntityUid> SpyMinds = [];

    [DataField]
    public ProtoId<AntagPrototype> TraitorPrototypeId = "Spy";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> SyndicateFaction = "Syndicate";

    [DataField]
    public ProtoId<DatasetPrototype> ObjectiveIssuers = "TraitorFlavor";

    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/spy_start.ogg");
}

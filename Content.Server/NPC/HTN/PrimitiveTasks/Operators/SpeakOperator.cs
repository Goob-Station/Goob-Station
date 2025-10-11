// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Chat; // Einstein Engines - Languages
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using static Content.Server.NPC.HTN.PrimitiveTasks.Operators.SpeakOperator.SpeakOperatorSpeech;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class SpeakOperator : HTNOperator
{
    private ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [DataField(required: true)]
    public SpeakOperatorSpeech Speech;

    /// <summary>
    /// Whether to hide message from chat window and logs.
    /// </summary>
    [DataField]
    public bool Hidden;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _chat = sysManager.GetEntitySystem<ChatSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        LocId speechLocId;
        switch (Speech)
        {
            case LocalizedSetSpeakOperatorSpeech localizedDataSet:
                if (!_proto.TryIndex(localizedDataSet.LineSet, out var speechSet))
                    return HTNOperatorStatus.Failed;
                speechLocId = _random.Pick(speechSet);
                break;
            case SingleSpeakOperatorSpeech single:
                speechLocId = single.Line;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Speech));
        }

        var speaker = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        _chat.TrySendInGameICMessage(
            speaker,
            Loc.GetString(speechLocId),
            InGameICChatType.Speak,
            hideChat: Hidden,
            hideLog: Hidden
        );

        return base.Update(blackboard, frameTime);
    }

    [ImplicitDataDefinitionForInheritors, MeansImplicitUse]
    public abstract partial class SpeakOperatorSpeech
    {
        public sealed partial class SingleSpeakOperatorSpeech : SpeakOperatorSpeech
        {
            [DataField(required: true)]
            public string Line;
        }

        public sealed partial class LocalizedSetSpeakOperatorSpeech : SpeakOperatorSpeech
        {
            [DataField(required: true)]
            public ProtoId<LocalizedDatasetPrototype> LineSet;
        }
    }
}

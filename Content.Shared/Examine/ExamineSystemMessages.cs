// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Verbs;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Examine
{
    public static class ExamineSystemMessages
    {
        [Serializable, NetSerializable]
        public sealed class RequestExamineInfoMessage : EntityEventArgs
        {
            public readonly NetEntity NetEntity;

            public readonly int Id;

            public readonly bool GetVerbs;

            public RequestExamineInfoMessage(NetEntity netEntity, int id, bool getVerbs=false)
            {
                NetEntity = netEntity;
                Id = id;
                GetVerbs = getVerbs;
            }
        }

        [Serializable, NetSerializable]
        public sealed class ExamineInfoResponseMessage : EntityEventArgs
        {
            public readonly NetEntity EntityUid;
            public readonly int Id;
            public readonly FormattedMessage Message;

            public List<Verb>? Verbs;

            public readonly bool CenterAtCursor;
            public readonly bool OpenAtOldTooltip;

            public readonly bool KnowTarget;

            public ExamineInfoResponseMessage(NetEntity entityUid, int id, FormattedMessage message, List<Verb>? verbs=null,
                bool centerAtCursor=true, bool openAtOldTooltip=true, bool knowTarget = true)
            {
                EntityUid = entityUid;
                Id = id;
                Message = message;
                Verbs = verbs;
                CenterAtCursor = centerAtCursor;
                OpenAtOldTooltip = openAtOldTooltip;
                KnowTarget = knowTarget;
            }
        }
    }
}
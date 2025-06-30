// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Examine;

public sealed class ExamineCompletedEvent : EntityEventArgs
{
    public FormattedMessage Message { get; }
    public EntityUid Examined { get; }
    public EntityUid Examiner { get; }
    public bool IsSecondaryInfo { get; }

    public ExamineCompletedEvent(FormattedMessage message, EntityUid examined, EntityUid examiner, bool isSecondaryInfo = false)
    {
        Message = message;
        Examined = examined;
        Examiner = examiner;
        IsSecondaryInfo = isSecondaryInfo;
    }
}
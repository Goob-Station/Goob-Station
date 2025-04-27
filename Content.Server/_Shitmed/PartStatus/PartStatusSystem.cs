// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.PartStatus.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Shitmed.PartStatus;
public sealed class PartStatusSystem : EntitySystem
{
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GetPartStatusEvent>(OnGetPartStatus);
    }

    private void OnGetPartStatus(GetPartStatusEvent message, EntitySessionEventArgs args)
    {
        var entity = GetEntity(message.Uid);
        if (_mobStateSystem.IsIncapacitated(entity))
            return;

        var info = GetExamineText(entity, entity);
    }

    public FormattedMessage GetExamineText(EntityUid entity, EntityUid examiner)
    {
        var message = new FormattedMessage();
        message.PushColor(Color.DarkGray);

        var examinedEvent = new PartStatusExaminedEvent(message,
            examiner,
            entity);

        RaiseLocalEvent(entity, examinedEvent);

        var newMessage = examinedEvent.GetTotalMessage();

        // Goobstation Change: I dont seem to have a way to get the event of examination to happen after EVERYTHING else, so fuck it.
        //var examineCompletedEvent = new ExamineCompletedEvent(newMessage, entity, examiner);
        //RaiseLocalEvent(entity, examineCompletedEvent);
        // pop color tag
        newMessage.Pop();

        return newMessage;
    }
}

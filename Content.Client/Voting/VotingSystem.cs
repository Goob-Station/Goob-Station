// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Voting;

namespace Content.Client.Voting;

public sealed class VotingSystem : EntitySystem
{
    public event Action<VotePlayerListResponseEvent>? VotePlayerListResponse; //Provides a list of players elligble for vote actions

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<VotePlayerListResponseEvent>(OnVotePlayerListResponseEvent);
    }

    private void OnVotePlayerListResponseEvent(VotePlayerListResponseEvent msg)
    {
        VotePlayerListResponse?.Invoke(msg);
    }

    public void RequestVotePlayerList()
    {
        RaiseNetworkEvent(new VotePlayerListRequestEvent());
    }
}
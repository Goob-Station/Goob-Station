// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Voting
{
    public delegate void VoteFinishedEventHandler(IVoteHandle sender, VoteFinishedEventArgs args);
    public delegate void VoteCancelledEventHandler(IVoteHandle sender);
}
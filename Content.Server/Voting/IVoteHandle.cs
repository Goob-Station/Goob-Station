// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Voting.Managers;
using Robust.Shared.Player;

namespace Content.Server.Voting
{
    /// <summary>
    /// A handle to vote, active or past.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vote options are referred to by UI/networking as integer IDs.
    /// These IDs are the index of the vote option in the <see cref="VoteOptions.Options"/> list
    /// used to create the vote.
    /// </para>
    /// </remarks>
    public interface IVoteHandle
    {
        /// <summary>
        /// The numeric ID of the vote. Can be used in <see cref="IVoteManager.TryGetVote"/>.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The title of the vote.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Text representing who/what initiated the vote.
        /// </summary>
        string InitiatorText { get; }

        /// <summary>
        /// Whether the vote has finished and is no longer active.
        /// </summary>
        bool Finished { get; }

        /// <summary>
        /// Whether the vote was cancelled by an administrator and did not finish naturally.
        /// </summary>
        /// <remarks>
        /// If this is true, <see cref="Finished"/> is also true.
        /// </remarks>
        bool Cancelled { get; }

        /// <summary>
        /// Dictionary of votes cast by players, matching the option's id.
        /// </summary>
        IReadOnlyDictionary<ICommonSession, int> CastVotes { get; }

        /// <summary>
        /// Current count of votes per option type.
        /// </summary>
        IReadOnlyDictionary<object, int> VotesPerOption { get; }

        /// <summary>
        /// Invoked when this vote has successfully finished.
        /// </summary>
        event VoteFinishedEventHandler OnFinished;

        /// <summary>
        /// Invoked if this vote gets cancelled.
        /// </summary>
        event VoteCancelledEventHandler OnCancelled;

        /// <summary>
        /// Check whether a certain integer option ID is valid.
        /// </summary>
        /// <param name="optionId">The integer ID of the option.</param>
        /// <returns>True if the option ID is valid, false otherwise.</returns>
        bool IsValidOption(int optionId);

        /// <summary>
        /// Cast a vote for a specific player.
        /// </summary>
        /// <param name="session">The player session to vote for.</param>
        /// <param name="optionId">
        /// The integer option ID to vote for. If null, "no vote" is selected (abstaining).
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="optionId"/> is not a valid option ID.
        /// </exception>
        void CastVote(ICommonSession session, int? optionId);

        /// <summary>
        /// Cancel this vote.
        /// </summary>
        void Cancel();
    }
}
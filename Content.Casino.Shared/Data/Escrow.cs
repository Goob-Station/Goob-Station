using Robust.Shared.Player;

namespace Content.Casino.Shared.Data;

/// <summary>
/// Stores goobcoins staked on a game
/// </summary>
/// <param name="Session">Session making the stake</param>
/// <param name="GameId">Game ID the stake is about</param>
/// <param name="Stake">Amount of goobcoins staked</param>
public record struct Escrow(ICommonSession Session, int GameId, int Stake);

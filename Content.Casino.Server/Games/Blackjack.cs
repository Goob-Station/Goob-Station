using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Games.Blackjack;
using Content.Casino.Shared.Cvars;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Configuration;

namespace Content.Casino.Server.Games;

public sealed class BlackjackGame : ICasinoGame
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly Dictionary<string, BlackjackGameState> _gameStates = new();

    public string GameId => "blackjack";
    public string DisplayName => "Blackjack";
    public string Description => "Classic 21 with splitting, doubling, and insurance";
    public int MinBet => 5;
    public int MaxBet => 1250; // Doubling raises this bet to 2500, threshold for "big win" is 5000 by default so.

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    public async Task<GameSession> StartGameAsync(ICommonSession player, int initialBet, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new GameSession(sessionId, GameId, player, initialBet, DateTime.UtcNow);

        // Get deck count from CVar
        var deckCount = _cfg.GetCVar(CasinoCVars.BlackjackDeckCount);

        // Create new deck and game state
        var deck = new Deck(new Random(_random.Next()), deckCount);
        var dealerHand = new List<Card>();
        var playerHands = new List<BlackjackHand>
        {
            new(new List<Card>(), initialBet)
        };

        var gameState = new BlackjackGameState(
            DealerHand: dealerHand,
            PlayerHands: playerHands,
            CurrentHandIndex: 0,
            Phase: BlackjackPhase.Dealing,
            GameDeck: deck,
            BaseBet: initialBet
        );

        // Deal initial cards
        gameState = DealInitialCards(gameState);
        _gameStates[sessionId] = gameState;

        // Create session with initial game state message
        var initialMessage = GetInitialDealMessage(gameState);
        session = session with { GameState = initialMessage };

        return await Task.FromResult(session);
    }

    private BlackjackGameState DealInitialCards(BlackjackGameState state)
    {
        var newState = state;

        // Deal two cards to player
        newState.PlayerHands[0].Cards.Add(newState.GameDeck.DrawCard());
        newState.PlayerHands[0].Cards.Add(newState.GameDeck.DrawCard());

        // Deal two cards to dealer
        newState.DealerHand.Add(newState.GameDeck.DrawCard());
        newState.DealerHand.Add(newState.GameDeck.DrawCard());

        // Check for dealer blackjack
        newState = newState with { DealerHasBlackjack = newState.DealerHand.IsBlackjack() };

        // Offer insurance if dealer shows ace
        var insuranceOffered = newState.DealerUpCard.IsAce;

        // Set initial phase
        var phase = BlackjackPhase.PlayerTurn;
        if (newState.PlayerHands[0].IsBlackjack && newState.DealerHasBlackjack)
        {
            phase = BlackjackPhase.GameOver;
            newState.PlayerHands[0] = newState.PlayerHands[0] with { Status = HandStatus.Blackjack };
        }
        else if (newState.PlayerHands[0].IsBlackjack)
        {
            newState.PlayerHands[0] = newState.PlayerHands[0] with { Status = HandStatus.Blackjack };
            phase = BlackjackPhase.DealerTurn;
        }

        return newState with
        {
            Phase = phase,
            InsuranceOffered = insuranceOffered,
            GameComplete = phase == BlackjackPhase.GameOver
        };
    }

    private string GetInitialDealMessage(BlackjackGameState gameState)
    {
        var playerHand = gameState.PlayerHands[0];
        var dealerDisplay = gameState.DealerDisplay;
        var playerDisplay = playerHand.Display;

        var message = $"Cards dealt! Dealer: {dealerDisplay} | Your hand: {playerDisplay} ({playerHand.Value})";

        if (playerHand.IsBlackjack)
        {
            message += " - BLACKJACK!";
            if (gameState.DealerHasBlackjack)
            {
                message += " Dealer also has blackjack - Push!";
            }
            else
            {
                message += " You win!";
            }
        }
        else if (gameState.DealerHasBlackjack)
        {
            message += " - Dealer has blackjack";
        }
        else if (gameState.InsuranceOffered)
        {
            message += " - Insurance available (dealer shows Ace)";
        }

        return message;
    }

    public async Task<GameActionCost> GetActionCostAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            return new GameActionCost(0, false);

        return action.ActionId switch
        {
            "hit" or "stand" or "status" => new GameActionCost(0, false),
            "double" => new GameActionCost(gameState.CurrentHand.Bet, true),
            "split" => new GameActionCost(gameState.CurrentHand.Bet, true),
            "insurance" => new GameActionCost(gameState.BaseBet / 2, true),
            _ => new GameActionCost(0, false)
        };
    }

    public async Task<GameActionResult> ExecuteActionAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            throw new InvalidOperationException("Game session not found");

        var result = action.ActionId switch
        {
            "hit" => await ExecuteHitAsync(sessionId, gameState),
            "stand" => await ExecuteStandAsync(sessionId, gameState),
            "double" => await ExecuteDoubleDownAsync(sessionId, gameState),
            "split" => await ExecuteSplitAsync(sessionId, gameState),
            "insurance" => await ExecuteInsuranceAsync(sessionId, gameState),
            _ => throw new ArgumentException($"Unknown action: {action.ActionId}")
        };

        return result;
    }

    private async Task<GameActionResult> ExecuteHitAsync(string sessionId, BlackjackGameState gameState)
    {
        if (gameState.Phase != BlackjackPhase.PlayerTurn)
            throw new InvalidOperationException("Cannot hit at this time");

        var currentHand = gameState.CurrentHand;
        if (currentHand.Status != HandStatus.Active)
            throw new InvalidOperationException("Current hand is not active");

        // Deal card
        currentHand.Cards.Add(gameState.GameDeck.DrawCard());

        var message = $"Hit: {currentHand.Cards[^1]} - Hand value: {currentHand.Value}";

        // Check for five-card win if enabled
        var fiveCardWinEnabled = _cfg.GetCVar(CasinoCVars.BlackjackFiveCardWin);
        if (fiveCardWinEnabled && currentHand.Cards.Count == 5 && !currentHand.IsBust)
        {
            currentHand = currentHand with { Status = HandStatus.FiveCardCharlie };
            gameState.PlayerHands[gameState.CurrentHandIndex] = currentHand;
            message += " - FIVE CARD CHARLIE! Automatic win!";

            var newState = MoveToNextHand(gameState);
            _gameStates[sessionId] = newState;

            return new GameActionResult(
                IsComplete: newState.GameComplete,
                Won: true,
                Payout: currentHand.Bet * 2, // Return bet + winnings
                Message: message
            );
        }

        // Check for bust
        if (currentHand.IsBust)
        {
            currentHand = currentHand with { Status = HandStatus.Bust };
            gameState.PlayerHands[gameState.CurrentHandIndex] = currentHand;
            message += " - BUST!";

            var newState = MoveToNextHand(gameState);
            _gameStates[sessionId] = newState;

            return new GameActionResult(
                IsComplete: newState.GameComplete,
                Won: false,
                Payout: 0,
                Message: message
            );
        }

        _gameStates[sessionId] = gameState;

        return await Task.FromResult(new GameActionResult(
            IsComplete: false,
            Won: false,
            Payout: 0,
            Message: message
        ));
    }

    private async Task<GameActionResult> ExecuteStandAsync(string sessionId, BlackjackGameState gameState)
    {
        if (gameState.Phase != BlackjackPhase.PlayerTurn)
            throw new InvalidOperationException("Cannot stand at this time");

        var currentHand = gameState.CurrentHand;
        if (currentHand.Status != HandStatus.Active)
            throw new InvalidOperationException("Current hand is not active");

        // Mark hand as standing
        gameState.PlayerHands[gameState.CurrentHandIndex] = currentHand with { Status = HandStatus.Stand };

        var newState = MoveToNextHand(gameState);
        _gameStates[sessionId] = newState;

        var message = $"Stand with {currentHand.Value}";

        // If game is complete, process final results
        if (newState.GameComplete)
        {
            var finalResult = ProcessFinalResults(newState);
            return new GameActionResult(
                IsComplete: true,
                Won: finalResult.TotalWin > 0,
                Payout: finalResult.TotalWin,
                Message: message + " - " + finalResult.Message
            );
        }

        return await Task.FromResult(new GameActionResult(
            IsComplete: false,
            Won: false,
            Payout: 0,
            Message: message
        ));
    }

    private async Task<GameActionResult> ExecuteDoubleDownAsync(string sessionId, BlackjackGameState gameState)
    {
        if (gameState.Phase != BlackjackPhase.PlayerTurn)
            throw new InvalidOperationException("Cannot double down at this time");

        var currentHand = gameState.CurrentHand;
        if (!currentHand.CanDoubleDown)
            throw new InvalidOperationException("Cannot double down on this hand");

        // Double the bet and deal one card
        var newBet = currentHand.Bet * 2;
        currentHand.Cards.Add(gameState.GameDeck.DrawCard());

        var message = $"Double Down: {currentHand.Cards[^1]} - Final hand value: {currentHand.Value}";

        // Check for five-card win if enabled (edge case with double down)
        var fiveCardWinEnabled = _cfg.GetCVar(CasinoCVars.BlackjackFiveCardWin);
        HandStatus newStatus;

        if (currentHand.IsBust)
        {
            newStatus = HandStatus.Bust;
            message += " - BUST!";
        }
        else if (fiveCardWinEnabled && currentHand.Cards.Count == 5)
        {
            newStatus = HandStatus.FiveCardCharlie;
            message += " - FIVE CARD CHARLIE!";
        }
        else
        {
            newStatus = HandStatus.Doubled;
        }

        gameState.PlayerHands[gameState.CurrentHandIndex] = currentHand with
        {
            Status = newStatus,
            Bet = newBet
        };

        var newState = MoveToNextHand(gameState);
        _gameStates[sessionId] = newState;

        // If game is complete, process final results
        if (newState.GameComplete)
        {
            var finalResult = ProcessFinalResults(newState);
            return new GameActionResult(
                IsComplete: true,
                Won: finalResult.TotalWin > 0,
                Payout: finalResult.TotalWin,
                Message: message + " - " + finalResult.Message
            );
        }

        return await Task.FromResult(new GameActionResult(
            IsComplete: false,
            Won: false,
            Payout: 0,
            Message: message
        ));
    }

    private async Task<GameActionResult> ExecuteSplitAsync(string sessionId, BlackjackGameState gameState)
    {
        if (gameState.Phase != BlackjackPhase.PlayerTurn)
            throw new InvalidOperationException("Cannot split at this time");

        var currentHand = gameState.CurrentHand;
        if (!currentHand.CanSplit)
            throw new InvalidOperationException("Cannot split this hand");

        // Create new hand with second card
        var secondCard = currentHand.Cards[1];
        var newHand = new BlackjackHand(new List<Card> { secondCard }, currentHand.Bet);

        // Keep first card in current hand
        currentHand.Cards.RemoveAt(1);

        // Deal new cards to both hands
        currentHand.Cards.Add(gameState.GameDeck.DrawCard());
        newHand.Cards.Add(gameState.GameDeck.DrawCard());

        // Insert new hand after current hand
        gameState.PlayerHands.Insert(gameState.CurrentHandIndex + 1, newHand);

        var message = $"Split! Hand 1: {currentHand.Display} ({currentHand.Value}), Hand 2: {newHand.Display} ({newHand.Value})";

        _gameStates[sessionId] = gameState;

        return await Task.FromResult(new GameActionResult(
            IsComplete: false,
            Won: false,
            Payout: 0,
            Message: message
        ));
    }

    private async Task<GameActionResult> ExecuteInsuranceAsync(string sessionId, BlackjackGameState gameState)
    {
        if (!gameState.CanTakeInsurance)
            throw new InvalidOperationException("Cannot take insurance at this time");

        // Mark current hand as having insurance
        var currentHand = gameState.CurrentHand;
        gameState.PlayerHands[gameState.CurrentHandIndex] = currentHand with { HasInsurance = true };

        var message = $"Insurance taken for {gameState.BaseBet / 2} coins";

        // If dealer has blackjack, insurance pays 2:1
        if (gameState.DealerHasBlackjack)
        {
            message += " - Insurance wins! Pays 2:1";
            var insurancePayout = gameState.BaseBet; // 2:1 on half bet = full bet

            // Check if player also has blackjack (push scenario)
            if (currentHand.IsBlackjack)
            {
                message += " - Player also has blackjack, push on main bet";
                return new GameActionResult(
                    IsComplete: true,
                    Won: true,
                    Payout: insurancePayout,
                    Message: message
                );
            }
            else
            {
                message += " - Player loses main bet";
                return new GameActionResult(
                    IsComplete: true,
                    Won: false,
                    Payout: insurancePayout,
                    Message: message
                );
            }
        }
        else
        {
            message += " - Insurance loses, dealer does not have blackjack";
            // Continue with normal game
            _gameStates[sessionId] = gameState;
            return new GameActionResult(
                IsComplete: false,
                Won: false,
                Payout: 0,
                Message: message
            );
        }
    }

    private BlackjackGameState MoveToNextHand(BlackjackGameState gameState)
    {
        var newIndex = gameState.CurrentHandIndex + 1;

        // If we've processed all hands, move to dealer turn
        if (newIndex >= gameState.PlayerHands.Count)
        {
            return ProcessDealerTurn(gameState with
            {
                Phase = BlackjackPhase.DealerTurn,
                CurrentHandIndex = 0
            });
        }

        return gameState with { CurrentHandIndex = newIndex };
    }

    private BlackjackGameState ProcessDealerTurn(BlackjackGameState gameState)
    {
        var dealerHitsSoft17 = _cfg.GetCVar(CasinoCVars.BlackjackDealerHitsSoft17);

        // Dealer plays based on CVar setting
        while (ShouldDealerHit(gameState.DealerHand, dealerHitsSoft17))
        {
            gameState.DealerHand.Add(gameState.GameDeck.DrawCard());
        }

        return gameState with
        {
            Phase = BlackjackPhase.GameOver,
            GameComplete = true
        };
    }

    private bool ShouldDealerHit(List<Card> dealerHand, bool hitSoft17)
    {
        var value = dealerHand.GetBestValue();

        // Always hit on 16 or less
        if (value < 17)
            return true;

        // Always stand on hard 17 or more
        if (value > 17)
            return false;

        // For exactly 17, check if it's soft and if dealer hits soft 17
        if (hitSoft17)
        {
            // Check if it's a soft 17 (contains ace counted as 11)
            var aceCount = dealerHand.Count(c => c.IsAce);
            var nonAceValue = dealerHand.Where(c => !c.IsAce).Sum(c => c.GetBlackjackValue());

            // Soft 17 means we have at least one ace counted as 11
            // If we have ace(s) and non-ace cards sum to 6 or less, then ace is counted as 11
            return aceCount > 0 && nonAceValue <= 6;
        }

        return false;
    }

    private (int TotalWin, string Message) ProcessFinalResults(BlackjackGameState gameState)
    {
        var totalWin = 0;
        var messages = new List<string>();
        var dealerValue = gameState.DealerValue;
        var dealerBust = dealerValue > 21;

        messages.Add($"Dealer: {gameState.DealerHand.GetHandDisplay()} ({dealerValue})" + (dealerBust ? " BUST" : ""));

        for (int i = 0; i < gameState.PlayerHands.Count; i++)
        {
            var hand = gameState.PlayerHands[i];
            var handMsg = $"Hand {i + 1}: {hand.Display} ({hand.Value})";

            if (hand.Status == HandStatus.Bust)
            {
                handMsg += " - BUST, lose " + hand.Bet;
            }
            else if (hand.Status == HandStatus.FiveCardCharlie)
            {
                var winAmount = hand.Bet * 2; // Return bet + winnings
                handMsg += $" - FIVE CARD CHARLIE! Win {winAmount}";
                totalWin += winAmount;
            }
            else if (hand.Status == HandStatus.Blackjack)
            {
                if (gameState.DealerHasBlackjack)
                {
                    handMsg += " - Push (both blackjack)";
                    totalWin += hand.Bet; // Return bet
                }
                else
                {
                    var blackjackPayout = (int)(hand.Bet * 2.5); // 3:2 payout
                    handMsg += $" - BLACKJACK! Win {blackjackPayout}";
                    totalWin += blackjackPayout;
                }
            }
            else
            {
                if (dealerBust)
                {
                    var winAmount = hand.Bet * 2; // Return bet + win
                    handMsg += $" - Dealer bust, win {winAmount}";
                    totalWin += winAmount;
                }
                else if (hand.Value > dealerValue)
                {
                    var winAmount = hand.Bet * 2;
                    handMsg += $" - Win {winAmount}";
                    totalWin += winAmount;
                }
                else if (hand.Value == dealerValue)
                {
                    handMsg += " - Push";
                    totalWin += hand.Bet; // Return bet
                }
                else
                {
                    handMsg += $" - Lose {hand.Bet}";
                }
            }

            messages.Add(handMsg);
        }

        return (totalWin, string.Join(" | ", messages));
    }

    public async Task<IReadOnlyList<GameAction>> GetAvailableActionsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            return Array.Empty<GameAction>();

        var actions = new List<GameAction>();

        if (gameState.Phase == BlackjackPhase.PlayerTurn)
        {
            var currentHand = gameState.CurrentHand;

            if (currentHand.Status == HandStatus.Active)
            {
                actions.Add(new GameAction("hit", "Hit"));
                actions.Add(new GameAction("stand", "Stand"));

                if (currentHand.CanDoubleDown)
                    actions.Add(new GameAction("double", $"Double Down (Bet: {currentHand.Bet})"));

                // Check CVar for split allowance
                var splitAllowed = _cfg.GetCVar(CasinoCVars.BlackjackSplitAllowed);
                if (splitAllowed && currentHand.CanSplit && gameState.PlayerHands.Count < 4) // Limit splits
                    actions.Add(new GameAction("split", $"Split (Bet: {currentHand.Bet})"));
            }

            if (gameState.CanTakeInsurance)
                actions.Add(new GameAction("insurance", $"Insurance (Bet: {gameState.BaseBet / 2})"));
        }

        return await Task.FromResult(actions);
    }

    public async Task EndGameAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _gameStates.Remove(sessionId);
        await Task.CompletedTask;
    }
}

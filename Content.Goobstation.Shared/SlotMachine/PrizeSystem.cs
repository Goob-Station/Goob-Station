using System.Linq;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.EntityTable;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.SlotMachine;

/// <summary>
/// Used for getting a weighted random prize from a list of prizes
/// </summary>
public sealed partial class PrizeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;

    /// <summary>
    /// Ts is taken from SharedRandomExtensions so don't blame me if the math isn't right
    /// </summary>
    /// <param name="prizes">List of prize prototypes to pick from</param>
    /// <returns></returns>
    public PrizePrototype GetRandomPrize(List<ProtoId<PrizePrototype>> prizes)
    {
        Dictionary<PrizePrototype, float> picks = new();

        foreach (var prize in prizes)
        {
            var proto = _proto.Index(prize);

            picks[proto] = proto.Weight;
        }

        var sum = picks.Values.Sum();
        var accumulated = 0f;

        var rand = _random.NextFloat() * sum;

        foreach (var (prize, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return prize;
            }
        }

        return _proto.Index(prizes[0]); // Shouldn't be possible but just incase
    }

    /// <summary>
    /// Selects a weighted random prize from the list, spawns it, plays its audio and depending on the AnnounceType speaks, popups or does nothing
    /// </summary>
    /// <param name="prizes"></param>
    /// <param name="uid">Whatever entity is spawning the prize</param>
    public void HandlePrize(List<ProtoId<PrizePrototype>> prizes, EntityUid uid)
    {
        var prize = GetRandomPrize(prizes);

        var win = _entityTable.GetSpawns(prize.PrizeTable);

        foreach (var item in win)
        {
            PredictedSpawnAtPosition(item, uid.ToCoordinates());
        }

        HandleAnnouncement(prize, uid);
        _audio.PlayPredicted(prize.WinSound, uid, uid);
    }

    private void HandleAnnouncement(PrizePrototype prize, EntityUid uid)
    {
        if (prize.WinMessage is null)
            return;

        switch (prize.AnnounceType)
        {
            case AnnounceType.Speak:
                _chatSystem.TrySendInGameICMessage(uid, Loc.GetString(prize.WinMessage), InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
                break;

            case AnnounceType.Popup:
                _popupSystem.PopupPredicted(Loc.GetString(prize.WinMessage), uid, uid);
                break;
        }
    }
}

using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Robust.Shared.Player;

namespace Content.Casino.Shared.Games;

public interface ICasinoGame
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public void Initialize();
    public Task<GameResult> Play(ICommonSession session, int bet);
}


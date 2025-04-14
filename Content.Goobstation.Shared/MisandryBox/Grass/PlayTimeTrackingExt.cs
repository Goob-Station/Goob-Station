using System.Threading.Tasks;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.MisandryBox.Grass;

// Called when minutes are added to a client
public delegate Task AddedPlaytimeMinutesCallback(ICommonSession player, int minutes);

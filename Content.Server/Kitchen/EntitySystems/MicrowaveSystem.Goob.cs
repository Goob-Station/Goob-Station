using Content.Goobstation.Common.Kitchen;

namespace Content.Server.Kitchen.EntitySystems;

public sealed partial class MicrowaveSystem
{
    private readonly Dictionary<EntityUid, EntityUid> _goobCooks = new();

    private void GoobRememberCook(EntityUid microwave, EntityUid? user)
    {
        if (user is { } cook)
            _goobCooks[microwave] = cook;
        else
            _goobCooks.Remove(microwave);
    }

    private void GoobRaiseCooked(EntityUid microwave, int count)
    {
        if (!_goobCooks.Remove(microwave, out var cook) || count <= 0 || TerminatingOrDeleted(cook))
            return;

        var ev = new FoodCookedEvent(microwave, count);
        RaiseLocalEvent(cook, ref ev);
    }
}

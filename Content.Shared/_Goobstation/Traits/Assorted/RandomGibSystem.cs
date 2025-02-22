using Content.Shared.Body.Systems;
using Robust.Shared.Random;
using System;

namespace Content.Shared.Traits.Assorted;

public sealed class RandomGibSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private TimeSpan _updateTime = TimeSpan.FromSeconds(1);
    private TimeSpan _updateCounter = TimeSpan.FromSeconds(0);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateCounter += TimeSpan.FromSeconds(frameTime);
        if (_updateCounter < _updateTime)
            return;
        _updateCounter -= _updateTime;

        var query = EntityQueryEnumerator<RandomGibComponent>();

        while (query.MoveNext(out var uid, out var randomGib))
        {
            if (_random.Prob(randomGib.Chance))
                _body.GibBody(uid);
        }
    }
}

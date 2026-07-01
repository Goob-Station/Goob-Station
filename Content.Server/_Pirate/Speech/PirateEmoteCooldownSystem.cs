// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Robust.Shared.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server._Pirate.Speech;

public sealed class PirateEmoteCooldownSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _emoteCooldown;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, CCVars.PirateEmoteCooldownSeconds, value => _emoteCooldown = TimeSpan.FromSeconds(value), true);
    }

    public bool TryEmote(EntityUid uid)
    {
        if (!HasComp<ActorComponent>(uid))
            return true;

        var cooldown = EnsureComp<PirateEmoteCooldownComponent>(uid);
        var time = _timing.CurTime;
        if (time < cooldown.NextEmote)
            return false;

        cooldown.NextEmote = time + _emoteCooldown;
        return true;
    }
}

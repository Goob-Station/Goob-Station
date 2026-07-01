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

    public bool CanEmote(EntityUid uid)
    {
        if (!HasComp<ActorComponent>(uid))
            return true;

        if (!TryComp<PirateEmoteCooldownComponent>(uid, out var cooldown))
            return true;

        var time = _timing.CurTime;
        return time >= cooldown.NextEmote;
    }

    public void CommitEmote(EntityUid uid)
    {
        if (!HasComp<ActorComponent>(uid))
            return;

        var cooldown = EnsureComp<PirateEmoteCooldownComponent>(uid);
        var time = _timing.CurTime;
        cooldown.NextEmote = time + _emoteCooldown;
    }
}

// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// Immediately kills all active particle emitters and their live particles.
/// Useful if something goes wrong and needs to be killed FAST..
/// </summary>
[AnyCommand]
public sealed class ParticlePanicCommand : IConsoleCommand
{
    public string Command => "particlepanic";
    public string Description => "Kills all active particle emitters and clears every live particle immediately.";
    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var particles = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ParticleSystem>();
        var count = particles.KillAll();
        shell.WriteLine($"Cleared {count} emitter(s)/particle(s).");
    }
}

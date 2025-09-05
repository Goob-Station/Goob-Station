using System.Text;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Random;

[AdminCommand(AdminFlags.Server)]
public sealed class TestGoobRandomCommand : IConsoleCommand
{
    public string Command => "testgoobrandom";
    public string Description => "Tests the GoobRandom system by generating a few random numbers.";
    public string Help => "testgoobrandom";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var random = IoCManager.Resolve<IGoobRandom>();
        var sb = new StringBuilder();

        sb.AppendLine("Testing GoobRandom:");
        sb.AppendLine($"Next(): {random.Next()}");
        sb.AppendLine($"Next(100): {random.Next(100)}");
        sb.AppendLine($"Next(50, 100): {random.Next(50, 100)}");
        sb.AppendLine($"NextFloat(): {random.NextFloat()}");
        sb.AppendLine($"NextDouble(): {random.NextDouble()}");

        shell.WriteLine(sb.ToString());
    }
}

[AdminCommand(AdminFlags.Server)]
public sealed class GoobRandomStatsCommand : IConsoleCommand
{
    public string Command => "goobrandomstats";
    public string Description => "Displays the current stats of the ApiRandomManager.";
    public string Help => "goobrandomstats";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var manager = IoCManager.Resolve<ApiRandomManager>();
        var sb = new StringBuilder();

        sb.AppendLine("ApiRandomManager Stats:");

        // This is a bit of a hack, but it's the easiest way to get the stats without making the fields public ¯\_(ツ)_/¯
        var intPoolField = typeof(ApiRandomManager).GetField("_intPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var floatPoolField = typeof(ApiRandomManager).GetField("_floatPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var intPool = (System.Collections.Concurrent.ConcurrentQueue<int>) intPoolField!.GetValue(manager)!;
        var floatPool = (System.Collections.Concurrent.ConcurrentQueue<float>) floatPoolField!.GetValue(manager)!;

        sb.AppendLine($"Int Pool Size: {intPool.Count}");
        sb.AppendLine($"Float Pool Size: {floatPool.Count}");

        shell.WriteLine(sb.ToString());
    }
}

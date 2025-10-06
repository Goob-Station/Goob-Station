using Robust.Client;

namespace Content.Trauma.Client;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ContentStart.Start(args);
    }
}

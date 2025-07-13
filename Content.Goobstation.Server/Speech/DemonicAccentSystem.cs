using Content.Goobstation.Common.Speech;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Speech;

public sealed class DemonicAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonicAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    private void OnAccentGet(Entity<DemonicAccentComponent> entity, ref AccentGetEvent args)
    {
        var words = args.Message.Split(' ');
        var wordsNew = new List<string>();

        foreach (var _ in words)
        {
            var pick = _random.Next(1, 13);

            wordsNew.Add(Loc.GetString($"accent-demonic-replace-{pick}"));
        }

        args.Message = string.Join(' ', wordsNew);
    }
}

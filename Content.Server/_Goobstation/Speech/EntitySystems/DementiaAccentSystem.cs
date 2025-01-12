using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class DementiaAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DementiaAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, DementiaAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "dementia");

        // Prefix
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 3);

            // Reverse sanitize capital
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-dementia-prefix-{pick}") + " " + message;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.3f))
        {
            var pick = _random.Next(1, 5);
            message += Loc.GetString($"accent-dementia-suffix-{pick}");
        }

        args.Message = message;
    }
};

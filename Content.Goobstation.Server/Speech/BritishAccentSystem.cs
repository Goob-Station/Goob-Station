using Content.Goobstation.Common.Speech;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Speech;

public sealed class BritishAccentSystem : EntitySystem
{   //shamelessly stolen from the dementia accent
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BritishAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, BritishAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "british");

        // Prefix
        if (_random.Prob(0.10f))
        {
            var pick = _random.Next(1, 5);

            // Reverse sanitize capital
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-british-prefix-{pick}") + " " + message;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.05f))
        {
            var pick = _random.Next(1, 6);
            message += Loc.GetString($"accent-british-suffix-{pick}");
        }

        args.Message = message;
    }
};
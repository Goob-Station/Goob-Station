using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class RodentiaAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RodentiaAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, RodentiaAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "rodentia");

        // Sanitize capital again, in case we substituted a word that should be capitalized
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        args.Message = message;
    }
};

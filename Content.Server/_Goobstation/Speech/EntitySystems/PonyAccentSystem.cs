using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class PonyAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PonyAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, PonyAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "pony");

        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        args.Message = message;
    }
};

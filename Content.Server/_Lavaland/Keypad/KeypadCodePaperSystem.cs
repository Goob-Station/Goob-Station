using Content.Shared._Lavaland.Keypad;
using Content.Shared.Paper;
using Content.Shared.GameTicking;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Keypad;

public sealed class KeypadCodePaperSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly KeypadSystem _keypad = default!;

    // Cache of generated codes keyed by KeypadGroup, so whichever entity
    // (keypad or paper) inits first generates it and other reads it back.
    private readonly Dictionary<string, string> _groupCodes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KeypadComponent, MapInitEvent>(OnKeypadMapInit);
        SubscribeLocalEvent<KeypadCodePaperComponent, MapInitEvent>(OnPaperMapInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _groupCodes.Clear();
    }

    // Generate or fetch a code if this keypad belongs to a group.
    private void OnKeypadMapInit(EntityUid uid, KeypadComponent comp, MapInitEvent args)
    {
        if (comp.KeypadGroup is null)
            return; // set in yaml = leave it alone

        var code = GetOrGenerateCode(comp.KeypadGroup, comp.MaxLength);
        _keypad.SetCode(uid, code, comp);
    }

    // Find the matching keypad's MaxLength and write the shared code.
    private void OnPaperMapInit(EntityUid uid, KeypadCodePaperComponent comp, MapInitEvent args)
    {
        if (!TryComp<PaperComponent>(uid, out var paperComp))
        {
            Log.Warning($"KeypadCodePaper {ToPrettyString(uid)} has no PaperComponent.");
            return;
        }

        if (!TryFindKeypadMaxLength(comp.KeypadGroup, out var maxLength))
        {
            Log.Warning($"KeypadCodePaper {ToPrettyString(uid)} could not find a keypad with group '{comp.KeypadGroup}'.");
            return;
        }

        var code = GetOrGenerateCode(comp.KeypadGroup, maxLength);
        var content = Loc.GetString(comp.ContentLocId, ("code", code));
        _paper.SetContent((uid, paperComp), content);
    }

    private bool TryFindKeypadMaxLength(string group, out int maxLength)
    {
        maxLength = 0;

        var query = EntityQueryEnumerator<KeypadComponent>();
        while (query.MoveNext(out _, out var keypad))
        {
            if (keypad.KeypadGroup != group)
            {
                continue;
            }

            maxLength = keypad.MaxLength;
            return true;
        }

        return false;
    }

    // Returns the cached code for a group if one was already generated this round,
    // otherwise generates a new zero-padded numeric code and caches it.
    private string GetOrGenerateCode(string group, int length)
    {
        if (_groupCodes.TryGetValue(group, out var existing))
            return existing;

        var max = (int) Math.Pow(10, length);
        var code = _random.Next(0, max).ToString().PadLeft(length, '0');
        _groupCodes[group] = code;
        return code;
    }
}

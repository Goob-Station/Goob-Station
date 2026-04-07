using Content.Goobstation.Shared.Keypad;
using Content.Shared.Paper;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Keypad;

public sealed class KeypadCodePaperSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly KeypadSystem _keypad = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Keypads generate their code first, then papers read it.
        SubscribeLocalEvent<KeypadComponent, MapInitEvent>(OnKeypadMapInit);
        SubscribeLocalEvent<KeypadCodePaperComponent, MapInitEvent>(OnPaperMapInit);
    }

    // generate a random code if this keypad belongs to a group

    private void OnKeypadMapInit(EntityUid uid, KeypadComponent comp, MapInitEvent args)
    {
        if (comp.KeypadGroup == null)
            return; // Statically coded keypads (set in YAML) are left alone.

        // Generate a zero-padded numeric code matching MaxLength.
        // e.g. MaxLength 4 → "0000" to "9999"
        var max = (int) Math.Pow(10, comp.MaxLength);
        var code = _random.Next(0, max).ToString().PadLeft(comp.MaxLength, '0');
        _keypad.SetCode(uid, code, comp);
    }

    // find the matching keypad and write its code

    private void OnPaperMapInit(EntityUid uid, KeypadCodePaperComponent comp, MapInitEvent args)
    {
        if (!TryGetCodeForGroup(comp.KeypadGroup, out var code))
        {
            Log.Warning($"KeypadCodePaper {ToPrettyString(uid)} could not find a keypad with group '{comp.KeypadGroup}'.");
            return;
        }

        if (!TryComp<PaperComponent>(uid, out var paperComp))
        {
            Log.Warning($"KeypadCodePaper {ToPrettyString(uid)} has no PaperComponent.");
            return;
        }

        var content = Loc.GetString("keypad-code-paper-content", ("code", code), ("group", comp.KeypadGroup));
        _paper.SetContent((uid, paperComp), content);
    }

    private bool TryGetCodeForGroup(string group, out string code)
    {
        code = string.Empty;

        var query = EntityQueryEnumerator<KeypadComponent>();
        while (query.MoveNext(out _, out var keypad))
        {
            if (keypad.KeypadGroup != group)
                continue;

            code = keypad.Code;
            return true;
        }

        return false;
    }
}

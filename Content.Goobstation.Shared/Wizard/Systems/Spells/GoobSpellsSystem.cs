
using Content.Goobstation.Common.Religion;
using Content.Shared.Chat;
using Content.Shared.Magic;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wizard.Systems;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public sealed partial class SharedGoobSpellsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private LocId _locFailSilicon = "spell-fail-target-silicon";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreamForMeEvent>(OnScreamForMe);
    }

    private bool IsTouchSpellDenied(EntityUid target)
    {
        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);

        return ev.Cancelled;
    }
}
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class SleepingCarpSystem : EntitySystem
{
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private readonly ProtoId<MartialArtPrototype> _sleepingCarp = "SleepingCarp";

    [SubscribeLocalEvent]
    private void OnShotAttempt(Entity<MartialArtsKnowledgeComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!_martialArts.HasMartialArtActive(ent.AsNullable(), _proto.Index(_sleepingCarp)))
            return;

        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}

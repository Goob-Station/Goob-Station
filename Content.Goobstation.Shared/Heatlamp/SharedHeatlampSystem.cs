using Content.Shared.Emag.Systems;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.PowerCell.Components;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Heatlamp;

public abstract class SharedHeatlampSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, HeatlampComponent component, ref GotEmaggedEvent args)
    {
        // Check that we're using an emag
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        // Check that we aren't already emagged
        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        // Update our appearance
        _appearance.SetData(uid, HeatlampVisuals.IsEmagged, true);

        // Tell EmagSystem to use the charge
        args.Handled = true;
    }
}

[Serializable, NetSerializable]
public enum HeatlampVisuals : byte
{
    IsPowered,
    IsEmagged
}

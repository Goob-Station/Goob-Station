using Content.Goobstation.Shared.Heatlamp;
using Content.Server.PowerCell;
using Content.Shared.Emag.Systems;
using Content.Shared.PowerCell.Components;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Heatlamp;

/// <summary>
///     Handles heatlamps
/// </summary>
public sealed class HeatlampSystem : SharedHeatlampSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly PowerCellSystem _powercell = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HeatlampComponent, PowerCellChangedEvent>(OnPowerCellChanged);
    }

    private void OnPowerCellChanged(EntityUid uid, HeatlampComponent component, PowerCellChangedEvent args)
    {
        // Update power visuals
        _appearance.SetData(uid, HeatlampVisuals.IsPowered, !(args.Ejected || !_powercell.HasCharge(uid, 0.01f)));
    }
}

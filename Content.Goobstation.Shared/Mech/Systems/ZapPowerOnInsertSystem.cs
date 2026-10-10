using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class ZapPowerOnInsertSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZapPowerOnInsertComponent, EntInsertedIntoContainerMessage>(OnInserted);
    }

    private void OnInserted(Entity<ZapPowerOnInsertComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId
            || args.OldParent == ent.Owner
            || !TryComp<BatteryComponent>(args.Entity, out var battery))
            return;

        var charge = MathF.Max(0f, _battery.GetCharge((args.Entity, battery)) - ent.Comp.ChargeLoss);
        _battery.SetCharge((args.Entity, battery), charge);

        _audio.PlayPredicted(ent.Comp.ZapSound, ent, null);
        _popup.PopupPredicted(Loc.GetString("zap-power-on-insert-popup", ("battery", args.Entity)), ent, null, PopupType.MediumCaution);
    }
}

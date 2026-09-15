using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Tag;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.WallConversion;

public sealed partial class VoidwalkerWallConversionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerWallConversionComponent, VoidwalkerConvertWallDoAfterEvent>(OnConvertWallDoAfter);
    }

    private void StartConvertWall(Entity<VoidwalkerWallConversionComponent> entity, EntityUid target)
    {
        var popup = Loc.GetString("voidwalker-convert-wall-begin", ("user", Name(entity.Owner)));
        _popup.PopupEntity(popup, target, PopupType.SmallCaution);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.WallConvertTime,
            new VoidwalkerConvertWallDoAfterEvent(),
            eventTarget: entity,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnConvertWallDoAfter(Entity<VoidwalkerWallConversionComponent> entity, ref VoidwalkerConvertWallDoAfterEvent args)
    {
        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        EnsureComp<VoidedVisualsComponent>(target);
        _tag.AddTag(target, entity.Comp.VoidedStructureTag); // TODO: Replace this with component later so it can be repaired by chaplain.
    }
}

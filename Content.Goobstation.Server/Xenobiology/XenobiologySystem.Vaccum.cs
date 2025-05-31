using System.Linq;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles the XenoVacuum and it's interactions.
/// </summary>
public partial class XenobiologySystem
{
    private void InitializeVacuum()
    {
        SubscribeLocalEvent<XenoVacuumComponent, GotEmaggedEvent>(OnVacEmagged);

        SubscribeLocalEvent<XenoVacuumComponent, AfterInteractEvent>(OnXenoVacuum);
        //SubscribeLocalEvent<XenoVacuumComponent, UseInHandEvent>(OnXenoVacuumClear);
    }

    private void OnVacEmagged(Entity<XenoVacuumComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        if (ent.Comp.IsEmagged)
            return;

        args.Handled = true;
        ent.Comp.IsEmagged = true;
    }

    private void OnXenoVacuum(Entity<XenoVacuumComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target is not { } target
            || TerminatingOrDeleted(target)
            || !args.CanReach
            || !HasComp<MobStateComponent>(target))
        {
            Logger.Debug("Is not a valid Target");
            return;
        }

        var user = args.User;
        var comp = ent.Comp;

        if (!_inventorySystem.TryGetSlotEntity(user, "back", out var backSlotEntity))
            return;

        if (backSlotEntity is not { } tank)
        {
            Logger.Debug("Is not Tank");
            return;
        }

        if (!_tagSystem.HasTag(tank, comp.XenoTankTag))
        {
            Logger.Debug("HasTagFailed");
            return;
        }

        var container = _containerSystem.GetContainer(tank, comp.StorageID);

        if (container.ContainedEntities.Count != 0)
        {
            Logger.Debug("Container Full");
            return;
        }

        var isSlime = HasComp<SlimeComponent>(target);
        if (!isSlime && !comp.IsEmagged)
        {
            Logger.Debug("Is not Slime, Is not Emagged");
            return;
        }

        if (!EntityManager.EnsureComponent<PseudoItemComponent>(target, out var pseudo))
            pseudo.IntendedComp = false;

        if (!_pseudoSystem.TryInsert(tank, target, pseudo)
            && !pseudo.IntendedComp)
        {
            Logger.Debug("Insert Failed");
            RemCompDeferred<PseudoItemComponent>(target);
            return;
        }

        _audio.PlayPredicted(comp.Sound, ent, user);
    }

    /*private void OnXenoVacuumClear(Entity<XenoVacuumComponent> ent, ref UseInHandEvent args)
    {
        var user = args.User;
        var comp = ent.Comp;

        if (!_inventorySystem.TryGetSlotEntity(user, "back", out var backSlotEntity))
            return;

        if (!_tagSystem.HasTag(backSlotEntity.Value, comp.XenoTankTag))
            return;

        var container = _containerSystem.GetContainer(backSlotEntity.Value, comp.StorageID);
        if (container.ContainedEntities.Count <= 0)
            return;

        var contained = container.ContainedEntities.FirstOrDefault();
        if (contained != default
            && TryComp<PseudoItemComponent>(contained, out var pseudo)
            && !pseudo.IntendedComp)
            RemCompDeferred<PseudoItemComponent>(contained);

        _containerSystem.EmptyContainer(container);
        _audio.PlayPredicted(comp.ClearSound, ent, user, AudioParams.Default.WithVolume(-2f));
    }*/
}

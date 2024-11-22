using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.ListViewSelector;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Items.VoidTorch;
using Robust.Server.Audio;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Items.VoidTorch;

public sealed class VoidTorchSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidTorchComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<VoidTorchComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<VoidTorchComponent, ListViewItemSelectedMessage>(OnCultistSelected);
    }

    private void OnComponentStartup(Entity<VoidTorchComponent> torch, ref ComponentStartup args)
    {
        _appearance.SetData(torch, GenericCultVisuals.State, true);
    }

    private void OnAfterInteract(Entity<VoidTorchComponent> torch, ref AfterInteractEvent args)
    {
        if (torch.Comp.Charges == 0 || args.Target is not { } target || !HasComp<ItemComponent>(target))
            return;

        var cultistsQuery = EntityQueryEnumerator<BloodCultistComponent>();
        var cultist = new List<ListViewSelectorEntry>();
        while (cultistsQuery.MoveNext(out var cultistUid, out _))
        {
            if (cultistUid == args.User)
                continue;

            var metaData = MetaData(cultistUid);
            var entry = new ListViewSelectorEntry(cultistUid.ToString(),
                metaData.EntityName,
                metaData.EntityDescription);

            cultist.Add(entry);
        }

        if (cultist.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-no-targets"), args.User, args.User);
            args.Handled = true;
            return;
        }

        torch.Comp.TargetItem = target;
        _ui.SetUiState(torch.Owner, ListViewSelectorUiKey.Key, new ListViewSelectorState(cultist));
        _ui.TryToggleUi(torch.Owner, ListViewSelectorUiKey.Key, args.User);
    }

    private void OnCultistSelected(Entity<VoidTorchComponent> torch, ref ListViewItemSelectedMessage args)
    {
        if (!EntityUid.TryParse(args.SelectedItem.Id, out var target) || torch.Comp.TargetItem is not { } item)
            return;

        var targetTransform = Transform(target);

        _transform.SetCoordinates(item, targetTransform.Coordinates);
        _hands.TryPickupAnyHand(target, item);

        _audio.PlayPvs(torch.Comp.TeleportSound, torch);

        torch.Comp.Charges--;
        if (torch.Comp.Charges == 0)
            _appearance.SetData(torch, GenericCultVisuals.State, false);
    }
}

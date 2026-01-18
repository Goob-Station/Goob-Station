using Content.Goobstation.Shared.Doodon.Building;
using Content.Goobstation.Shared.Doodons;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System;

namespace Content.Goobstation.Server.Doodon.Building;

public sealed class DoodonBuildMenuBuiSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoodonBuilderComponent, DoodonOpenBuildMenuActionEvent>(OnOpenMenu);

        SubscribeLocalEvent<DoodonBuildMenuComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<DoodonBuildMenuComponent, DoodonBuildSelectMessage>(OnSelected);
    }

    private void OnOpenMenu(Entity<DoodonBuilderComponent> ent, ref DoodonOpenBuildMenuActionEvent args)
    {
        if (!HasComp<DoodonBuildMenuComponent>(ent.Owner))
            return;

        _ui.OpenUi(ent.Owner, DoodonBuildUiKey.Key, args.Performer);
        args.Handled = true;
    }

    private void OnUiOpened(Entity<DoodonBuildMenuComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!TryComp<DoodonBuilderComponent>(ent.Owner, out var builder))
            return;

        SendState(ent.Owner, builder);
    }

    private void OnSelected(Entity<DoodonBuildMenuComponent> ent, ref DoodonBuildSelectMessage msg)
    {
        if (!TryComp<DoodonBuilderComponent>(ent.Owner, out var builder))
            return;

        for (var i = 0; i < builder.Buildables.Count; i++)
        {
            if (!string.Equals(builder.Buildables[i].ToString(), msg.PrototypeId, StringComparison.Ordinal))
                continue;

            builder.SelectedIndex = i;
            Dirty(ent.Owner, builder);
            break;
        }

        // Popup selected building + cost
        var sel = builder.GetSelected();
        if (sel != null)
        {
            var name = sel.Value.ToString();
            var cost = 0;

            if (_proto.TryIndex<EntityPrototype>(sel.Value, out var proto))
            {
                name = proto.Name;
                if (proto.TryGetComponent(out DoodonBuildingComponent? b))
                    cost = Math.Max(0, b.ResinCost);
            }

            _popup.PopupEntity($"Selected: {name} ({cost} resin)", ent.Owner, msg.Actor);
        }

        _ui.CloseUi(ent.Owner, DoodonBuildUiKey.Key);
    }

    private void SendState(EntityUid owner, DoodonBuilderComponent builder)
    {
        var entries = new DoodonBuildMenuEntry[builder.Buildables.Count];

        for (var i = 0; i < builder.Buildables.Count; i++)
        {
            var id = builder.Buildables[i].ToString();
            var name = id;
            Robust.Shared.Utility.SpriteSpecifier? icon = null;
            var resinCost = 0;
            string? desc = null;

            if (_proto.TryIndex<EntityPrototype>(builder.Buildables[i], out var entProto))
            {
                name = entProto.Name;
                desc = entProto.Description;

                if (entProto.TryGetComponent(out DoodonBuildingComponent? doodonBuilding))
                {
                    icon = doodonBuilding.BuildIcon;
                    resinCost = Math.Max(0, doodonBuilding.ResinCost);
                }
            }

            entries[i] = new DoodonBuildMenuEntry(id, name, icon, resinCost, desc);
        }

        _ui.SetUiState(owner, DoodonBuildUiKey.Key, new DoodonBuildMenuState(entries, builder.SelectedIndex));
    }
}

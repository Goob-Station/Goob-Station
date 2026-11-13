// SPDX-License-Identifier: AGPL-3.0-or-later
//Stuff for Master silo if breaks find @Paul44ur ill fix it!! :)
using System.Linq;
using Content.Shared.Materials;
using Content.Shared.Materials.OreSilo;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Materials.UI;

public sealed partial class OreSiloMenu
{
    private const int DefaultTransferSheets = 10;

    public event Action<NetEntity, string, int>? OnTransferPressed;

    private IEntityManager _entMan = default!;
    private IPrototypeManager _proto = default!;
    private MaterialStorageSystem _materialStorage = default!;

    private EntityUid? _masterSilo;
    private List<(NetEntity Entity, string Name, Dictionary<string, int> Materials)> _targets = new();
    private List<string> _materials = new();
    private List<string> _targetLabels = new();
    private List<string> _materialLabels = new();

    private NetEntity? _selectedTarget;
    private string? _selectedMaterial;

    public void SetMasterSilo(EntityUid uid)
    {
        _entMan = IoCManager.Resolve<IEntityManager>();
        _proto = IoCManager.Resolve<IPrototypeManager>();
        _materialStorage = _entMan.System<MaterialStorageSystem>();
        _masterSilo = uid;

        Title = Loc.GetString("ore-silo-ui-title-master");
        ClientsLabel.Text = Loc.GetString("ore-silo-ui-label-clients-master");
        TransferPanel.Visible = true;
        MinSize = new(MinSize.X, MinSize.Y + 150);
        SetSize = new(SetSize.X, SetSize.Y + 150);

        TransferAmount.IsValid = value => value > 0;
        TransferAmount.Value = DefaultTransferSheets;

        TransferTarget.OnItemSelected += args =>
        {
            _selectedTarget = _targets[args.Id].Entity;
            TransferTarget.SelectId(args.Id);
            UpdateTransferControls();
        };

        TransferMaterial.OnItemSelected += args =>
        {
            _selectedMaterial = _materials[args.Id];
            TransferMaterial.SelectId(args.Id);
            UpdateTransferControls();
        };

        TransferButton.OnPressed += _ =>
        {
            if (_selectedTarget is { } target && _selectedMaterial is { } material && TransferAmount.Value > 0)
                OnTransferPressed?.Invoke(target, material, TransferAmount.Value);
        };

        UpdateTransferControls();
    }

    private void UpdateTransfer(OreSiloBuiState state)
    {
        if (_masterSilo is not { } master)
            return;

        _targets = state.Targets.OrderBy(t => t.Name).ThenBy(t => t.Entity.Id).ToList();
        SetOptions(TransferTarget, ref _targetLabels, _targets.Select(t => t.Name).ToList());

        var targetIndex = _targets.FindIndex(t => t.Entity == _selectedTarget);
        if (targetIndex == -1 && _targets.Count > 0)
            targetIndex = 0;

        _selectedTarget = targetIndex == -1 ? null : _targets[targetIndex].Entity;
        if (targetIndex != -1)
            TransferTarget.SelectId(targetIndex);

        var stock = _materialStorage.GetStoredMaterials(master, true);
        _materials = stock.Where(pair => pair.Value > 0)
            .Select(pair => pair.Key.Id)
            .Where(id => _proto.HasIndex<MaterialPrototype>(id))
            .OrderBy(MaterialName)
            .ToList();

        var materialLabels = _materials.Select(id => Loc.GetString("ore-silo-ui-transfer-material-entry",
                ("material", MaterialName(id)),
                ("amount", SheetText(id, stock[id]))))
            .ToList();
        SetOptions(TransferMaterial, ref _materialLabels, materialLabels);

        var materialIndex = _selectedMaterial == null ? -1 : _materials.IndexOf(_selectedMaterial);
        if (materialIndex == -1 && _materials.Count > 0)
            materialIndex = 0;

        _selectedMaterial = materialIndex == -1 ? null : _materials[materialIndex];
        if (materialIndex != -1)
            TransferMaterial.SelectId(materialIndex);

        UpdateTransferControls();
    }

    private void UpdateTransferControls()
    {
        TransferTarget.Disabled = _targets.Count == 0;
        TransferMaterial.Disabled = _materials.Count == 0;
        TransferButton.Disabled = _selectedTarget == null || _selectedMaterial == null;

        var targetIndex = _targets.FindIndex(t => t.Entity == _selectedTarget);
        if (targetIndex == -1)
        {
            TransferTargetStock.Text = Loc.GetString("ore-silo-ui-transfer-no-targets");
            return;
        }

        var stock = _targets[targetIndex].Materials
            .Where(pair => pair.Value > 0 && _proto.HasIndex<MaterialPrototype>(pair.Key))
            .OrderBy(pair => MaterialName(pair.Key))
            .Select(pair => Loc.GetString("ore-silo-ui-transfer-material-entry",
                ("material", MaterialName(pair.Key)),
                ("amount", SheetText(pair.Key, pair.Value))))
            .ToList();

        TransferTargetStock.Text = stock.Count == 0
            ? Loc.GetString("ore-silo-ui-transfer-stock-empty")
            : Loc.GetString("ore-silo-ui-transfer-stock", ("stock", string.Join(", ", stock)));
    }

    private static void SetOptions(OptionButton button, ref List<string> current, List<string> labels)
    {
        if (current.SequenceEqual(labels))
            return;

        current = labels;
        button.Clear();
        for (var i = 0; i < labels.Count; i++)
        {
            button.AddItem(labels[i], i);
        }
    }

    private string MaterialName(string id)
    {
        return Loc.GetString(_proto.Index<MaterialPrototype>(id).Name);
    }

    private string SheetText(string id, int volume)
    {
        var material = _proto.Index<MaterialPrototype>(id);
        var sheetVolume = Math.Max(1, _materialStorage.GetSheetVolume(material));
        var sheets = (float) volume / sheetVolume;
        return Loc.GetString("lathe-menu-material-amount", ("amount", sheets), ("unit", Loc.GetString(material.Unit)));
    }
}

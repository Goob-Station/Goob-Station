// SPDX-License-Identifier: AGPL-3.0-or-later
//More for the Master Silo @Paul44ur
using Content.Goobstation.Shared.Materials.OreSilo;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Materials;
using Content.Shared.Materials.OreSilo;
using Robust.Shared.Prototypes;

namespace Content.Server.Materials;

public sealed partial class OreSiloSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;

    private const float MasterSiloUiUpdateInterval = 1f;

    private float _masterSiloUiAccumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OreSiloComponent, OreSiloTransferMessage>(OnTransferMessage);
    }

    private void OnTransferMessage(Entity<OreSiloComponent> ent, ref OreSiloTransferMessage args)
    {
        var target = GetEntity(args.Target);

        if (!TryTransferMaterial(ent, target, args.Material, args.Sheets, out var volume))
            return;

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(args.Actor):player} sent {volume} units of {args.Material} from {ToPrettyString(ent):silo} to {ToPrettyString(target):target}");

        UpdateOreSiloUi(ent);
    }

    public bool TryTransferMaterial(Entity<OreSiloComponent> master,
        EntityUid target,
        ProtoId<MaterialPrototype> material,
        int sheets,
        out int volume)
    {
        volume = 0;

        if (!master.Comp.MasterSilo || sheets <= 0)
            return false;

        if (!master.Comp.Clients.Contains(target) || !CanTransmitMaterials((master, master), target))
            return false;

        if (!_prototype.TryIndex(material, out var materialProto))
            return false;

        if (!TryComp<MaterialStorageComponent>(master, out var storage) ||
            !TryComp<MaterialStorageComponent>(target, out var targetStorage))
            return false;

        var sheetVolume = _materialStorage.GetSheetVolume(materialProto);
        var available = _materialStorage.GetMaterialAmount(master, material, storage, localOnly: true);
        volume = (int) Math.Min((long) sheets * sheetVolume, available);

        if (volume <= 0)
            return false;

        if (!_materialStorage.CanChangeMaterialAmount(target, material, volume, targetStorage, localOnly: true))
            return false;

        if (!_materialStorage.TryChangeMaterialAmount(master, material, -volume, storage, localOnly: true))
            return false;

        _materialStorage.TryChangeMaterialAmount(target, material, volume, targetStorage, localOnly: true);
        return true;
    }

    private void AddTransferTargets(Entity<OreSiloComponent> ent, OreSiloBuiState state)
    {
        foreach (var client in ent.Comp.Clients)
        {
            if (!CanTransmitMaterials((ent, ent), client))
                continue;

            var name = Loc.GetString("ore-silo-ui-transfer-target-entry",
                ("name", Identity.Name(client, EntityManager)),
                ("beacon", _navMap.GetNearestBeaconString(client, onlyName: true)));

            var materials = new Dictionary<string, int>();
            foreach (var (material, amount) in _materialStorage.GetStoredMaterials(client, localOnly: true))
            {
                materials[material] = amount;
            }

            state.Targets.Add((GetNetEntity(client), name, materials));
        }
    }

    private void UpdateMasterSilos(float frameTime)
    {
        _masterSiloUiAccumulator += frameTime;
        if (_masterSiloUiAccumulator < MasterSiloUiUpdateInterval)
            return;

        _masterSiloUiAccumulator = 0f;

        var query = EntityQueryEnumerator<OreSiloComponent>();
        while (query.MoveNext(out var uid, out var silo))
        {
            if (silo.MasterSilo)
                UpdateOreSiloUi((uid, silo));
        }
    }
}

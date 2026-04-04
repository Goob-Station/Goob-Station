// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Checkraze <71046427+Cheackraze@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Item;
using Content.Shared.Prototypes;
using Content.Shared.Storage.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Cargo;

public abstract class SharedCargoSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    // CorvaxGoob-CargoFeatures-Start
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly AccessReaderSystem AccessReader = default!;
    [Dependency] protected readonly SharedIdCardSystem IdCard = default!;
    // CorvaxGoob-CargoFeatures-End

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationBankAccountComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<StationBankAccountComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextIncomeTime = Timing.CurTime + ent.Comp.IncomeDelay;
        Dirty(ent);
    }

    /// <summary>
    /// For a given station, retrieves the balance in a specific account.
    /// </summary>
    public int GetBalanceFromAccount(Entity<StationBankAccountComponent?> station, ProtoId<CargoAccountPrototype> account)
    {
        if (!Resolve(station, ref station.Comp))
            return 0;

        return station.Comp.Accounts.GetValueOrDefault(account);
    }

    /// <summary>
    /// For a station, creates a distribution between one the bank's account and the other accounts.
    /// The primary account receives the majority percentage listed on the bank account, with the remaining
    /// funds distributed to all accounts based on <see cref="StationBankAccountComponent.RevenueDistribution"/>
    /// </summary>
    public Dictionary<ProtoId<CargoAccountPrototype>, double> CreateAccountDistribution(Entity<StationBankAccountComponent> stationBank)
    {
        var distribution = new Dictionary<ProtoId<CargoAccountPrototype>, double>
        {
            { stationBank.Comp.PrimaryAccount, stationBank.Comp.PrimaryCut }
        };
        var remaining = 1.0 - stationBank.Comp.PrimaryCut;

        foreach (var (account, percentage) in stationBank.Comp.RevenueDistribution)
        {
            var existing = distribution.GetOrNew(account);
            distribution[account] = existing + remaining * percentage;
        }
        return distribution;
    }

    // CorvaxGoob-CargoFeatures-Start
    /// <summary>
    /// Проверяет, может ли прототип продукта помещен в защищенный ящик отдела.
    /// </summary>
    public bool CanBeSecuredDelivery(Entity<CargoOrderConsoleComponent> entity, CargoProductPrototype productId)
    {
        var product = Proto.Index<EntityPrototype>(productId.Product);

        return CanBeSecuredDelivery(entity, product);
    }

    /// <summary>
    /// Проверяет, может ли прототип продукта помещен в защищенный ящик отдела.
    /// </summary>
    public bool CanBeSecuredDelivery(Entity<CargoOrderConsoleComponent> entity, EntityPrototype productProto)
    {
        var access = Proto.Index<CargoAccountPrototype>(entity.Comp.Account).SecureCrateOrderAccess;

        if (productProto.HasComponent<StorageFillComponent>())
        {
            if (productProto.TryGetComponent<AccessReaderComponent>(out var reader)
            && access is not null
            && !AccessReader.AreAccessTagsAllowed(access, reader))
                return false;
        }
        else if (!productProto.HasComponent<ItemComponent>())
            return false;

        return true;
    }

    /// <summary>
    /// Генерирует имя создателя запроса на основе его ID карты или имени напрямую
    /// </summary>
    public string GenerateRequesterName(Entity<CargoOrderConsoleComponent> entity, EntityUid requester)
    {
        string name = string.Empty;

        if (AccessReader.FindAccessItemsInventory(requester, out var items))
            foreach (var item in items)
            {
                if (IdCard.TryGetIdCard(item, out var idCard))
                    name = Loc.GetString("cargo-console-menu-order-requester-format", ("name", idCard.Comp.FullName ?? ""), ("job", idCard.Comp.JobTitle ?? idCard.Comp.LocalizedJobTitle ?? ""));
            }
        else
            name = Identity.Name(requester, EntityManager);

        return name;
    }
    // CorvaxGoob-CargoFeatures-End
}

[NetSerializable, Serializable]
public enum CargoConsoleUiKey : byte
{
    Orders,
    Bounty,
    Shuttle,
    Telepad
}

[NetSerializable, Serializable]
public enum CargoPalletConsoleUiKey : byte
{
    Sale
}

[Serializable, NetSerializable]
public enum CargoTelepadState : byte
{
    Unpowered,
    Idle,
    Teleporting,
};

[Serializable, NetSerializable]
public enum CargoTelepadVisuals : byte
{
    State,
};

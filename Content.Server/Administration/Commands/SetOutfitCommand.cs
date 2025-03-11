using Content.Server.Administration.UI;
using Content.Server.EUI;
using Content.Server.Hands.Systems;
using Content.Server.Preferences.Managers;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Access.Components;
using Content.Shared.Administration;
using Content.Shared.Clothing;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Station;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared._EinsteinEngines.Silicon.IPC; // Goobstation
using Content.Shared.Radio.Components; // Goobstation

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetOutfitCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "setoutfit";

        public string Description => Loc.GetString("set-outfit-command-description", ("requiredComponent", nameof(InventoryComponent)));

        public string Help => Loc.GetString("set-outfit-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 1)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!int.TryParse(args[0], out var entInt))
            {
                shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            var nent = new NetEntity(entInt);

            if (!_entities.TryGetEntity(nent, out var target))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            if (!_entities.HasComponent<InventoryComponent>(target))
            {
                shell.WriteLine(Loc.GetString("shell-target-entity-does-not-have-message", ("missing", "inventory")));
                return;
            }

            if (args.Length == 1)
            {
                if (shell.Player is not { } player)
                {
                    shell.WriteError(Loc.GetString("set-outfit-command-is-not-player-error"));
                    return;
                }

                var eui = IoCManager.Resolve<EuiManager>();
                var ui = new SetOutfitEui(nent);
                eui.OpenEui(ui, player);
                return;
            }

            if (!SetOutfit(target.Value, args[1], _entities))
                shell.WriteLine(Loc.GetString("set-outfit-command-invalid-outfit-id-error"));
        }

        public static bool SetOutfit(EntityUid target, string gear, IEntityManager entityManager, Action<EntityUid, EntityUid>? onEquipped = null)
        {
            if (!entityManager.TryGetComponent(target, out InventoryComponent? inventoryComponent))
                return false;

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            if (!prototypeManager.TryIndex<StartingGearPrototype>(gear, out var startingGear))
                return false;

            HumanoidCharacterProfile? profile = null;
            ICommonSession? session = null;
            // Check if we are setting the outfit of a player to respect the preferences
            if (entityManager.TryGetComponent(target, out ActorComponent? actorComponent))
            {
                session = actorComponent.PlayerSession;
                var userId = actorComponent.PlayerSession.UserId;
                var preferencesManager = IoCManager.Resolve<IServerPreferencesManager>();
                var prefs = preferencesManager.GetPreferences(userId);
                profile = prefs.SelectedCharacter as HumanoidCharacterProfile;
            }

            var invSystem = entityManager.System<InventorySystem>();
            // ShibaStation - Track storage containers (like backpacks) that we equip to fill them later
            Dictionary<string, EntityUid> equippedStorage = new();

            if (invSystem.TryGetSlots(target, out var slots))
            {
                foreach (var slot in slots)
                {
                    invSystem.TryUnequip(target, slot.Name, true, true, false, inventoryComponent);
                    var gearStr = ((IEquipmentLoadout) startingGear).GetGear(slot.Name);
                    if (gearStr == string.Empty)
                    {
                        continue;
                    }

                    var equipmentEntity = entityManager.SpawnEntity(gearStr, entityManager.GetComponent<TransformComponent>(target).Coordinates);
                    if (slot.Name == "id" &&
                        entityManager.TryGetComponent(equipmentEntity, out PdaComponent? pdaComponent) &&
                        entityManager.TryGetComponent<IdCardComponent>(pdaComponent.ContainedId, out var id))
                    {
                        id.FullName = entityManager.GetComponent<MetaDataComponent>(target).EntityName;
                    }

                    invSystem.TryEquip(target, equipmentEntity, slot.Name, silent: true, force: true, inventory: inventoryComponent);
                    onEquipped?.Invoke(target, equipmentEntity);

                    // ShibaStation - Keep track of any storage containers we equip (backpacks, duffels, etc.)
                    if (entityManager.HasComponent<StorageComponent>(equipmentEntity))
                    {
                        equippedStorage[slot.Name] = equipmentEntity;
                    }
                }
            }

            if (entityManager.TryGetComponent(target, out HandsComponent? handsComponent))
            {
                var handsSystem = entityManager.System<HandsSystem>();
                var coords = entityManager.GetComponent<TransformComponent>(target).Coordinates;
                foreach (var prototype in startingGear.Inhand)
                {
                    var inhandEntity = entityManager.SpawnEntity(prototype, coords);
                    handsSystem.TryPickup(target, inhandEntity, checkActionBlocker: false, handsComp: handsComponent);
                }
            }

            // ShibaStation - Fill storage containers (like backpacks) with their specified contents from the starting gear prototype
            if (startingGear.Storage != null)
            {
                var storageSystem = entityManager.System<StorageSystem>();
                foreach (var storageEntry in startingGear.Storage)
                {
                    if (!equippedStorage.TryGetValue(storageEntry.Key, out var storageUid))
                        continue;

                    if (!entityManager.TryGetComponent<StorageComponent>(storageUid, out var storage))
                        continue;

                    var coords = entityManager.GetComponent<TransformComponent>(storageUid).Coordinates;
                    foreach (var itemId in storageEntry.Value)
                    {
                        var item = entityManager.SpawnEntity(itemId, coords);
                        storageSystem.Insert(storageUid, item, out _, user: null, storage);
                    }
                }
            }

            // See if this starting gear is associated with a job
            var jobs = prototypeManager.EnumeratePrototypes<JobPrototype>();
            foreach (var job in jobs)
            {
                if (job.StartingGear != gear)
                    continue;

                var jobProtoId = LoadoutSystem.GetJobPrototype(job.ID);
                if (!prototypeManager.TryIndex<RoleLoadoutPrototype>(jobProtoId, out var jobProto))
                    break;

                // Don't require a player, so this works on Urists
                profile ??= entityManager.TryGetComponent<HumanoidAppearanceComponent>(target, out var comp)
                    ? HumanoidCharacterProfile.DefaultWithSpecies(comp.Species)
                    : new HumanoidCharacterProfile();
                // Try to get the user's existing loadout for the role
                profile.Loadouts.TryGetValue(jobProtoId, out var roleLoadout);

                if (roleLoadout == null)
                {
                    // If they don't have a loadout for the role, make a default one
                    roleLoadout = new RoleLoadout(jobProtoId);
                    roleLoadout.SetDefault(profile, session, prototypeManager);
                }

                // Equip the target with the job loadout
                var stationSpawning = entityManager.System<SharedStationSpawningSystem>();
                stationSpawning.EquipRoleLoadout(target, roleLoadout, jobProto);
            }

            if (entityManager.HasComponent<EncryptionKeyHolderComponent>(target))
            {
                var encryption = entityManager.System<InternalEncryptionKeySpawner>();
                encryption.TryInsertEncryptionKey(target, startingGear);
            }

            return true;
        }
    }
}

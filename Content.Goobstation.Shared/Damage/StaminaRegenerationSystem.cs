using Content.Goobstation.Common.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Goobstation.Shared.Damage;
public sealed class StaminaRegenerationSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaRegenerationComponent, ComponentStartup>(OnStaminaRegenerationStartup);
        SubscribeLocalEvent<StaminaRegenerationComponent, ComponentShutdown>(OnStaminaRegenerationShutdown);
    }

    private void OnStaminaRegenerationStartup(EntityUid uid, StaminaRegenerationComponent component, ComponentStartup args) =>
      _staminaSystem.ToggleStaminaDrain(uid, component.RegenerationRate, true, false, component.RegenerationKey);

    private void OnStaminaRegenerationShutdown(EntityUid uid, StaminaRegenerationComponent component, ComponentShutdown args) =>
      _staminaSystem.ToggleStaminaDrain(uid, component.RegenerationRate, false, false, component.RegenerationKey);

}
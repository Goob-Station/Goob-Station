using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.DeviceLinking;
using System.Linq;

namespace Content.Goobstation.Shared.Factory;

public sealed class AutomationSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;

    private EntityQuery<AutomationSlotsComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<AutomationSlotsComponent>();

        SubscribeLocalEvent<AutomationSlotsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<AutomationSlotsComponent, MapInitEvent>(OnMapInit);
    }

    private void OnInit(Entity<AutomationSlotsComponent> ent, ref ComponentInit args)
    {
        foreach (var slot in ent.Comp.Inputs.Values)
        {
            slot.Initialize();
        }

        foreach (var slot in ent.Comp.Outputs.Values)
        {
            slot.Initialize();
        }
    }

    private void OnMapInit(Entity<AutomationSlotsComponent> ent, ref MapInitEvent args)
    {
        _device.EnsureSinkPorts(ent, ent.Comp.Inputs.Keys.ToArray());
        _device.EnsureSourcePorts(ent, ent.Comp.Outputs.Keys.ToArray());
    }

    #region Public API

    public AutomationSlot? GetSlot(Entity<AutomationSlotsComponent?> ent, string port, bool input)
    {
        if (!_query.Resolve(ref ent, false))
            return null;

        AutomationSlot? slot = null;
        if (input)
            ent.Comp.Inputs.TryGetValue(port, out slot);
        else
            ent.Comp.Outputs.TryGetValue(port, out slot);

        return slot;
    }

    public bool HasSlot(Entity<AutomationSlotsComponent?> ent, string port, bool input)
    {
        return GetSlot(ent, port, input) != null;
    }

    #endregion
}

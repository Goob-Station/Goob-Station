using Content.Shared.Overhaul;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Server.Overhaul;

[RegisterSystem]
public sealed class OverhaulSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OverhaulComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<OverhaulComponent, UseInHandEvent>(OnUse);
    }

    private void OnStartup(EntityUid uid, OverhaulComponent component, ComponentStartup args)
    {
        // Add verb to toggle mode
        var verb = new Verb { Text = "Toggle Overhaul Mode", Act = () => ToggleMode(uid, component) };
        VerbSystem.AddVerb(uid, verb);
    }

    private void ToggleMode(EntityUid uid, OverhaulComponent component)
    {
        component.Mode = component.Mode == "disassemble" ? "reassemble" : "disassemble";
        Dirty(component);
    }

    private void OnUse(EntityUid uid, OverhaulComponent component, UseInHandEvent args)
    {
        if (args.Handled || !TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHandEntity is not EntityUid target)
            return;

        args.Handled = true;

        if (component.Mode == "disassemble")
        {
            _damageable.TryChangeDamage(target, new DamageSpecifier { Brute = component.DisassembleDamage });
        }
        else
        {
            if (TryComp<DamageableComponent>(target, out var damage))
            {
                _damageable.TryChangeDamage(target, new DamageSpecifier { Brute = -component.ReassembleHeal });
            }
        }

        // Cooldown
        _status.TryAddStatusEffect(uid, "Cooldown", component.Cooldown, false);
    }
}

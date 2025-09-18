using Content.Server.Popups;

namespace Content.Goobstation.Server.BloodCult.Runes.Empower;

public sealed class CultRuneEmpowerSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;

    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuneEmpowerComponent, TryInvokeCultRuneEvent>(OnStrengthRuneInvoked);
    }

    private void OnStrengthRuneInvoked(Entity<CultRuneEmpowerComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        var registration = _factory.GetRegistration(ent.Comp.ComponentToGive);
        if (HasComp(args.User, registration.Type))
        {
            _popup.PopupEntity(Loc.GetString("cult-buff-already-buffed"), args.User, args.User);
            args.Cancel();
            return;
        }

        var empowered = (Goobstation.Server.BloodCult.Empower.BloodCultEmpoweredComponent) _factory.GetComponent(ent.Comp.ComponentToGive);
        empowered.TimeRemaining = empowered.DefaultTime;
        AddComp(args.User, empowered);
    }
}

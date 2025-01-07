using Content.Server.Shuttles.Components;
using Content.Shared._Goobstation.Emag;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;

namespace Content.Server.Shuttles.Systems;

public sealed partial class EmergencyShuttleSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedChargesSystem _charge = default!;

    private void InitializeEmag()
    {
        SubscribeLocalEvent<EmergencyShuttleConsoleComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<EmergencyShuttleConsoleComponent, EmergencyShuttleConsoleEmagDoAfterEvent>(OnEmagDoAfter);
    }

    private void OnEmagDoAfter(Entity<EmergencyShuttleConsoleComponent> ent,
        ref EmergencyShuttleConsoleEmagDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!EarlyLaunch())
            return;

        _logger.Add(LogType.Emag,
            LogImpact.High,
            $"{ToPrettyString(args.User):player} emagged shuttle console for early launch");

        EnsureComp<EmaggedComponent>(ent);

        if (args.Used != null)
            _charge.UseCharge(args.Used.Value);
    }

    private void OnEmagged(EntityUid uid, EmergencyShuttleConsoleComponent component, ref GotEmaggedEvent args)
    {
        if (EarlyLaunchAuthorized || !EmergencyShuttleArrived || _consoleAccumulator <= _authorizeTime)
            return;

        args.Handled = false;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            args.UserUid,
            component.EmagTime,
            new EmergencyShuttleConsoleEmagDoAfterEvent(),
            uid,
            uid,
            args.EmagUid)
        {
            DistanceThreshold = 1.5f,
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
}

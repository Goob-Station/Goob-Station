using Robust.Server.GameObjects;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Server.Stack;
using Content.Server.Implants;


namespace Content.Server.Goobstation.BluespaceCrystal;

public sealed partial class BluespaceCrystalSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SubdermalImplantSystem _implantSys = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceCrystalComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, BluespaceCrystalComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):actor} teleported with {ToPrettyString(uid)}");

        _implantSys.TeleportEnt((EntityUid) args.User, component.TeleportRadius, component.TeleportSound);
        var toDel = _stack.Split((EntityUid) uid, 1, Transform(uid).Coordinates);
        QueueDel(toDel);
    }
}
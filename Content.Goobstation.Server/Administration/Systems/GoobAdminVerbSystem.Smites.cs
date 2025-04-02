using Content.Goobstation.Server.Cursed;
using Content.Goobstation.Server.Cursed.Systems;
using Content.Goobstation.Server.DelayedTeleport;
using Content.Goobstation.Shared.BlockSuicide;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem
{
    [Dependency] private readonly AdminHellSystem _adminHellSystem = default!;
    [Dependency] private readonly CursedSystem _cursedSystem = default!;
    [Dependency] private readonly DelayedTeleportSystem _delayedTeleport = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private void AddSmiteVerbs(GetVerbsEvent<Verb> args)
    {
        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        // 1984.
        if (HasComp<MapComponent>(args.Target) || HasComp<MapGridComponent>(args.Target))
            return;

        var helldragName = Loc.GetString("admin-smite-helldrag-name").ToLowerInvariant();
        Verb helldrag = new()
        {
            Text = helldragName,
            Category = VerbCategory.Smite,
            Icon = new SpriteSpecifier.Rsi(new ("/Textures/_Goobstation/Effects/helldrag.rsi"), "icon"),
            Act = () =>
            {
                // Stop them from moving & Suiciding
                _cursedSystem.CurseEntity(args.Target, 4);

                // Spawn animation proto.
                Spawn("PentagramHellHand", Transform(args.Target).Coordinates);
                _popupSystem.PopupEntity(Loc.GetString("helldrag-action", ("target", args.Target)), args.Target, args.Target, PopupType.LargeCaution);

                if (!EntityManager.TryGetComponent(args.Target, out ActorComponent? targetActor))
                    return;

                // Load map, then teleport them to it once the animation is complete.
                var (mapUid, gridUid) = _adminHellSystem.AssertHellLoaded(args.Target, targetActor.PlayerSession.UserId);
                _delayedTeleport.ScheduleTeleport(args.Target, mapUid, gridUid, 4f);

            },
            Impact = LogImpact.Extreme,
            Message = string.Join(": ", helldragName, Loc.GetString("admin-smite-helldrag-description"))
        };
        args.Verbs.Add(helldrag);
    }
}

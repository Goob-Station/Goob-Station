using Robust.Shared.GameStates;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared._Goobstation.Emoting;
using Robust.Shared.Prototypes;
using Content.Shared.Mobs.Systems;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Atmos;
using System.Diagnostics;

namespace Content.Server._Goobstation.Emoting;

public sealed partial class FartSystem : SharedFartSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(EntityUid uid, FartComponent component, ref EmoteEvent args)
    {
        Console.WriteLine($"FartSystem.OnEmote: {args.Emote.ID}");
        if (args.Handled || component.SuperFarted)
            return;

        if (args.Emote.ID == "Fart")
        {
            // Make sure we aren't in timeout
            if (component.FartTimeout)
            {
                _popup.PopupEntity(Loc.GetString("emote-out-of-farts"), uid, uid);
                return;
            }

            // Release ammonia into the air
            args.Handled = true;
            component.FartTimeout = true;
            component.FartInhale = false;

            var tileMix = _atmos.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(Gas.Ammonia, component.MolesAmmoniaPerFart);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
        }
        else if (args.Emote.ID == "FartInhale")
        {
            component.FartInhale = true;
        }
        else if (args.Emote.ID == "FartSuper")
        {
            // Release ammonia into the air
            args.Handled = true;
            component.FartTimeout = true;
            component.FartInhale = false;
            component.SuperFarted = true;

            var tileMix = _atmos.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(Gas.Ammonia, component.MolesAmmoniaPerFart * 2);

            // One minute timeout for ammonia release (60000MS = 60S)
            Timer.Spawn(60000, () =>
            {
                component.FartTimeout = false;
            });
        }
    }
}

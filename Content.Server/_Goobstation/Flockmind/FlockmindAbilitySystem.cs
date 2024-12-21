using Content.Server.Flash;
using Content.Server.Hands.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Flockmind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared.Body.Systems;
using Content.Server.Medical;
using Robust.Server.GameObjects;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Content.Shared.StatusEffect;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Robust.Shared.Audio;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Flockmind;

public sealed partial class FlockmindAbilitySystem : EntitySystem
{
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<FlockmindComponent, EventHereticOpenStore>(OnStore);
    }
}


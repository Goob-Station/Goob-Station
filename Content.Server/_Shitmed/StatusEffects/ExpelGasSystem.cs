// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.StatusEffects;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Robust.Shared.Random;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ExpelGasEffectSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExpelGasComponent, ComponentInit>(OnInit);
    }
    private void OnInit(EntityUid uid, ExpelGasComponent component, ComponentInit args)
    {
        var mix = _atmos.GetContainingMixture((uid, Transform(uid)), true, true) ?? new();
        var gas = _random.Pick(component.PossibleGases);
        mix.AdjustMoles(gas, 60);
        _chat.TrySendInGameICMessage(uid, Loc.GetString(component.Emote), InGameICChatType.Emote, ChatTransmitRange.Normal, ignoreActionBlocker: true);
    }


}

// SPDX-License-Identifier: MIT

using Content.Shared.Effects;
using Robust.Shared.Player;

namespace Content.Server.Effects;

public sealed class ColorFlashEffectSystem : SharedColorFlashEffectSystem
{
    // EE Plasmeme Change
    public override void RaiseEffect(Color color, List<EntityUid> entities, Filter filter, float? animationLength = null)
    {
        RaiseNetworkEvent(new ColorFlashEffectEvent(color, GetNetEntityList(entities), animationLength), filter);
    }
}
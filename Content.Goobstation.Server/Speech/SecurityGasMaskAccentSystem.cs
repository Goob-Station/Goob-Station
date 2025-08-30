// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Speech;
using Content.Server.Speech;
using Content.Shared.Clothing;

namespace Content.Goobstation.Server.Speech;

/// <summary>
/// Applies a robo-hailer style decoration to spoken messages when the Security Gas Mask is equipped.
/// </summary>
public sealed class SecurityGasMaskAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SecurityGasMaskAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<SecurityGasMaskAccentComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<SecurityGasMaskAccentComponent, ClothingGotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, SecurityGasMaskAccentComponent comp, ref ClothingGotEquippedEvent args)
    {
        // When the security gas mask (which this component is on) is equipped, mark the wearer to apply the accent.
        EnsureComp<SecurityGasMaskAccentComponent>(args.Wearer);
    }

    private void OnUnequipped(EntityUid uid, SecurityGasMaskAccentComponent comp, ref ClothingGotUnequippedEvent args)
    {
        // When the mask is unequipped, remove the marker from the wearer.
        RemCompDeferred<SecurityGasMaskAccentComponent>(args.Wearer);
    }

    private void OnAccent(EntityUid uid, SecurityGasMaskAccentComponent comp, AccentGetEvent args)
    {
        // If the entity speaking does not have the accent marker, skip.
        if (!HasComp<SecurityGasMaskAccentComponent>(args.Entity))
            return;

        // Decorate with the desired wrapper.
        // Avoid double-wrapping if already present.
        var msg = args.Message;
        if (!msg.StartsWith("<:: ") && !msg.EndsWith(" ::>"))
            args.Message = $"<:: {msg} ::>";
    }
}

// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Prototypes;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Werewolf.Systems;

public abstract class SharedWerewolfTransformSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    #region Helpers

    public PolymorphConfiguration? GetPolymorphConfig(Entity<WerewolfTransformComponent> ent)
    {
        // Initialize the werewolf form
        if (!_proto.TryIndex(ent.Comp.CurrentWerewolfForm, out var index)
            || index.Configuration.Entity == null)
            return null;

        // Initialize the polymorph
        var polymorphConfig = new PolymorphConfiguration
        {
            Entity = index.Configuration.Entity,
            TransferName = true,
            TransferDamage = false,
            Forced = true,
            Inventory = PolymorphInventoryChange.Drop,
            RevertOnCrit = false,
            RevertOnDeath = true,
            ComponentsToTransfer = new()
            {
                new("Werewolf"),
            },
        };

        return polymorphConfig;
    }

    /// <summary>
    ///  Gets the fur color of the current form the werewolf has selected
    /// </summary>
    /// <param name="ent"></param> The werewolf
    /// <returns></returns>
    public Color GetFurColor(Entity<WerewolfTransformComponent> ent)
    {
        if (!_proto.TryIndex(ent.Comp.CurrentWerewolfForm, out var index))
            return Color.White;

        return index.Configuration.FurColor;
    }

    public WerewolfFormPrototype? GetFormProto(ProtoId<WerewolfFormPrototype> proto)
    {
        if (!_proto.TryIndex(proto, out var index))
            return null;

        return index;
    }

    public SpriteSpecifier? GetSpriteSpecifierForm(ProtoId<WerewolfFormPrototype> proto)
    {
        var form = GetFormProto(proto);
        if (form == null ||
            form.Configuration.Sprite == null)
            return null;

        return form.Configuration.Sprite;
    }

    #endregion
}

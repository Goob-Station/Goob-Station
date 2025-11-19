// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using SpeakOnUIClosedComponent = Content.Shared.Advertise.Components.SpeakOnUIClosedComponent;

namespace Content.Shared.Advertise.Systems;

public abstract class SharedSpeakOnUIClosedSystem : EntitySystem
{
    public bool TrySetFlag(Entity<SpeakOnUIClosedComponent?> entity, bool value = true)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        entity.Comp.Flag = value;
        Dirty(entity);
        return true;
    }
}

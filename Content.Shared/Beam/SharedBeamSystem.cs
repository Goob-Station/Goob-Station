// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Beam;

public abstract class SharedBeamSystem : EntitySystem
{
    public virtual void AccumulateIndex() { }
}
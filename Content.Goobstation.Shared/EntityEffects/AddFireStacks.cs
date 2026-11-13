using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class AddFireStacks : EntityEffectBase<AddFireStacks>
{
    [DataField(required: true)]
    public int Stacks;
};

using JetBrains.Annotations;

namespace Content.Goobstation.Shared.Sandevistan;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class SandevistanEffect
{
    public abstract void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime);
}

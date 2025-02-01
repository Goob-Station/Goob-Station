namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem
{
    private void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseUpdateEvent>(onDamageEffect);
    }
}

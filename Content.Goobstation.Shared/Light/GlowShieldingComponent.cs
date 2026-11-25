namespace Content.Goobstation.Shared.Light
{
    [RegisterComponent]
    public sealed partial class GlowShieldingComponent : Component
    {
        [DataField]
        public float Reduction = 0.1f;
    }
}

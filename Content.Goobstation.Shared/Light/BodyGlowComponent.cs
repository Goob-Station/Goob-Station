namespace Content.Goobstation.Shared.Light
{
    [RegisterComponent]
    public sealed partial class BodyGlowComponent : Component
    {
        [DataField]
        public float BaseRadius = 3f;

        [DataField]
        public float BaseEnergy = 1.5f;

        [ViewVariables]
        public float Factor = 1f;
    }
}

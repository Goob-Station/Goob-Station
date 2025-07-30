namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MindbrokenComponent : Component
    {
        /// <summary>
        ///     The text that will appear when someone with the Mindbroken component is examined at close range
        /// </summary>
        [DataField]
        public string MindbrokenExaminationText = "examine-mindbroken-message";
    }
}

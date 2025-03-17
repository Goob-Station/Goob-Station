﻿using Content.Shared.FixedPoint;

namespace Content.Server.Chemistry.Components
{
    [RegisterComponent]
    public sealed partial class VaporComponent : Component
    {
        public const string SolutionName = "vapor";

        [DataField]
        public FixedPoint2 TransferAmount = FixedPoint2.New(0.5);

        public float ReactTimer;

        [DataField]
        public bool Active;

        [DataField]
        public bool IsFaceReaction = false;
    }
}

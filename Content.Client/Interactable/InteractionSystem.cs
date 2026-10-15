// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;

namespace Content.Client.Interactable
{
    // TODO Remove Shared prefix
    public sealed class InteractionSystem : SharedInteractionSystem
    {
        [Dependency] private readonly TransformSystem _clientTransform = default!;

        protected override bool TryFaceInteraction(EntityUid user, Vector2 coordinates)
        {
            var faced = base.TryFaceInteraction(user, coordinates);

            if (faced)
                _clientTransform.SnapRenderRotation(user);

            return faced;
        }
    }
}
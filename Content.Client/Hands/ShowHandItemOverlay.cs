// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Hands.Systems;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Content.Shared.CCVar;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Direction = Robust.Shared.Maths.Direction;

namespace Content.Client.Hands
{
    public sealed partial class ShowHandItemOverlay : Overlay // Goob
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IClyde _clyde = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!; // Goobstation

        private readonly SpriteSystem _sprite; // Goobstation

        private HandsSystem? _hands;
        private readonly IRenderTexture _renderBackbuffer;

        public override OverlaySpace Space => OverlaySpace.ScreenSpace;

        public Texture? IconOverride;
        public EntityUid? EntityOverride;

        public ShowHandItemOverlay()
        {
            IoCManager.InjectDependencies(this);

            _sprite = _entMan.System<SpriteSystem>(); // Goobstation

            _renderBackbuffer = _clyde.CreateRenderTarget(
                (64, 64),
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb, true),
                new TextureSampleParameters
                {
                    Filter = true
                }, nameof(ShowHandItemOverlay));
        }

        protected override void DisposeBehavior()
        {
            base.DisposeBehavior();

            _renderBackbuffer.Dispose();
        }

        protected override bool BeforeDraw(in OverlayDrawArgs args)
        {
            return base.BeforeDraw(in args); // Goob
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            var mousePos = _inputManager.MouseScreenPosition;

            // Offscreen
            if (mousePos.Window == WindowId.Invalid)
                return;

            var screen = args.ScreenHandle;
            var offset = _cfg.GetCVar(CCVars.HudHeldItemOffset);
            var offsetVec = new Vector2(offset, offset);

            if (IconOverride != null)
            {
                screen.DrawTexture(IconOverride, mousePos.Position - IconOverride.Size / 2 + offsetVec, Color.White.WithAlpha(0.75f));
                return;
            }

            _hands ??= _entMan.System<HandsSystem>();
            var handEntity = EntityOverride ?? (_cfg.GetCVar(CCVars.HudHeldItemShow) ? _hands.GetActiveHandEntity() : null); // Goob

            // Goob edit start
            if (handEntity == null)
                return;

            if (_entMan.TryGetComponent(handEntity.Value, out AmmoSelectorComponent? ammoSelector) &&
                ammoSelector.CurrentlySelected is { } selected)
            {
                var selectedProto = _protoMan.Index(selected);
                var texture = _sprite.Frame0(selectedProto.Icon);
                screen.DrawTexture(texture,
                    mousePos.Position - texture.Size / 2 + offsetVec,
                    Color.White.WithAlpha(0.75f));
                return;
            }

            if (!_entMan.TryGetComponent(handEntity.Value, out SpriteComponent? sprite))
                return;
            // Goob edit end

            var halfSize = _renderBackbuffer.Size / 2;
            var vpControl = args.ViewportControl as Control; // Goob
            var uiScale = vpControl?.UIScale ?? 1f; // Goob

            screen.RenderInRenderTarget(_renderBackbuffer, () =>
            {
                screen.DrawEntity(handEntity.Value, halfSize, new Vector2(1f, 1f) * uiScale, Angle.Zero, Angle.Zero, Direction.South, sprite);
            }, Color.Transparent);

            screen.DrawTexture(_renderBackbuffer.Texture, mousePos.Position - halfSize + offsetVec, Color.White.WithAlpha(0.75f));

            DrawCooldown(args, handEntity.Value, mousePos.Position, vpControl); // Goob
        }
    }
}

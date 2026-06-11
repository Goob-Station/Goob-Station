using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics.RSI;

namespace Content.Goobstation.Client.JoinQueue;

// I wanted a partial class because the first file was getting a little too big for my liking, but I couldn't think of a good name so now we have QueueCritterWalk.Critter. Gem.

public sealed partial class QueueCritterWalk
{
    private sealed class Critter
    {
        public string Name = string.Empty;
        public bool IsLocalPlayer;
        public float XPosition;
        public float TargetX;
        public float Speed;
        public bool Leaving;
        public bool IsMoving;
        public float IdleTimer;
        public int QueueIndex;
        public RsiDirection FacingDirection = RsiDirection.South;

        public float ZoneStart;
        public float ZoneEnd;

        public IRsiStateLike IdleState = default!;
        public IRsiStateLike? MovingState;
        public int CurFrame;
        public float CurFrameTime;

        public TextureRect SpriteRect = default!;
        public Label NameLabel = default!;
        public BoxContainer Box = default!;

        public static IRsiStateLike GetActiveState(IRsiStateLike idle, IRsiStateLike? moving, bool isMoving)
        {
            return isMoving && moving != null ? moving : idle;
        }

        public static RsiDirection GetRsiDirection(int moveSign, IRsiStateLike state)
        {
            if (state.RsiDirections == RsiDirectionType.Dir1)
                return RsiDirection.South;

            return moveSign switch
            {
                > 0 => RsiDirection.East,
                < 0 => RsiDirection.West,
                _ => RsiDirection.South,
            };
        }

        public void UpdateSpriteFrame()
        {
            var activeState = GetActiveState(IdleState, MovingState, IsMoving);
            SpriteRect.Texture = activeState.GetFrame(FacingDirection, CurFrame);
        }


        public void SetMoving(bool moving)
        {
            if (IsMoving == moving)
                return;

            IsMoving = moving;
            CurFrame = 0;
            var activeState = GetActiveState(IdleState, MovingState, moving);
            CurFrameTime = activeState.GetDelay(0);
            UpdateSpriteFrame();
        }

        public void AdvanceAnimation(float deltaSeconds, bool wasMoving)
        {
            var state = GetActiveState(IdleState, MovingState, IsMoving);
            if (!state.IsAnimated)
                return;

            CurFrameTime -= deltaSeconds;
            var oldFrame = CurFrame;

            while (CurFrameTime <= 0f)
            {
                CurFrame = (CurFrame + 1) % state.AnimationFrameCount;
                CurFrameTime += state.GetDelay(CurFrame);
            }

            if (CurFrame != oldFrame || wasMoving != IsMoving)
                UpdateSpriteFrame();
        }
    }
}

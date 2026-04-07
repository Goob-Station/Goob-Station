using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.JoinQueue;

public sealed partial class QueueCritterWalk : LayoutContainer
{
    /// <summary>
    ///     Sprite pool entries. IdleState is shown when standing still, MovingState when walking.
    ///     If MovingState is null, the same sprite is used for both.
    /// </summary>
    private static readonly (string Path, string IdleState, string? MovingState)[] SpritePool =
    [
        ("Mobs/Animals/mothroach/mothroach.rsi", "mothroach", "mothroach-moving"),
        ("_Goobstation/Mobs/Animals/mustard_mothroach.rsi", "mothroach", "mothroach-moving"),
        ("_Goobstation/Mobs/Animals/leopard_mothroach.rsi", "mothroach", "mothroach-moving"),
        ("_Goobstation/Mobs/Animals/cecropia_mothroach.rsi", "mothroach", "mothroach-moving"),
        ("Mobs/Silicon/Bots/cleanbot.rsi", "cleanbot", null),
        ("Mobs/Aliens/slimes.rsi", "blue_adult_slime", null),
        ("Mobs/Aliens/slimes.rsi", "green_adult_slime", null),
        ("Mobs/Aliens/slimes.rsi", "yellow_adult_slime", null),
        ("Mobs/Silicon/Bots/mommi.rsi", "wiggle", null),
        ("Mobs/Animals/hamster.rsi", "hamster-0", "hamster-moving-0"),
        ("Mobs/Animals/mouse.rsi", "mouse-0", "mouse-moving-0"),
        ("Mobs/Animals/mouse.rsi", "mouse-1", "mouse-moving-1"),
        ("Mobs/Animals/mouse.rsi", "mouse-2", "mouse-moving-2"),
        ("Mobs/Pets/cat.rsi", "cat", null),
        ("Mobs/Pets/cat.rsi", "cat2", null),
        ("_Goobstation/Mobs/Bingle/bingle.rsi", "alive", null),
    ];

    private const float SpriteScale = 3f;
    private const float SpriteDisplaySize = 32f * SpriteScale;
    private const float ControlHeight = SpriteDisplaySize + 20f;
    private const float MinSpeed = 40f;
    private const float MaxSpeed = 60f;
    private const float IdleRetargetMinSeconds = 2f;
    private const float IdleRetargetMaxSeconds = 5f;
    private const float OffScreenSpawn = -SpriteDisplaySize;

    private readonly Dictionary<string, Critter> _critters = [];
    private readonly IRobustRandom _random;
    private readonly SpriteSystem _spriteSystem;

    private int _totalPlayers;

    public QueueCritterWalk()
    {
        _random = IoCManager.Resolve<IRobustRandom>();
        _spriteSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();
        InheritChildMeasure = false;
        HorizontalExpand = true;
        MinHeight = ControlHeight;
        RectClipContent = true;
    }

    public void UpdateCritters(List<string> playerNames, string yourName)
    {
        _totalPlayers = playerNames.Count;

        var currentNames = new HashSet<string>(playerNames);

        foreach (var (name, critter) in _critters)
        {
            if (!currentNames.Contains(name) && !critter.Leaving)
            {
                critter.Leaving = true;
                critter.TargetX = Width + SpriteDisplaySize;
            }
        }

        for (var i = 0; i < playerNames.Count; i++)
        {
            var name = playerNames[i];

            if (_critters.TryGetValue(name, out var existing))
            {
                existing.QueueIndex = i;

                var isLocal = name == yourName;
                if (existing.IsLocalPlayer != isLocal)
                {
                    existing.IsLocalPlayer = isLocal;
                    UpdateNameLabelStyle(existing);
                }

                UpdateZone(existing);
                continue;
            }

            var critter = CreateCritter(name, name == yourName, i);
            _critters[name] = critter;
        }
    }

    private void UpdateZone(Critter critter)
    {
        if (_totalPlayers <= 0 || critter.Leaving)
            return;

        var usableWidth = Math.Max(0f, Width - SpriteDisplaySize);
        var reversedIndex = _totalPlayers - 1 - critter.QueueIndex;
        critter.ZoneStart = (float) reversedIndex / _totalPlayers * usableWidth;
        critter.ZoneEnd = (float) (reversedIndex + 1) / _totalPlayers * usableWidth;

        if (critter.TargetX < critter.ZoneStart || critter.TargetX > critter.ZoneEnd)
            critter.TargetX = _random.NextFloat(critter.ZoneStart, critter.ZoneEnd);
    }

    private Critter CreateCritter(string name, bool isLocalPlayer, int queueIndex)
    {
        var spriteIndex = (int) (StableHash(name) % (uint) SpritePool.Length);
        var (rsiPath, idleStateName, movingStateName) = SpritePool[spriteIndex];

        var idleSpec = new SpriteSpecifier.Rsi(new ResPath(rsiPath), idleStateName);
        var idleState = _spriteSystem.RsiStateLike(idleSpec);

        IRsiStateLike? movingState = null;
        if (movingStateName != null)
        {
            var movingSpec = new SpriteSpecifier.Rsi(new ResPath(rsiPath), movingStateName);
            movingState = _spriteSystem.RsiStateLike(movingSpec);
        }

        var spriteRect = new TextureRect
        {
            TextureScale = new Vector2(SpriteScale, SpriteScale),
            Stretch = TextureRect.StretchMode.KeepCentered,
            HorizontalAlignment = HAlignment.Center,
            MinSize = new Vector2(SpriteDisplaySize, SpriteDisplaySize),
            Texture = idleState.GetFrame(RsiDirection.South, 0),
        };

        var nameLabel = new Label
        {
            Text = name,
            HorizontalAlignment = HAlignment.Center,
            Align = Label.AlignMode.Center,
        };

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
        };
        box.AddChild(spriteRect);
        box.AddChild(nameLabel);

        AddChild(box);

        var usableWidth = Math.Max(0f, Width - SpriteDisplaySize);
        var reversedIndex = _totalPlayers > 0 ? _totalPlayers - 1 - queueIndex : 0;
        var zoneStart = _totalPlayers > 0 ? (float) reversedIndex / _totalPlayers * usableWidth : 0f;
        var zoneEnd = _totalPlayers > 0 ? (float) (reversedIndex + 1) / _totalPlayers * usableWidth : usableWidth;

        var critter = new Critter // I hate making monolithic objects but whatever, this is private and contained so it's not the worst.
        {
            Name = name,
            IsLocalPlayer = isLocalPlayer,
            XPosition = OffScreenSpawn,
            TargetX = _random.NextFloat(zoneStart, Math.Max(zoneStart, zoneEnd)),
            Speed = _random.NextFloat(MinSpeed, MaxSpeed),
            Leaving = false,
            IsMoving = true,
            IdleTimer = _random.NextFloat(IdleRetargetMinSeconds, IdleRetargetMaxSeconds),
            QueueIndex = queueIndex,
            ZoneStart = zoneStart,
            ZoneEnd = zoneEnd,
            FacingDirection = RsiDirection.East,
            IdleState = idleState,
            MovingState = movingState,
            CurFrame = 0,
            CurFrameTime = Critter.GetActiveState(idleState, movingState, true).GetDelay(0),
            SpriteRect = spriteRect,
            NameLabel = nameLabel,
            Box = box,
        };

        UpdateNameLabelStyle(critter);
        SetPosition(box, new Vector2(OffScreenSpawn, 0));

        return critter;
    }

    private static void UpdateNameLabelStyle(Critter critter)
    {
        critter.NameLabel.StyleClasses.Clear();
        critter.NameLabel.StyleClasses.Add(critter.IsLocalPlayer ? "LabelKeyText" : "LabelSubText");
    }

    // Random method I pulled off of stack overflow to get a decent hash from string cuz the default dotnet one is random per session.
    private static uint StableHash(string s)
    {
        var hash = 2166136261u;
        foreach (var c in s)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!VisibleInTree)
            return;

        var toRemove = new List<string>();

        var usableWidth = Math.Max(0f, Width - SpriteDisplaySize);

        foreach (var (name, critter) in _critters)
        {
            if (!critter.Leaving && _totalPlayers > 0)
            {
                var reversedIndex = _totalPlayers - 1 - critter.QueueIndex;
                critter.ZoneStart = (float) reversedIndex / _totalPlayers * usableWidth;
                critter.ZoneEnd = (float) (reversedIndex + 1) / _totalPlayers * usableWidth;
            }

            var dx = critter.TargetX - critter.XPosition;
            var wasMoving = critter.IsMoving;

            if (Math.Abs(dx) > 1f)
            {
                var direction = Math.Sign(dx);
                var step = critter.Speed * args.DeltaSeconds;
                critter.XPosition += direction * Math.Min(step, Math.Abs(dx));

                var activeState = Critter.GetActiveState(critter.IdleState, critter.MovingState, true);
                critter.FacingDirection = Critter.GetRsiDirection(direction, activeState);

                critter.SetMoving(true);
            }
            else if (!critter.Leaving)
            {
                critter.SetMoving(false);

                critter.IdleTimer -= args.DeltaSeconds;
                if (critter.IdleTimer <= 0f)
                {
                    critter.TargetX = _random.NextFloat(critter.ZoneStart, Math.Max(critter.ZoneStart, critter.ZoneEnd));
                    critter.IdleTimer = _random.NextFloat(IdleRetargetMinSeconds, IdleRetargetMaxSeconds);
                }
            }

            critter.AdvanceAnimation(args.DeltaSeconds, wasMoving);

            var boxHeight = critter.Box.DesiredSize.Y;
            var yPos = Height - boxHeight;
            SetPosition(critter.Box, new Vector2(critter.XPosition, yPos));

            if (critter.Leaving && critter.XPosition >= Width + SpriteDisplaySize - 1f)
                toRemove.Add(name);
        }

        foreach (var name in toRemove)
            if (_critters.Remove(name, out var critter))
                RemoveChild(critter.Box);
    }
}

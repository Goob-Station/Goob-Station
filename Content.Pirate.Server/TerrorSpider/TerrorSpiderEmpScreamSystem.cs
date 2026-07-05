using System.Numerics;
using Content.Pirate.Shared.TerrorSpider;
using Content.Server.Emp;
using Content.Shared.Camera;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Pirate.Server.TerrorSpider;

public sealed class TerrorSpiderEmpScreamSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EMPScreamEvent>(OnEmpScream);
    }

    private void OnEmpScream(EMPScreamEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var performer = args.Performer;
        ScreamEffect(performer, args.Power, args.ScreamSound);

        _emp.EmpPulse(_transform.GetMapCoordinates(performer),
            args.Power,
            args.EnergyConsumption,
            args.DurationMultiply * args.Power);
    }

    private void ScreamEffect(EntityUid source, float screamPower, SoundSpecifier? sound)
    {
        if (sound != null)
            _audio.PlayPvs(sound, source);

        var center = _transform.GetMapCoordinates(source);
        var recipients = Filter.Empty();
        recipients.AddInRange(center, screamPower, _player, EntityManager);

        foreach (var recipient in recipients.Recipients)
        {
            if (recipient.AttachedEntity == null)
                continue;

            var position = _transform.GetWorldPosition(recipient.AttachedEntity.Value);
            var delta = center.Position - position;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new Vector2(.01f, 0);

            _recoil.KickCamera(source, -delta.Normalized());
        }
    }
}

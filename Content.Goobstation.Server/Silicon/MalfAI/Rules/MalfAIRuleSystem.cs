using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;

namespace Content.Goobstation.Server.Silicon.MalfAI.Rules;

public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAIRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAIRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    private void OnSelectAntag(Entity<MalfAIRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var malfComp = EnsureComp<MalfStationAIComponent>(args.EntityUid);
    }
}
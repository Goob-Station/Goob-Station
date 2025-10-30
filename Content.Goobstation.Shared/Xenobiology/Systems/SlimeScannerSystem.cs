using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using System.Text;

namespace Content.Goobstation.Shared.Xenobiology.Systems;
public sealed partial class SlimeScannerSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
    }

    private void OnAfterInteractUsing(EntityUid uid, SlimeComponent slime, AfterInteractUsingEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach || _net.IsClient || !HasComp<SlimeScannerComponent>(args.Used))
            return;

        var markup = FormattedMessage.FromMarkupOrThrow(GenerateMarkup(args.Target.Value, slime));
        _examineSystem.SendExamineTooltip(args.User, uid, markup, false, true);
        args.Handled = true;
    }

    private string GenerateMarkup(EntityUid uid, SlimeComponent comp)
    {
        var mutationChance = MathF.Floor(comp.MutationChance * 100f);

        var sb = new StringBuilder();

        sb.AppendLine(Loc.GetString("slime-scanner-examine-description", ("color", comp.SlimeColor.ToHex()), ("name", _prot.Index(comp.Breed).BreedName)));

        // all this shit for a good looking examine text. imagine.
        sb.Append($"{Loc.GetString("slime-scanner-examine-mutations", ("chance", mutationChance))} ");
        var mutations = comp.PotentialMutations.ToList();
        for (int i = 0; i < mutations.Count; i++)
        {
            var info = _prot.Index(mutations[i]);

            var color = "white";
            // todo make it actually work
            if (info.Components.TryGetComponent(nameof(SlimeComponent), out var sc))
                color = ((SlimeComponent) sc!).SlimeColor.ToHex();

            sb.Append($"[color={color}]{info.BreedName}[/color]");

            if (i == mutations.Count - 1) sb.AppendLine(".");
            else sb.Append(", ");
        }

        sb.AppendLine(Loc.GetString("slime-scanner-examine-extracts", ("num", comp.ExtractsProduced)));

        return sb.ToString();
    }
}

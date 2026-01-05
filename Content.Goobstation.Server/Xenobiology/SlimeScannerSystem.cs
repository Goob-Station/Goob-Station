using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using System.Text;

namespace Content.Goobstation.Server.Xenobiology;
public sealed partial class SlimeScannerSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, AfterInteractUsingEvent>(OnSlimeAfterInteractUsing);
        SubscribeLocalEvent<SlimeExtractComponent, AfterInteractUsingEvent>(OnExtractAfterInteractUsing);
    }

    private void OnSlimeAfterInteractUsing(Entity<SlimeComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!CanSendTooltip(args))
            return;

        TrySendTooltip(args.User, ent, GenerateSlimeMarkup(ent));
        args.Handled = true;
    }

    private void OnExtractAfterInteractUsing(Entity<SlimeExtractComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!CanSendTooltip(args))
            return;

        var loc = Loc.GetString("slime-scanner-examine-extract", ("reagents", GenerateExtractMarkup(ent)));
        TrySendTooltip(args.User, ent, loc);
        args.Handled = true;
    }

    private bool CanSendTooltip(AfterInteractUsingEvent args)
        => !args.Handled && args.Target != null && args.CanReach && HasComp<SlimeScannerComponent>(args.Used);

    private void TrySendTooltip(EntityUid player, EntityUid target, string message)
    {
        var markup = FormattedMessage.FromMarkupOrThrow(message);
        _examineSystem.SendExamineTooltip(player, target, markup, false, true);
    }

    private string GenerateSlimeMarkup(Entity<SlimeComponent> ent)
    {
        var mutationChance = MathF.Floor(ent.Comp.MutationChance * 100f);

        var sb = new StringBuilder();

        sb.AppendLine(Loc.GetString("slime-scanner-examine-slime-description", ("color", ent.Comp.SlimeColor.ToHex()), ("name", _prot.Index(ent.Comp.Breed).BreedName)));

        // all this shit for a good looking examine text. imagine.
        sb.Append($"{Loc.GetString("slime-scanner-examine-slime-mutations", ("chance", mutationChance))} ");
        var mutations = ent.Comp.PotentialMutations.ToList();
        for (int i = 0; i < mutations.Count; i++)
        {
            var info = _prot.Index(mutations[i]);

            var color = "white";
            // todo make the colors work
            if (info.Components.TryGetComponent(nameof(SlimeComponent), out var sc))
                color = ((SlimeComponent) sc!).SlimeColor.ToHex();

            sb.Append($"[color={color}]{info.BreedName}[/color]");

            if (i == mutations.Count - 1) sb.AppendLine(".");
            else sb.Append(", ");
        }

        sb.AppendLine(Loc.GetString("slime-scanner-examine-slime-extracts", ("num", ent.Comp.ExtractsProduced)));

        return sb.ToString();
    }

    private string GenerateExtractMarkup(Entity<SlimeExtractComponent> ent)
    {
        var sb = new StringBuilder();

        if (!TryComp<ReactiveComponent>(ent, out var reactive) || reactive.Reactions == null)
        {
            sb.AppendLine(Loc.GetString("slime-scanner-examine-extract-unreactive"));
            return sb.ToString();
        }

        var reactions = reactive.Reactions;
        for (int i = 0; i < reactions.Count; i++)
        {
            var item = reactions[i];
            if (item.Reagents == null)
                continue;

            var reagents = item.Reagents.ToList();
            for (int j = 0; j < reagents.Count; j++)
            {
                var reagent = reagents[j];
                if (!_prot.TryIndex<ReagentPrototype>(reagent, out var rid))
                    continue;

                sb.Append($"[color={rid.SubstanceColor.ToHex()}]{rid.ID.ToLower()}[/color]");

                if (reagents.Count <= 1)
                    continue;

                // jic
                if (i == reagents.Count - 1) sb.Append("; ");
                else sb.Append(", ");
            }

            if (i == reactions.Count - 1) sb.AppendLine(".");
            else sb.Append(", ");
        }

        return sb.ToString();
    }
}

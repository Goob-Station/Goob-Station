using Content.Server.Materials;
using Content.Server.Stack;
using Content.Shared._Pirate.Paper;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Paper;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._Pirate.Paper;

public sealed class DocumentPrinterPaperInputSystem : EntitySystem
{
    private const string PaperStackType = "Paper";
    private const string PrinterPaperStackType = "SheetPrinter";
    private static readonly ProtoId<StackPrototype> PrinterPaperStack = "SheetPrinter";

    [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DocumentPrinterPaperInputComponent, InteractUsingEvent>(
            OnInteractUsing,
            before: [typeof(SharedMaterialStorageSystem)]);
    }

    private void OnInteractUsing(Entity<DocumentPrinterPaperInputComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        TryComp<StackComponent>(args.Used, out var usedStack);

        // Let blank printer-paper stacks follow the normal material insertion path,
        // but stop written or stamped sheets from being recycled back into stock.
        if (usedStack?.StackTypeId == PrinterPaperStackType)
        {
            if (TryComp<PaperComponent>(args.Used, out var printerPaper) &&
                !IsBlankPaper(printerPaper))
            {
                args.Handled = true;
            }

            return;
        }

        if (usedStack?.StackTypeId == PaperStackType)
        {
            InsertConvertedPaper(ent, args, usedStack.Count);
            return;
        }

        if (!TryComp<PaperComponent>(args.Used, out var paper) ||
            !IsBlankPaper(paper))
        {
            return;
        }

        InsertConvertedPaper(ent, args, 1);
    }

    private void InsertConvertedPaper(Entity<DocumentPrinterPaperInputComponent> ent, InteractUsingEvent args, int amount)
    {
        var printerPaper = _stack.SpawnAtPosition(amount, PrinterPaperStack, Transform(ent).Coordinates);
        if (!_materialStorage.TryInsertMaterialEntity(args.User, printerPaper, ent))
        {
            QueueDel(printerPaper);
            return;
        }

        args.Handled = true;
        QueueDel(args.Used);
    }

    private static bool IsBlankPaper(PaperComponent paper)
    {
        return paper.Content == string.Empty &&
               paper.StampedBy.Count == 0 &&
               paper.StampState == null;
    }
}

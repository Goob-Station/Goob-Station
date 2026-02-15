using System.Globalization;
using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

[TypeSerializer]
public sealed class MegafaunaNumberSelectorTypeSerializer :
    ITypeReader<MegafaunaNumberSelector, ValueDataNode>
{
    public ValidationNode Validate(ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        // ConstantMegafaunaNumberSelector validation
        if (float.TryParse(node.Value, out _))
            return new ValidatedValueNode(node);

        // RangeMegafaunaNumberSelector validation
        if (VectorSerializerUtility.TryParseArgs(node.Value, 2, out _))
        {
            return new ValidatedValueNode(node);
        }

        return new ErrorNode(node, "Custom validation not supported! Please specify the type manually!");
    }

    public MegafaunaNumberSelector Read(ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<MegafaunaNumberSelector>? instanceProvider = null)
    {
        var type = typeof(MegafaunaNumberSelector);

        if (float.TryParse(node.Value, CultureInfo.InvariantCulture, out var result))
            return new MegafaunaConstantNumberSelector(result);

        if (VectorSerializerUtility.TryParseArgs(node.Value, 2, out var args))
        {
            var x = float.Parse(args[0], CultureInfo.InvariantCulture);
            var y = float.Parse(args[1], CultureInfo.InvariantCulture);
            return new MegafaunaRangeNumberSelector(new Vector2(x, y));
        }

        throw new InvalidOperationException($"Cannot parse MegafaunaNumberSelector from value: {node.Value}");
    }
}

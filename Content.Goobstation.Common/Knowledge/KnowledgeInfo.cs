using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Knowledge;

[Serializable, NetSerializable]
public record struct KnowledgeInfo(string Name, string Description, Color Color, SpriteSpecifier? Sprite);

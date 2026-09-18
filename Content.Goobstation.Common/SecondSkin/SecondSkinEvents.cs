namespace Content.Goobstation.Common.SecondSkin;

[ByRefEvent]
public record struct GetSecondSkinDeductionEvent(int Coverage, string TraumaType, float Deduction = 0f);

[ByRefEvent]
public record struct ModifyDisgustEvent(float Delta);

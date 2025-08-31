using System;
using System.ComponentModel;
using System.Reflection;

public enum MarkerBuoyType
{
    [Description("Unknown")]
    Unknown = 0,

    [Description("Cardinal Mark - North")]
    CardinalMarkNorth = 1,

    [Description("Cardinal Mark - South")]
    CardinalMarkSouth = 2,

    [Description("Cardinal Mark - East")]
    CardinalMarkEast = 3,

    [Description("Cardinal Mark - West")]
    CardinalMarkWest = 4,

    [Description("Emergency Wreck Mark")]
    EmergencyWreckMark = 5,

    [Description("Isolated Danger Mark")]
    IsolatedDangerMark = 6,

    [Description("Port Mark")]
    PortMark = 7,

    [Description("Safe Water Mark")]
    SafeWaterMark = 8,

    [Description("Special Mark")]
    SpecialMark = 9,

    [Description("Starboard Mark")]
    StarboardMark = 10
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }
}

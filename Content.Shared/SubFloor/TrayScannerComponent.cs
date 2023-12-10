using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.SubFloor;

public enum TrayScannerFilter
{
    ALL,
    GAS_PIPES,
    CARGO_PIPES,
    ALL_WIRES,
    LV,
    MV,
    HV,
}

[RegisterComponent, NetworkedComponent]
public sealed partial class TrayScannerComponent : Component
{
    /// <summary>
    ///     Whether the scanner is currently on.
    /// </summary>
    [ViewVariables, DataField("enabled")] public bool Enabled;

    [ViewVariables, DataField("filter")] public TrayScannerFilter Filter = TrayScannerFilter.ALL;

    /// <summary>
    ///     Radius in which the scanner will reveal entities. Centered on the <see cref="LastLocation"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 4f;
}

[Serializable, NetSerializable]
public sealed class TrayScannerState : ComponentState
{
    public bool Enabled;

    public TrayScannerFilter Filter;

    public float Range;

    public TrayScannerState(bool enabled, )
    {
        Enabled = enabled;
        Filter = filter;
        Range = range;
    }
}

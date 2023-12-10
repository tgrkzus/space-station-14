using Content.Shared.Interaction;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Verbs;

namespace Content.Shared.SubFloor;

public abstract class SharedTrayScannerSystem : EntitySystem
{
    [Dependency] private readonly GameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public const float SubfloorRevealAlpha = 0.8f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrayScannerComponent, ComponentGetState>(OnTrayScannerGetState);
        SubscribeLocalEvent<TrayScannerComponent, ComponentHandleState>(OnTrayScannerHandleState);
        SubscribeLocalEvent<TrayScannerComponent, ActivateInWorldEvent>(OnTrayScannerActivate);
        SubscribeLocalEvent<TrayScannerComponent, GetVerbsEvent<AlternativeVerb>>(RotateFilter);
    }

    private void OnTrayScannerActivate(EntityUid uid, TrayScannerComponent scanner, ActivateInWorldEvent args)
    {
        SetScannerEnabled(uid, !scanner.Enabled, scanner);
    }

    private void SetScannerEnabled(EntityUid uid, bool enabled, TrayScannerComponent? scanner = null)
    {
        if (!Resolve(uid, ref scanner) || scanner.Enabled == enabled)
            return;

        scanner.Enabled = enabled;
        Dirty(scanner);

        // We don't remove from _activeScanners on disabled, because the update function will handle that, as well as
        // managing the revealed subfloor entities

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, TrayScannerVisual.Visual, scanner.Enabled ? TrayScannerVisual.On : TrayScannerVisual.Off, appearance);
        }
    }


    private void RotateFilter(EntityUid uid, TrayScannerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;
        // if (!_timing.IsFirstTimePredicted)
        //     return;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                var nextFilter = TrayScannerFilter.ALL;
                switch (component.Filter)
                {
                    case TrayScannerFilter.ALL:
                        nextFilter = TrayScannerFilter.GAS_PIPES;
                        break;
                    case TrayScannerFilter.GAS_PIPES:
                        nextFilter = TrayScannerFilter.CARGO_PIPES;
                        break;
                    case TrayScannerFilter.CARGO_PIPES:
                        nextFilter = TrayScannerFilter.ALL_WIRES;
                        break;
                    case TrayScannerFilter.ALL_WIRES:
                        nextFilter = TrayScannerFilter.LV;
                        break;
                    case TrayScannerFilter.LV:
                        nextFilter = TrayScannerFilter.MV;
                        break;
                    case TrayScannerFilter.MV:
                        nextFilter = TrayScannerFilter.HV;
                        break;
                    case TrayScannerFilter.HV:
                        nextFilter = TrayScannerFilter.ALL;
                        break;
                }
                SetFilter(uid, nextFilter, component);
            },
            // TODO
            Text = Loc.GetString("action-name-wake"),
            Priority = 1
        };
        args.Verbs.Add(verb);

    }

    private void SetFilter(EntityUid uid, TrayScannerFilter filter, TrayScannerComponent? scanner = null)
    {
        if (!Resolve(uid, ref scanner) || scanner.Filter == filter)
            return;
        scanner.Filter = filter;
        Dirty(uid, scanner);
    }

    private void OnTrayScannerGetState(EntityUid uid, TrayScannerComponent scanner, ref ComponentGetState args)
    {
        args.State = new TrayScannerState(scanner.Enabled, scanner.Filter);
    }

    private void OnTrayScannerHandleState(EntityUid uid, TrayScannerComponent scanner, ref ComponentHandleState args)
    {
        if (args.Current is not TrayScannerState state)
            return;

        SetScannerEnabled(uid, state.Enabled, scanner);
    }
}

[Serializable, NetSerializable]
public enum TrayScannerVisual : sbyte
{
    Visual,
    On,
    Off
}

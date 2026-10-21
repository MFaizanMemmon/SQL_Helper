# Desktop layout guide

All 14 application forms share the design system in `AppUi.cs`. Designer controls are reused so their event handlers and data bindings remain attached. The dashboard retains its explorer layout and uses the same colors, typography, button styles, and grid styles.

- Connection, comparison, search, preview, script, sample-data, X-Ray, and parameter windows resize freely.
- Field and action groups wrap into fewer columns as the viewport narrows.
- Database navigation moves above results in compact windows.
- Long forms and generated column inputs can scroll; grids and SQL editors keep their own scrolling.
- Dialogs fit the current monitor working area when opened.
- PerMonitorV2 DPI awareness is enabled in the project configuration.
- Parameter fields stretch with the dialog. Date parameters now return an invariant date/time value.

## Verification

All 14 forms were instantiated and rendered at 1200x800, 800x600, 420x640, 380x480, and 1366x768. The 70 layout cases completed without exceptions or controls extending outside non-scrollable containers. Screenshots were visually inspected during iteration. Database load handlers were suppressed during these layout checks; SQL operations were not exercised.

Actual movement between monitors with different scaling factors still needs device verification. The functionality completion pass added multi-result previews, sample-data configuration, script save/copy controls, and parameter null/default controls. The current build has zero warnings; database verification is documented in FUNCTIONALITY_AUDIT.md.

## Extending the UI

Call `AppUi.Apply(this)` after `InitializeComponent()` for a supported form. Add a form-specific layout in its switch using existing named controls. Keep business logic in the form. Use `MakeField` for dynamically generated parameter fields and `StyleTree` for controls created after construction. Preserve input names and control collection structure when business logic enumerates controls.

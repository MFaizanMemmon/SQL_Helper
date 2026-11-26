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


## Modern workspace refresh (feature branch)

The existing 14-form layout is retained and the new Live Activity Monitor uses the same design system. The refresh uses a navy workspace header, slate canvas, white surfaces, blue primary actions, a keyboard-accessible owner-drawn database list, updated data grids, modern tab headers, and consistent input/editor styling. Live SQL operations are colored by type while retaining the text labels. Dynamically created preview tabs use the shared theme as well.

No new UI package or target framework change is required. The existing control names, event handlers, SQL services and resize/reflow logic are retained. This is a light workspace theme with navy accents, not a light/dark toggle.

The 70 layout cases and zero-warning build recorded above are from the earlier baseline, not this refresh. The new design has not yet been built or visually verified on Windows. Before merging, build on Windows with the .NET 8 SDK, repeat the existing verification suite, and check the dashboard, every dialog, editable sample-data grid, dynamic result tabs and monitor at 1366x768, 800x600 and compact sizes with 100%/150% display scaling. Confirm keyboard focus and navigation, long database names, long SQL statements, disabled actions, scrolling and data bindings.


## Dark console theme (supersedes the light workspace palette)

The shared design now uses a near-black canvas, charcoal surfaces, emerald actions and headings, muted green-gray text, terminal-inspired headers, monospace SQL editors and database navigation, dark grids, tabs and context menus. The monitor uses bright green for inserts, amber for updates and rose for deletes/errors; operation names remain visible. This is a fixed dark theme, not a theme switcher. Native Windows title bars, scrollbars, date-picker chrome and system message boxes may retain OS styling.

Only presentation code changed in this pass; SQL capture and database operations remain unchanged. Windows compilation and visual checks are still pending. Check all dialogs for text contrast, focus, disabled inputs, native controls and editable grid cells before merging.

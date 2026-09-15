RASSEF — SIDEBAR UNIFICATION: WHAT WAS DONE THIS SESSION
==========================================================
Continuation of Part B (unified permission-aware sidebar) from the original
onboarding brief. If you're picking this up after me, read this before
touching anything sidebar-related — it tells you exactly what changed, why,
and what's still open.

STARTING STATE (confirmed by reading the actual repo, not assumed):
- AdminSidebarViewComponent + its Default.cshtml view already existed and
  were correct — permission-gated via IUserPermissionService, accordion
  categories already built, warehouse dropdown already wired to real data.
- BUT: 0 pages actually called it. 32 pages still called the old
  `<partial name="_AdminSidebar" model='"X"' />`, and the screenshots that
  looked "still old/flat" were correct — that page really was still on the
  old partial.
- A second, separate, legitimately-designed component called UserSidebar
  existed (own doc-comment confirmed it was intentional, not a stray/failed
  attempt) — a lighter "greeting + warehouse picker + logout" sidebar used
  via the _SupOrTra.cshtml shared layout on 36 non-admin-styled pages, kept
  deliberately separate from AdminSidebar to avoid two sidebars on one page.

PART 1 — Mechanical swap (partial → ViewComponent), DONE:
- Converted all 31 real `<partial name="_AdminSidebar" model='"X"' />` call
  sites (the 32nd hit was just a code comment mentioning the name, not a
  real call) to `<vc:admin-sidebar active="X" />`, preserving the exact same
  "active" string each page already used — verified every one of those
  strings is checked by Default.cshtml, so highlighting still works
  correctly with zero mismatches.
- No `@addTagHelper` registration needed — `_ViewImports.cshtml` already has
  `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`, which covers
  ViewComponent tag helpers app-wide.

PART 2 — Duplicate mobile-toggle button conflict, FOUND AND FIXED:
Every one of those 31 pages had its OWN page-level `#mobileToggle` button
wired by wwwroot/js/sidebarToggle.js to toggle an `.active` class on
`.dashboard-sidebar` (the OLD custom mechanism). The new AdminSidebar
component's Default.cshtml renders its OWN separate `.mobile-toggle` button
using `data-bs-toggle="offcanvas"` (Bootstrap's real Offcanvas). Left
unfixed, every page would've shown two overlapping toggle buttons, and the
old one would've been dead (the sidebar is now a real `.offcanvas`, not a
`.dashboard-sidebar.active`).
Fix applied:
- Removed the old `#mobileToggle` button markup + its
  `<script src="~/js/sidebarToggle.js">` reference from all 31 pages.
- Administration/Details.cshtml additionally had its own inline JS wiring
  the same button — removed that block too.
- Verified via grep that no other code (JS or cshtml) still depends on
  `.dashboard-sidebar.active` — the only two places that did
  (sidebarToggle.js, and an already-unused employeeDetails.js) are now
  referenced by zero pages.
- Removed the now-dead `.dashboard-sidebar` / `.dashboard-sidebar.active`
  positioning rules from the `@media (max-width: 900px)` block in
  wwwroot/css/sidebar.css. Left the `.mobile-toggle { display: flex
  !important; }` visibility rule as-is — STILL MOBILE-ONLY (<=900px), same
  as before. Making it show at ALL screen sizes (mentioned as an eventual
  goal in the original brief) was NOT done — that's a real UX decision
  (hamburger visible on desktop too?), not cleanup, so it was left alone.
  ASK THE USER if this is wanted before changing display behavior.

PART 3 — UserSidebar retired, unified rollout to every remaining page, DONE:
User's explicit instruction: remove UserSidebar entirely, AdminSidebar
everywhere, keep the toggle button, keep the accordion categories, keep
permission-based hiding.
- Views/Shared/_SupOrTra.cshtml (the shared LAYOUT — 36 pages: Supplier,
  Driver, Truck, TransferRequest, SupplyOrder, SupplierRequest,
  Department/Warehouses/Dock/Position/Administration's Create/Edit/Details/
  Delete forms, and Authentication/SupOrTra.cshtml) now renders
  `<vc:admin-sidebar active="@(ViewData["ActiveSidebar"] ?? "")" />`
  unconditionally instead of `@await Component.InvokeAsync("UserSidebar")`.
- Each of the 35 pages on that layout that has an obvious matching
  accordion category got one new line added to its existing `@{ }` block:
  `ViewData["ActiveSidebar"] = "X";` (e.g. Warehouses/Edit.cshtml →
  "Warehouses", Truck/AddTraDriver.cshtml → "Trucks"). Full mapping was
  derived from what the sibling Index page of the same entity already used
  — verify list below if something looks highlighted wrong.
- Views/SupplyOrder/Index.cshtml and Views/Authentication/SupOrTra.cshtml
  were left with no ActiveSidebar value (no matching category exists yet) —
  they render the accordion with nothing highlighted. Not broken, just
  nothing lit up.
- Views/Authentication/AddRoleOrView.cshtml and
  Views/Authentication/viewRole.cshtml are standalone (`Layout = null`) and
  called UserSidebar directly rather than via the shared layout — both
  swapped to `<vc:admin-sidebar active="" />` directly in the view. Verified
  both actions (`AddRoleOrView`, `ViewRole`) and `SupOrTra` are
  `[PermissionAuthorize]` (require login) — no "anonymous user, no context
  to resolve" problem, safe to render the full component there.
- UserSidebarViewComponent.cs and Views/Shared/Components/UserSidebar/
  (whole folder) DELETED. Confirmed zero remaining references anywhere in
  .cs or .cshtml files (one harmless comment of mine in _SupOrTra.cshtml
  mentions the name only to say it was removed).
- Permission-based hiding: NOT NEW CODE NEEDED — IUserPermissionService.
  HasAsync() already correctly mirrors PermissionAuthorizeFilter's exact
  three-tier logic (admin bypass → hard AdminOnlyControllers denylist →
  granular GroupPermission lookup), confirmed by reading it, not assumed.
  So a non-admin never sees a sidebar link they'd get a 403 on — this was
  already true before this session's changes, just re-verified.

STILL OPEN / NOT DECIDED — genuinely needs the user, don't guess:
1. Mobile-toggle button visibility is still gated to <=900px only (see Part
   2). Confirm whether "all screen sizes" is actually wanted now.
2. Four pages were NEVER given ANY sidebar, by either UserSidebarViewComponent
   or AdminSidebar, and were left untouched this session:
   - Login.cshtml, Intro.cshtml — genuinely pre-authentication, no user
     session exists yet, so a permission-gated admin accordion makes no
     sense here. Almost certainly should stay excluded.
   - ChangeInitialPassword.cshtml — user IS authenticated at this point
     (mid-forced-password-change flow) but hasn't finished onboarding.
   - AccessDenied.cshtml — user IS authenticated, just denied one specific
     resource; arguably the MOST useful place to have a working sidebar so
     they can navigate away.
   All four are standalone (`Layout = null`), so adding a sidebar to any of
   them means editing each file directly (they don't use _SupOrTra layout).
   USER HAS NOT CONFIRMED whether "everywhere" was meant to include these 4
   — asked, awaiting answer. Do not add sidebar to these without an
   explicit yes, especially Login/Intro (would render nonsense —
   "أهلا, مستخدم" placeholder greeting — to an anonymous visitor).
3. Migration/DB status (Part A, separate from this sidebar work) is STILL
   UNCONFIRMED — user was asked to run `dotnet ef migrations list` and
   confirm whether `dotnet ef database update` has been run for
   AddWarehouseScoping. Not yet answered as of this doc. Doesn't block
   sidebar work (verified independent), but blocks anything touching
   Shift/Warehouse code.

====================================================================
SESSION 2 — OVERLAY REWRITE + BUG FIXES (continuation after token cutoff)
====================================================================
User reported, with screenshots:
1. Wanted the sidebar to actually show/hide on the hamburger press, and
   wanted the page content to NEVER shift/move when toggling — stay
   centered/fixed always.
2. Authentication/AddRoleOrView and Authentication/SupOrTra "don't have it".
3. Authentication/viewRole was visibly broken (giant unstyled icon filling
   the screen).
4. Supplier/Index "has it but it doesn't open".
5. Truck/Index?supplierId=1 "has it but it doesn't open too, and has text
   on the left".
6. "some other pages have the same problems."

ROOT CAUSES FOUND (by actually reading the rendered CSS/HTML, not guessing):
- viewRole.cshtml and AddRoleOrView.cshtml call the AdminSidebar component
  but link NEITHER sidebar.css NOR sidebar-widget.css — the component's
  markup rendered completely unstyled, so its bare `<svg viewBox="0 0 24
  24">` icons (no CSS width/height) expanded to fill the width of their
  container, producing the giant black icon in the screenshot.
- Truck/Index, Supplier/Index, and every other page on _SupOrTra layout:
  their own page CSS (e.g. companyCars.css) sets `body { display: flex;
  justify-content: center; ... }`. The AdminSidebar's `<aside
  class="dashboard-sidebar">` was a second flow child of that flex body
  (position: relative), so the flex algorithm placed it as a second flex
  item — in an RTL flex row that lands on the LEFT edge, which is exactly
  the "text on the left" the user saw (it was the unstyled/dim sidebar
  peeking out, not page content). And because the old JS only toggled a
  `.sidebar-open` class that had ZERO matching CSS rule anywhere, pressing
  the hamburger visibly did nothing — hence "doesn't open".
- On the plain-block (non-flex) standalone pages the same "sidebar is just
  a normal flow element" problem meant the sidebar rendered BELOW the
  visible viewport (after the page's own full-height content), invisible
  without scrolling — and again the toggle had no CSS to act on.
- Common thread: the sidebar's visibility/position was never independent
  of the host page's own layout, so it broke differently on every
  differently-built page, and there was no working show/hide mechanism at
  all (the `.sidebar-open` class was dead).

FIX — rebuilt AdminSidebar as a fully self-contained fixed-overlay widget,
all inside Views/Shared/Components/AdminSidebar/Default.cshtml (the ONE
file every page already calls, so this fixes every page at once with no
per-page edits needed):
- `.dashboard-sidebar` is now `position: fixed !important` (top/right/
  bottom 0, 280px wide), hidden by default via `transform:
  translateX(100%)`, slid in via `.sidebar-open { transform:
  translateX(0) }`. Because it's `position: fixed`, it is removed from
  the host page's normal flow / flex layout entirely — it can no longer
  become a stray flex item, and toggling it can never resize or reposition
  the page's own content (main content literally never references the
  sidebar's state). This directly satisfies "don't make pages move,
  keep centered always."
- Added a `.sidebar-backdrop` overlay (dimmed background, click-to-close)
  and Esc-to-close, in addition to the hamburger toggle.
- ALL of the sidebar's own visual styling (colors, sizing, the user
  footer, the mobile-toggle button, `#warehouseSwitcher`) was copied
  in full, inline, into this component's own `<style>` block — it no
  longer depends on sidebar.css or sidebar-widget.css being linked by
  the host page at all. This is what fixes viewRole/AddRoleOrView
  without touching those files: the giant-icon bug is gone because the
  component now always carries its own complete styling everywhere it's
  used.
- Toggle button raised to z-index 2001 (above the sidebar's 2000 and the
  backdrop's 1999) so it stays visible/clickable as a close control even
  while the sidebar is open.
- sidebar.css and sidebar-widget.css were intentionally left UNCHANGED —
  their old `.dashboard-sidebar`/`.sidebar-*`/`.mobile-toggle` rules are
  now dead weight (fully superseded by the component's own inline
  `!important` styles, which win any cascade tie because they're emitted
  later in the document — inside `<body>` — than any page's `<head>`
  `<link>`). Left as-is to minimize risk of breaking something
  page-specific that also happens to load those files; safe to clean up
  later as a separate pass if desired.
- Old dead responsive rules in dashboard.css / employeeDetails.css /
  detailsPages.css (`.dashboard-sidebar.active { right: 0 }`, `.mobile-
  toggle { display: none }` etc., leftover from the pre-unification
  offcanvas mechanism) were also left alone for the same reason — they
  target a `.active` class the sidebar no longer uses, and any
  `display`/`position` value they set is a plain (non-!important) rule
  that the component's `!important` always overrides.

VERIFIED BY READING (not assumed):
- Every one of the 34 pages that calls `<vc:admin-sidebar>` or
  `Component.InvokeAsync("AdminSidebar")` renders through this same
  Default.cshtml, so this is a single point of fix — no per-page CSS
  audit needed going forward for sidebar issues specifically.
- Checked z-index usage app-wide (grep across wwwroot/css/*.css): nothing
  else in the app uses 1999-2001, and the few higher values that do exist
  (9999/10000/99999 — toasts, a logout/stepback button on the SupOrTra
  intro screens) sit above the sidebar, which is correct (those should
  stay visible/usable over the sidebar).

STILL OPEN — genuinely worth flagging, not fixed this session:
1. UX judgment call made without asking: the sidebar now defaults to
   CLOSED on every page (including the 31 "dashboard" pages where it used
   to be permanently visible, e.g. GroupManagment in the user's own
   screenshot). This was the most literal reading of "I want it to show
   and hide when press the three lines button" + "don't move the page" —
   an always-open-and-reserving-280px sidebar can't be toggled without
   either covering content or shifting it. If the user actually wants it
   OPEN BY DEFAULT on desktop/wide screens (auto-open above some
   breakpoint, closed by default only on narrow/mobile), that's a
   straightforward follow-up: default `.sidebar-open` on load above e.g.
   900px, or persist the last state in localStorage. Ask before guessing
   which.
2. sidebar.css / sidebar-widget.css still contain the old, now-fully-dead
   `.dashboard-sidebar`/`.sidebar-*` rule blocks (see above) — safe to
   delete in a follow-up cleanup pass; not done this session to keep the
   diff minimal and low-risk.
3. Did not re-verify migration/DB status (Part A) — still unconfirmed
   from Session 1, unrelated to this sidebar work.

CAUTION FOR NEXT EDITOR (bug introduced and fixed within this same
session, but worth flagging so it isn't repeated): the new explanatory
comment block at the top of Default.cshtml's `<style>` originally
contained literal angle-bracket mentions of HTML tags (`<style>`,
`<body>`, `<head>`, `<link>`) written as plain English references inside
a CSS `/* ... */` comment. Razor's parser does not understand CSS comment
syntax — it scans the whole file for tag-like tokens regardless of
whether they're inside a `<style>` block or a CSS comment. Those stray
mentions were counted as real unclosed tags, which broke Razor's tag
-balance tracking for the *entire* file and surfaced as "malformed head
tag helper" / "malformed body tag helper" compile errors on every page
that invokes this component (i.e. broke the whole app, not just this
file). Fixed by rewording the comments to avoid angle brackets entirely
(e.g. "inside the body element" instead of "inside `<body>`"). Lesson:
never write a literal `<tagname>` inside any comment in a .cshtml file,
CSS comment or otherwise — spell it out in prose instead.

ACTIVE-CATEGORY MAPPING USED (for reference / auditing):
  Suppliers        → Supplier/{Create,Index,Edit}, Administration/Suppliers,
                      SupplierRequest/CreateTruckWithDriver → "SupplierRequests" (not Suppliers)
  Drivers          → Driver/{Create,Index,addDriverTransfer,TransferDrivers},
                      Administration/{DriversIndex,DriverEdit,DriverDetails}
  Trucks           → Truck/{Create,Index,MainTraDrivers,AddTraDriver,Edit},
                      Administration/{TrucksIndex,TruckCreate,TruckEdit,TruckDetails}
  Employees        → Administration/{Index,Create,ChangePassword,Edit,Details}
  SupplierRequests → Administration/SupplierRequests, SupplierRequest/CreateTruckWithDriver
  TransferRequests → Administration/TransferRequests, TransferRequest/Create
  Groups           → Group/{GroupDetails,GroupManagment,ManagePermissions}
  Roles            → Group/MangeRolesIndex
  Options          → Group/{MangeTypesIndex,MangeTypeDetails}, TruckTypes/*, DriverType/*
  Warehouses       → Warehouses/{Index,Create,Details,Delete,Edit}
  Departments      → Department/{Index,Create,Details,Update,Delete}
  Docks            → Dock/{Index,Create,Details,Update,Delete}
  Shifts           → Shift/Index
  Position         → Position/{Index,Create,Delete,Edit}
  (none)           → SupplyOrder/Index, Authentication/SupOrTra,
                      Authentication/AddRoleOrView, Authentication/viewRole

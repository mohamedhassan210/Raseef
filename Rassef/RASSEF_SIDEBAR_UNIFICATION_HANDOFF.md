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

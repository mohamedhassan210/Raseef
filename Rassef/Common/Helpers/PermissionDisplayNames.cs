namespace Rassef.Common.Helpers
{
    /// <summary>
    /// Permissions are discovered by reflection off the real C# controller/action names
    /// (see PermissionHelper.DiscoverPermissions), so ControllerName/ActionName are always
    /// the literal English identifiers — that's necessary for permission checks to keep
    /// working, but it's not something to show a non-technical admin managing group access.
    ///
    /// This is a display-only lookup: it never changes what's stored or checked, only what
    /// text the ManagePermissions screen renders for a given ControllerName/ActionName.
    ///
    /// Lookup order for an action, most specific first:
    ///   1. "Controller.Action" in SpecificActionNames — for actions whose meaning depends on
    ///      which controller they're on (e.g. Administration.TrucksIndex vs a generic Index).
    ///   2. "Action" in CommonActionNames — generic CRUD-style names shared across most
    ///      controllers (Index/Create/Details/Edit/Update/Delete/DeleteConfirmed).
    ///   3. The raw ActionName — if a new action gets added to a controller and nobody has
    ///      translated it yet, the admin sees the English name instead of a blank or an
    ///      exception. Same fallback behaviour for controller names.
    /// </summary>
    public static class PermissionDisplayNames
    {
        private static readonly Dictionary<string, string> ControllerNames =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["ActionTypes"] = "أنواع الإجراءات",
            ["Administration"] = "الإدارة العامة",
            ["Authentication"] = "المصادقة وتسجيل الدخول",
            ["CheckOut"] = "تسجيلات الخروج",
            ["CommodityTypes"] = "أنواع البضائع",
            ["Department"] = "الأقسام",
            ["DepartmentType"] = "أنواع الأقسام",
            ["Dock"] = "الأرصفة",
            ["DockAssignment"] = "تخصيص الأرصفة",
            ["DockStatuses"] = "حالات الأرصفة",
            ["Driver"] = "السائقون",
            ["DriverType"] = "أنواع السائقين",
            ["ExitTypes"] = "أنواع الخروج",
            ["Group"] = "المجموعات والأدوار",
            ["PermitTypes"] = "أنواع التصاريح",
            ["Position"] = "الأدوار الوظيفية",
            ["QueueAction"] = "إجراءات الطابور",
            ["QueueSettings"] = "إعدادات الطابور",
            ["QueueTicket"] = "تذاكر الطابور",
            ["RequestStatuses"] = "حالات الطلبات",
            ["Shift"] = "الورديات",
            ["Supplier"] = "الموردون",
            ["SupplierRequest"] = "طلبات التوريد",
            ["SupplyOrder"] = "أوامر التوريد",
            ["TicketStatuses"] = "حالات التذاكر",
            ["TransferRequest"] = "طلبات التحويل",
            ["Truck"] = "الشاحنات",
            ["TruckTypes"] = "أنواع الشاحنات",
            ["Warehouse"] = "المستودعات",
        };

        // Shared by most controllers — the ordinary CRUD verbs discovered on almost every one.
        private static readonly Dictionary<string, string> CommonActionNames =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["Index"] = "عرض القائمة",
            ["Create"] = "إضافة",
            ["Details"] = "عرض التفاصيل",
            ["Edit"] = "تعديل",
            ["Update"] = "تعديل",
            ["Delete"] = "حذف",
            ["DeleteConfirmed"] = "تأكيد الحذف",
        };

        // "Controller.Action" — actions that are either controller-specific or whose generic
        // CRUD name would be misleading out of context (e.g. Administration.Index lists
        // employees, not a generic index, so it gets its own entry below).
        private static readonly Dictionary<string, string> SpecificActionNames =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["Administration.Index"] = "عرض الموظفين",
            ["Administration.Create"] = "إضافة موظف",
            ["Administration.Details"] = "تفاصيل الموظف",
            ["Administration.Edit"] = "تعديل الموظف",
            ["Administration.Delete"] = "حذف موظف",
            ["Administration.DeleteConfirmed"] = "تأكيد حذف الموظف",
            ["Administration.ChangePassword"] = "تغيير كلمة المرور",
            ["Administration.CallNextSupplierRequest"] = "استدعاء طلب التوريد التالي",
            ["Administration.CompleteTicket"] = "إنهاء التذكرة",
            ["Administration.UpdateTicketStatus"] = "تحديث حالة التذكرة",
            ["Administration.Suppliers"] = "عرض الموردين",
            ["Administration.SupplierDetails"] = "تفاصيل المورد",
            ["Administration.SupplierDeleteConfirmed"] = "تأكيد حذف المورد",
            ["Administration.SupplierRequests"] = "عرض طلبات التوريد",
            ["Administration.SupplierRequestDeleteConfirmed"] = "تأكيد حذف طلب التوريد",
            ["Administration.TransferRequests"] = "عرض طلبات التحويل",
            ["Administration.TransferRequestDeleteConfirmed"] = "تأكيد حذف طلب التحويل",
            ["Administration.TrucksIndex"] = "عرض الشاحنات",
            ["Administration.TruckCreate"] = "إضافة شاحنة",
            ["Administration.TruckDetails"] = "تفاصيل الشاحنة",
            ["Administration.TruckEdit"] = "تعديل الشاحنة",
            ["Administration.TruckDeleteConfirmed"] = "تأكيد حذف الشاحنة",
            ["Administration.DriversIndex"] = "عرض السائقين",
            ["Administration.DriverCreate"] = "إضافة سائق",
            ["Administration.DriverDetails"] = "تفاصيل السائق",
            ["Administration.DriverEdit"] = "تعديل السائق",
            ["Administration.DriverDelete"] = "حذف سائق",
            ["Administration.DriverDeleteConfirmed"] = "تأكيد حذف السائق",

            ["Authentication.AccessDenied"] = "شاشة رفض الوصول",
            ["Authentication.AddRoleOrView"] = "اختيار نوع الخدمة",
            ["Authentication.CallNext"] = "استدعاء التالي",
            ["Authentication.ChangeInitialPassword"] = "تعيين كلمة المرور الأولى",
            ["Authentication.ForgetPassword"] = "نسيت كلمة المرور",
            ["Authentication.Intro"] = "الشاشة الترحيبية",
            ["Authentication.Login"] = "تسجيل الدخول",
            ["Authentication.Logout"] = "تسجيل الخروج",
            ["Authentication.SelectWarehouse"] = "اختيار المستودع",
            ["Authentication.SupOrTra"] = "اختيار التوريد أو التحويل",
            ["Authentication.UpdateStatus"] = "تحديث الحالة",
            ["Authentication.UserProfile"] = "الملف الشخصي",
            ["Authentication.ViewRole"] = "عرض الدور",

            ["Department.Reset"] = "إعادة تعيين القسم",

            ["Driver.CreateSupplierTicket"] = "إنشاء تذكرة مورد",
            ["Driver.DeleteDriver"] = "حذف السائق",
            ["Driver.GetLiveStatus"] = "الحالة اللحظية",
            ["Driver.LiveStatus"] = "شاشة الحالة اللحظية",
            ["Driver.Recript"] = "إعادة طباعة الإيصال",
            ["Driver.TransferDrivers"] = "تحويل السائقين",
            ["Driver.addDriverTransfer"] = "إضافة تحويل سائق",

            ["Group.AddGroup"] = "إضافة مجموعة",
            ["Group.EditGroup"] = "تعديل مجموعة",
            ["Group.DeleteGroup"] = "حذف مجموعة",
            ["Group.GroupDetails"] = "تفاصيل المجموعة",
            ["Group.GroupManagment"] = "إدارة المجموعات",
            ["Group.ManagePermissions"] = "إدارة الصلاحيات",
            ["Group.MangeRolesIndex"] = "إدارة الأدوار",
            ["Group.RefreshPermissions"] = "تحديث الصلاحيات",
            ["Group.TransferUser"] = "نقل موظف بين المجموعات",
            ["Group.CreateTypeItem"] = "إضافة عنصر اختيار",
            ["Group.EditTypeItem"] = "تعديل عنصر اختيار",
            ["Group.DeleteTypeItem"] = "حذف عنصر اختيار",
            ["Group.MangeTypesIndex"] = "إدارة الاختيارات",
            ["Group.MangeTypeDetails"] = "تفاصيل الاختيار",

            ["Position.Edit"] = "تعديل الدور الوظيفي",

            ["QueueSettings.ResetAll"] = "إعادة تعيين الكل",

            ["QueueTicket.CallNext"] = "استدعاء التالي",
            ["QueueTicket.CallStation"] = "محطة استدعاء الأدوار",
            ["QueueTicket.CheckoutTicket"] = "تسجيل الخروج بالتذكرة",
            ["QueueTicket.ExitGate"] = "بوابة الخروج",
            ["QueueTicket.GetLiveDisplayData"] = "بيانات العرض اللحظي",
            ["QueueTicket.GetStationState"] = "حالة محطة الاستدعاء",
            ["QueueTicket.LiveQueue"] = "قائمة الانتظار اللحظية",
            ["QueueTicket.SearchTicketForCheckout"] = "البحث عن تذكرة للخروج",
            ["QueueTicket.UpdateStatus"] = "تحديث حالة التذكرة",

            ["Shift.ResetAll"] = "إعادة تعيين كل الورديات",
            ["Shift.ResetDepartment"] = "إعادة تعيين وردية القسم",
            ["Shift.ResetShift"] = "إعادة تعيين الوردية",
            ["Shift.SaveSettings"] = "حفظ إعدادات الوردية",

            ["SupplierRequest.CreateConfirmed"] = "تأكيد إنشاء طلب التوريد",
            ["SupplierRequest.CreateTruckWithDriver"] = "إضافة شاحنة وسائق للطلب",
            ["SupplierRequest.SearchDrivers"] = "البحث عن سائقين",

            ["SupplyOrder.ExportToExcel"] = "تصدير إلى إكسل",

            ["TransferRequest.EnsureOrder"] = "التحقق من ترتيب الطلب",

            ["Truck.AddTraDriver"] = "إضافة سائق نقل",
            ["Truck.CreateTransferTicket"] = "إنشاء تذكرة تحويل",
            ["Truck.MainTraDrivers"] = "سائقو النقل الرئيسيون",
        };

        public static string GetControllerDisplayName(string? controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return controllerName ?? string.Empty;

            return ControllerNames.TryGetValue(controllerName, out var arabicName)
                ? arabicName
                : controllerName;
        }

        public static string GetActionDisplayName(string? controllerName, string? actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return actionName ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(controllerName))
            {
                var specificKey = $"{controllerName}.{actionName}";
                if (SpecificActionNames.TryGetValue(specificKey, out var specificName))
                    return specificName;
            }

            return CommonActionNames.TryGetValue(actionName, out var commonName)
                ? commonName
                : actionName;
        }
    }
}

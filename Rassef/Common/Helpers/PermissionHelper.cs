namespace Rassef.Common.Helpers
{
    public static class PermissionHelper
    {
        /// <summary>
        /// كنترولرز بنية تحتية (صفحة الأخطاء ونقاط التصدير): مش صلاحيات بتتوزّع على المجموعات،
        /// فمتتسجلش في جدول الصلاحيات ومتظهرش في شاشة إدارة الصلاحيات. التصدير بيتحكم فيه
        /// [PermissionAuthorize("Administration", ...)] بتاع الصفحة نفسها.
        /// </summary>
        public static readonly HashSet<string> InfrastructureControllers =
            new(StringComparer.OrdinalIgnoreCase) { "Home", "RequestsExport", "ReportExport" };

        public static List<PermissionViewModel> DiscoverPermissions()
        {
            var permissions = new List<PermissionViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => (typeof(Controller).IsAssignableFrom(type) || type.Name.EndsWith("Controller"))
                               && !InfrastructureControllers.Contains(type.Name.Replace("Controller", "")));

            foreach (var controller in controllers)
            {
                var controllerName = controller.Name.Replace("Controller", "");

                var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(m =>
                    !m.IsSpecialName &&
                    !m.IsVirtual &&
                    !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any() &&
                    !m.IsDefined(typeof(NonActionAttribute)))
                    .Select(x => x.Name)
                    .Distinct();

                foreach (var action in actions)
                {
                    permissions.Add(new PermissionViewModel
                    {
                        ActionName = action,
                        ControllerName = controllerName,
                        Description = $"صلاحية {action} في {controllerName}"
                    });
                }
            }
            return permissions;
        }
    }
}

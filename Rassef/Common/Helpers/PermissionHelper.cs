namespace Rassef.Common.Helpers
{
    public static class PermissionHelper
    {
        public static List<PermissionViewModel> DiscoverPermissions()
        {
            var permissions = new List<PermissionViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) || type.Name.EndsWith("Controller"));

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

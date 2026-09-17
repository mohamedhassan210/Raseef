namespace Rassef.ViewModels.Shared
{
    /// <summary>
    /// Data for one instance of the shared "_SelectDropdown" partial — the custom
    /// styled dropdown (hidden native &lt;select&gt; + JS-rendered list) used across
    /// Department/Create.cshtml, Warehouses/CreateDepartment.cshtml, and the
    /// SupplierRequest Create/Update ("doc create"/"doc edit") views. Pulling this
    /// markup into one partial avoids re-typing the same ~20 lines per field per view.
    /// </summary>
    public class SelectDropdownVM
    {
        public string FieldName { get; }
        public string Label { get; }
        public IEnumerable<SelectListItem> Items { get; }
        public int? SelectedValue { get; }
        public string Placeholder { get; }
        public bool Required { get; }

        public SelectDropdownVM(
            string fieldName,
            string label,
            IEnumerable<SelectListItem>? items,
            int? selectedValue,
            string placeholder,
            bool required = true)
        {
            FieldName = fieldName;
            Label = label;
            Items = items ?? new List<SelectListItem>();
            SelectedValue = selectedValue;
            Placeholder = placeholder;
            Required = required;
        }
    }
}

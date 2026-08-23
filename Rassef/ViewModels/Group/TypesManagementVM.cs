using System;
using System.Collections.Generic;

namespace Rassef.ViewModels.Group
{
    public class TypesManagementVM
    {
        public List<TypeCardVM> TypeCategories { get; set; } = new();
    }

    public class TypeCardVM
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    public class TypeDetailsVM
    {
        public string TypeKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<TypeItemVM> Items { get; set; } = new();
    }

    public class TypeItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

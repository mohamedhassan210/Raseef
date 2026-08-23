using System;

namespace Rassef.ViewModels.Group
{
    public class GroupCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UsersCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}

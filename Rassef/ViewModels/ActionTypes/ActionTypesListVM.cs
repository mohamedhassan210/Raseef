namespace Rassef.ViewModels.ActionTypes
{
    public class ActionTypesListVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم الإجراء")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد الحركات/العمليات")]
        public int QueueActionsCount { get; set; }
    }
}

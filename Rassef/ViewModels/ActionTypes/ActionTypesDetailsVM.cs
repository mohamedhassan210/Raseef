namespace Rassef.ViewModels.ActionTypes
{
    public class ActionTypesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم الإجراء")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد الحركات المرتبطة")]
        public int QueueActionsCount { get; set; }
    }
}


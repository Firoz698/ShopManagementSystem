namespace ShopManagementSystem.ViewModels
{
    public class MenuPermissionInput
    {
        public int MenuId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

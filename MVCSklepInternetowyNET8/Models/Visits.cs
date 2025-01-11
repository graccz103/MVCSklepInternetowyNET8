namespace MVCSklepInternetowyNET8.Models
{
    public class Visit
    {
        public int VisitId { get; set; }
        public DateTime VisitDate { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}

using Microsoft.AspNetCore.Routing.Constraints;

namespace BuildingManagementTool.Models
{
    public class Invitation
    {
        public int InvitationId { get; set; }
        public string Email { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        public string Status { get; set; }
        public DateTime SentOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public string InvitedBy { get; set; }
    }
}

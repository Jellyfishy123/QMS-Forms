using System;
using System.ComponentModel.DataAnnotations;

namespace QMSForms.Models
{
    public enum RequestStatus
    {
        Draft,        // Red
        Acknowledged, // Orange
        Completed,    // Yellow
        Approved,     // Green
        Closure       // Blue
    }

    public class Request
    {
        public Guid Id { get; set; }

        [Required]
        public string? Type { get; set; } 

        [Required]
        [Display(Name = "Project Code")]
        public string? ProjectCode { get; set; } 

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow; // UTC

        [Required]
        [Display(Name = "Target Completion Date")]
        public DateTime TargetCompletionDate { get; set; } = DateTime.UtcNow.AddDays(14); // UTC + 2 weeks

        [Required]
        public string? Location { get; set; }

        [Required]
        [Display(Name = "Person in Charge")]
        public string? PersonInCharge { get; set; }

        [Required]
        [Display(Name = "Non-Conformity Highlighted")]
        public string? NonConformityHighlighted { get; set; }

        [Required]
        [Display(Name = "Non-Conformity Related To")]
        public string? NonConformityRelatedTo { get; set; }

        [Required]
        [Display(Name = "Description of Non-Conformity")]
        public string? DescriptionOfNonConformity { get; set; }

        public string? AttachmentPath { get; set; }

        [Required]
        [Display(Name = "Designated Person To Close")]
        public string? DesignatedPersonToClose { get; set; }

        public string? Title { get; set; }

        public string? RequestType { get; set; }


        // NEW FIELDS FOR ACKNOWLEDGED STAGE
        public string? DescriptionOfRootCause { get; set; }
        public string? TypeOfRootCause { get; set; }
        public string? CorrectivePreventiveAction { get; set; }
        public string? PMOrCM { get; set; }

        // Fields for Completed stage (nullable to allow Draft/Completed without approval)
        public string? ApprovalDecision { get; set; }  // "Approve", "Reject", or null
        public string? ApprovalComments { get; set; }

        // Fields for Approved stage
        // Closure stage
        public string? ClosureBy { get; set; }
        public string? ClosureComments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public RequestStatus Status { get; set; } = RequestStatus.Draft;
    }
}

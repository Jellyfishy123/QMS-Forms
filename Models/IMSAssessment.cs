using System;
using System.ComponentModel.DataAnnotations;

namespace QMSForms.Models
{
    public class IMSAssessment
    {
        public int Id { get; set; }

        [Required]
        public string Project { get; set; } = string.Empty;

        public string SiteRepresentative { get; set; } = string.Empty;

        // Force UTC kind for PostgreSQL compatibility
        private DateTime _dateOfAssessment = DateTime.UtcNow.Date;
        [DataType(DataType.Date)]
        public DateTime DateOfAssessment
        {
            get => _dateOfAssessment;
            set => _dateOfAssessment = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string AssessmentNo { get; set; } = string.Empty;

        public string AssessorAuditor { get; set; } = string.Empty;

        public decimal? OverallInspectionScore { get; set; }

        // Photo Upload
        public string SitePhotoPath { get; set; } = string.Empty;

        // Force UTC for CreatedDate as well
        private DateTime _createdDate = DateTime.UtcNow;
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => _createdDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        // Navigation property for related rows
        public ICollection<QMRow> QMRows { get; set; } = new List<QMRow>();

    }
}
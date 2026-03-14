using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QMSForms.Models
{
    public class QMRow
    {
        [Key]
        public int Id { get; set; }

        // Remove Required to avoid model validation failure
        public string Question { get; set; } = string.Empty;

        public int Ideal { get; set; } = 100;

        public int? Actual { get; set; }

        public string ObservationRemarks { get; set; } = string.Empty;

        public int IMSAssessmentId { get; set; }

        [ForeignKey("IMSAssessmentId")]
        public IMSAssessment? IMSAssessment { get; set; }
    }
}
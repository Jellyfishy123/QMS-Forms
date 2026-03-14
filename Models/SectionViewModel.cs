using System.ComponentModel.DataAnnotations;

namespace QMSForms.Models
{
    public class SectionViewModel
    {
        public int AssessmentId { get; set; }

        [Required]
        public List<QMRow> QM1Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM2Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM3Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM4Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM5Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM6Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM7Rows { get; set; } = new List<QMRow>();
        public List<QMRow> QM8Rows { get; set; } = new List<QMRow>();

        // Dynamic inspection scores per section
        public decimal QM1Score { get; set; }
        public decimal QM2Score { get; set; }
        public decimal QM3Score { get; set; }
        public decimal QM4Score { get; set; }
        public decimal QM5Score { get; set; }
        public decimal QM6Score { get; set; }
        public decimal QM7Score { get; set; }
        public decimal QM8Score { get; set; }

        // For read-only view after saving
        public bool IsReadOnly { get; set; } = false;
    }
}
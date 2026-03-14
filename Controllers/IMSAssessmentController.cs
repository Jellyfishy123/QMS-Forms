using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSForms.Data;
using QMSForms.Models;

namespace QMSForms.Controllers
{
    public class IMSAssessmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IMSAssessmentController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var assessments = await _context.IMSAssessments
                                            .OrderByDescending(a => a.CreatedDate)
                                            .ToListAsync();
            return View(assessments);
        }

        // Create Assessment
        public IActionResult Create()
        {
            return View(new IMSAssessment
            {
                DateOfAssessment = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IMSAssessment model, IFormFile SitePhoto)
        {
            if (ModelState.IsValid)
            {
                model.DateOfAssessment = DateTime.UtcNow;
                model.CreatedDate = DateTime.UtcNow;

                if (SitePhoto != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(SitePhoto.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await SitePhoto.CopyToAsync(stream);

                    model.SitePhotoPath = "/uploads/" + fileName;
                }

                _context.IMSAssessments.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Section", new { id = model.Id });
            }

            return View(model);
        }

        // Section (QM1 + QM2)
        public async Task<IActionResult> Section(int id)
        {
            var assessment = await _context.IMSAssessments
                                           .Include(a => a.QMRows)
                                           .FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null) return NotFound();

            var qmRows = assessment.QMRows.ToList();

            var model = new SectionViewModel
            {
                AssessmentId = assessment.Id,
                QM1Rows = qmRows.Any(r => r.Question.Contains("PQP") || r.Question.Contains("company policies") || r.Question.Contains("Organizational Chart") || r.Question.Contains("implementation"))
                            ? qmRows.Where(r => r.Question.Contains("PQP") || r.Question.Contains("company policies") || r.Question.Contains("Organizational Chart") || r.Question.Contains("implementation")).ToList()
                            : new List<QMRow>
                            {
                                new QMRow { Question="Is the PQP approved by client and available at site; reflects project scope and requirements.", Ideal=100 },
                                new QMRow { Question="Are the latest company policies display?", Ideal=100 },
                                new QMRow { Question="Are the IMS objectives are available and being control / monitor?", Ideal=100 },
                                new QMRow { Question="Latest Project Organizational Chart is available and display?", Ideal=100 },
                                new QMRow { Question="How does implementation and changes to integrated management system communicated to site office?", Ideal=100 }
                            },
                QM2Rows = qmRows.Any(r => r.Question.Contains("Personnel") || r.Question.Contains("Letter of Appointment"))
                            ? qmRows.Where(r => r.Question.Contains("Personnel") || r.Question.Contains("Letter of Appointment")).ToList()
                            : new List<QMRow>
                            {
                                new QMRow { Question="Are Personnel qualified and training records maintained.", Ideal=100 },
                                new QMRow { Question="Employees aware of their roles and Letter of Appointment is signed off.", Ideal=100 }
                            },
                IsReadOnly = assessment.QMRows.Any()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Section(SectionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var assessment = await _context.IMSAssessments
                                           .Include(a => a.QMRows)
                                           .FirstOrDefaultAsync(a => a.Id == model.AssessmentId);
            if (assessment == null) return NotFound();

            // Remove old rows
            if (assessment.QMRows.Any()) _context.QMRows.RemoveRange(assessment.QMRows);

            // Add QM1 + QM2 rows
            foreach (var row in model.QM1Rows.Concat(model.QM2Rows))
            {
                row.IMSAssessmentId = assessment.Id;
                _context.QMRows.Add(row);
            }

            // Update Overall QM Inspection Score
            var allRows = model.QM1Rows.Concat(model.QM2Rows).ToList();
            assessment.OverallInspectionScore = allRows.Any() ? (decimal?)allRows.Average(r => r.Actual ?? 0) : 0m;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Delete Assessment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var assessment = await _context.IMSAssessments
                                           .Include(a => a.QMRows)
                                           .FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null) return NotFound();

            if (assessment.QMRows.Any()) _context.QMRows.RemoveRange(assessment.QMRows);
            _context.IMSAssessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
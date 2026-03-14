using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSForms.Data;
using QMSForms.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// Add iText namespaces
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

using ClosedXML.Excel; // for Excel export

namespace QMSForms.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // GET: /Requests
        // ==========================
        public IActionResult Index(string status, string type, string project)
        {
            var allRequests = _context.Requests.AsQueryable();

            // Filter dropdowns
            ViewBag.AllStatuses = Enum.GetValues(typeof(RequestStatus)).Cast<RequestStatus>().ToList();
            ViewBag.AllTypes = _context.Requests.Select(r => r.Type).Distinct().ToList() ?? new List<string>();
            ViewBag.AllProjects = _context.Requests.Select(r => r.ProjectCode).Distinct().ToList() ?? new List<string>();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                allRequests = allRequests.Where(r => r.Status.ToString() == status);

            if (!string.IsNullOrEmpty(type))
                allRequests = allRequests.Where(r => r.Type == type);

            if (!string.IsNullOrEmpty(project))
                allRequests = allRequests.Where(r => r.ProjectCode == project);

            return View(allRequests.OrderByDescending(r => r.RequestDate).ToList());
        }

        // ==========================
        // GET: /Requests/Details/{id}
        // ==========================
        public async Task<IActionResult> Details(Guid id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound();

            // Auto-change Draft → Acknowledged
            if (request.Status == RequestStatus.Draft)
            {
                request.Status = RequestStatus.Acknowledged;
                request.UpdatedAt = DateTime.UtcNow;
                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            ViewBag.TypeOfRootCauseOptions = new[] { "Environment", "Machine", "Manpower", "Materials", "Method" };
            ViewBag.PMOrCMs = new[] { "melchor.ignacio@ktcgroup.com.sg", "tiendat.nguyen@ktcgroup.com.sg" };
            ViewBag.ClosureByOptions = new[] { "melchor.ignacio@ktcgroup.com.sg", "tiendat.nguyen@ktcgroup.com.sg" };

            return View(request);
        }

        // ==========================
        // POST: Update Acknowledged → Completed
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAcknowledged(Guid id, Request model, IFormFile attachment)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status == RequestStatus.Acknowledged)
            {
                request.DescriptionOfRootCause = model.DescriptionOfRootCause;
                request.TypeOfRootCause = model.TypeOfRootCause;
                request.CorrectivePreventiveAction = model.CorrectivePreventiveAction;
                request.PMOrCM = model.PMOrCM;

                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploads);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await attachment.CopyToAsync(stream);

                    request.AttachmentPath = "/uploads/" + fileName;
                }

                request.Status = RequestStatus.Completed;
                request.UpdatedAt = DateTime.UtcNow;

                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        // ==========================
        // POST: Approve Request
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(Guid id, string? approvalDecision, string? approvalComments)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status == RequestStatus.Completed && !string.IsNullOrEmpty(approvalDecision))
            {
                request.ApprovalDecision = approvalDecision;
                request.ApprovalComments = approvalComments;
                request.Status = approvalDecision == "Approve" ? RequestStatus.Approved : RequestStatus.Completed;
                request.UpdatedAt = DateTime.UtcNow;

                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        // ==========================
        // POST: Close Request
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseRequest(Guid id, string closureBy, string closureComments)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != RequestStatus.Approved) return BadRequest();

            request.ClosureBy = closureBy;
            request.ClosureComments = closureComments;
            request.Status = RequestStatus.Closure;
            request.UpdatedAt = DateTime.UtcNow;

            _context.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // ==========================
        // GET: Export Closed Request PDF
        // ==========================
        [HttpGet]
        public async Task<IActionResult> ExportClosedRequestPdf(Guid id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null || request.Status != RequestStatus.Closure)
                return BadRequest("Request is not closed or does not exist.");

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms, new WriterProperties());
            writer.SetSmartMode(false);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            // Add Title
            doc.Add(new Paragraph($"Closed Request: {request.Title}")
                .SetFontSize(18).SetTextAlignment(TextAlignment.CENTER));

            doc.Add(new Paragraph($"Type: {request.Type}"));
            doc.Add(new Paragraph($"Project: {request.ProjectCode}"));
            doc.Add(new Paragraph($"Request Date: {request.RequestDate:yyyy-MM-dd}"));
            doc.Add(new Paragraph($"Target Completion: {request.TargetCompletionDate:yyyy-MM-dd}"));
            doc.Add(new Paragraph($"Location: {request.Location}"));
            doc.Add(new Paragraph($"PIC: {request.PersonInCharge}"));
            doc.Add(new Paragraph($"Designated: {request.DesignatedPersonToClose}"));
            doc.Add(new Paragraph($"Status: {request.Status}"));
            doc.Add(new Paragraph($"Closure By: {request.ClosureBy}"));
            doc.Add(new Paragraph($"Closure Comments: {request.ClosureComments}"));

            doc.Close();
            return File(ms.ToArray(), "application/pdf", $"{request.Title}.pdf");
        }

        // ==========================
        // GET: Export all requests as Excel (filtered)
        /*
        public IActionResult ExportAllRequestsExcel(string status, string type, string project)
        {
            // Set EPPlus license context
            var allRequests = _context.Requests.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                allRequests = allRequests.Where(r => r.Status.ToString() == status);

            if (!string.IsNullOrEmpty(type))
                allRequests = allRequests.Where(r => r.Type == type);

            if (!string.IsNullOrEmpty(project))
                allRequests = allRequests.Where(r => r.ProjectCode == project);

            var list = allRequests.OrderByDescending(r => r.RequestDate).ToList();

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Requests");

            // Header row
            ws.Cells[1, 1].Value = "Title";
            ws.Cells[1, 2].Value = "Type";
            ws.Cells[1, 3].Value = "Project";
            ws.Cells[1, 4].Value = "Request Date";
            ws.Cells[1, 5].Value = "Target Completion";
            ws.Cells[1, 6].Value = "Location";
            ws.Cells[1, 7].Value = "PIC";
            ws.Cells[1, 8].Value = "Designated";
            ws.Cells[1, 9].Value = "Status";
            ws.Cells[1, 10].Value = "Approval";

            // Fill data
            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                ws.Cells[i + 2, 1].Value = r.Title;
                ws.Cells[i + 2, 2].Value = r.Type;
                ws.Cells[i + 2, 3].Value = r.ProjectCode;
                ws.Cells[i + 2, 4].Value = r.RequestDate.ToString("yyyy-MM-dd");
                ws.Cells[i + 2, 5].Value = r.TargetCompletionDate.ToString("yyyy-MM-dd");
                ws.Cells[i + 2, 6].Value = r.Location;
                ws.Cells[i + 2, 7].Value = r.PersonInCharge;
                ws.Cells[i + 2, 8].Value = r.DesignatedPersonToClose;
                ws.Cells[i + 2, 9].Value = r.Status.ToString();
                ws.Cells[i + 2, 10].Value = r.ApprovalDecision ?? "Pending";
            }

            // Auto-fit columns
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Requests_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        */


        // ==========================
        // POST: Delete
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound();

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // GET: Create Request
        // ==========================
        public IActionResult Create(string type)
        {
            var request = new Request
            {
                Type = type,
                RequestDate = DateTime.UtcNow,
                TargetCompletionDate = DateTime.UtcNow.AddDays(14),

                PersonInCharge = null,
                NonConformityHighlighted = null,
                NonConformityRelatedTo = null,
                ProjectCode = null,
                DesignatedPersonToClose = null,
            };

            ViewBag.PICs = new[] { "melchor.ignacio@ktcgroup.com.sg", "tiendat.nguyen@ktcgroup.com.sg" };
            ViewBag.NonConformityHighlighted = new[] { "Internal", "Authority", "Client", "Others" };
            ViewBag.NonConformityRelatedTo = new[] { "Quality", "H&S", "Environmental", "Others" };
            ViewBag.ProjectCodes = new[] { "HDBBSHO", "KTTJPS", "CR211" };
            ViewBag.DesignatedPersonToClose = new[] { "melchor.ignacio@ktcgroup.com.sg", "tiendat.nguyen@ktcgroup.com.sg" };

            return View(request);
        }

        // ==========================
        // POST: Create Request
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Request request, IFormFile attachment)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
                request.Type = "IAR";

            var year = DateTime.UtcNow.Year;
            var count = await _context.Requests
                .Where(r => r.Type == request.Type && r.CreatedAt.Year == year)
                .CountAsync();

            request.Title = $"{request.Type}-{year}-{"KTC/KTCCE"}-{(count + 1):D3}";
            request.RequestDate = DateTime.SpecifyKind(request.RequestDate, DateTimeKind.Utc);
            request.TargetCompletionDate = DateTime.SpecifyKind(request.TargetCompletionDate, DateTimeKind.Utc);
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Draft;

            if (attachment != null && attachment.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await attachment.CopyToAsync(stream);

                request.AttachmentPath = "/uploads/" + fileName;
            }

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

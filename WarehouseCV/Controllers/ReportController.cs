using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Python.Runtime;
using WarehouseCV.Data;
using WarehouseCV.Models;
using WarehouseCV.Services;

namespace WarehouseCV.Controllers;

public class GetReportDTO
{
    public int Id { get; set; }

    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public DateTime Date { get; set; }
    public string WarehouseZone { get; set; }
    public int ProductCount { get; set; }
    public int ProductId { get; set; }
    public string ImageFilename { get; set; }
    public string ImageBase64 { get; set; }
    public string UserEmail { get; set; }
    public int InventoryId { get; set; }
}

public class CreateReportDTO
{
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string WarehouseZone { get; set; }
    public int ProductId { get; set; }
    public string ImageFilename { get; set; }
    public string ImageBase64 { get; set; }
    public int InventoryId { get; set; }
}

[ApiController]
[Authorize]
[Route("api/[controller]s")]
public class ReportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly PythonScriptService _py;

    public ReportController(AppDbContext db, IWebHostEnvironment env, PythonScriptService py)
    {
        _db = db;
        _env = env;
        _py = py;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetReportDTO>> Get(int id)
    {
        Report? report = await _db
            .Reports.Include(r => r.Image)
            .Include(r => r.User)
            .Where(r => r.Id == id)
            .SingleAsync();

        if (report is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find report with id {id}"
            );
        }

        if (report.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Report with id {id} belongs to another user"
            );
        }

        return await CreateReportDTO(report);
    }

    [HttpGet("inventory/{id}")]
    public async Task<ActionResult<List<GetReportDTO>>> GetInventory(int id)
    {
        Inventory? inventory = await _db.Inventories.FindAsync(id);
        if (inventory is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find an inventory with id: {id}"
            );
        }

        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (inventory.UserId != currentUserId)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Inventory with id {inventory.Id} belongs to another user"
            );
        }

        var reports = await _db
            .Reports.AsNoTracking()
            .Include(r => r.Image)
            .Include(r => r.User)
            .Where(r => r.InventoryId == id && r.UserId == currentUserId)
            .ToListAsync();

        var reportDTOs = new List<GetReportDTO>();

        foreach (var report in reports)
        {
            reportDTOs.Add(await CreateReportDTO(report));
        }

        return reportDTOs;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetReportDTO>>> GetAll()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reports = await _db
            .Reports.AsNoTracking()
            .Include(r => r.Image)
            .Include(r => r.User)
            .Where(r => r.UserId == currentUserId)
            .ToListAsync();

        var reportDTOs = new List<GetReportDTO>();

        foreach (var report in reports)
        {
            reportDTOs.Add(await CreateReportDTO(report));
        }

        return reportDTOs;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateReportDTO reportDTO)
    {
        Inventory? inventory = await _db.Inventories.FindAsync(reportDTO.InventoryId);
        if (inventory is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find an inventory with id: {reportDTO.InventoryId}"
            );
        }

        if (inventory.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Inventory with id {inventory.Id} belongs to another user"
            );
        }

        bool productExists = await _db.Products.AnyAsync(p => p.Id == reportDTO.ProductId);
        if (!productExists)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find a product with id: {reportDTO.ProductId}"
            );
        }

        // TODO: add image validation
        string base64Data = reportDTO.ImageBase64.Substring(reportDTO.ImageBase64.IndexOf(",") + 1);
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        var filename = Guid.NewGuid().ToString() + Path.GetExtension(reportDTO.ImageFilename);
        var filepath = Path.Combine(_env.WebRootPath, "images", filename);
        await System.IO.File.WriteAllBytesAsync(filepath, imageBytes);

        byte[] processedBytes;
        int count = 0;
        try
        {
            (processedBytes, count) = _py.ProcessImage(filepath, 0, ".jpg");
            await System.IO.File.WriteAllBytesAsync(filepath, processedBytes);
        }
        catch (PythonException e)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Python error: {e.Message}"
            );
        }

        var image = new Image { Path = filepath };
        _db.Images.Add(image);
        await _db.SaveChangesAsync();

        var gf = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        var location = gf.CreatePoint(
            new Coordinate(
                double.Parse(reportDTO.Longitude, CultureInfo.InvariantCulture),
                double.Parse(reportDTO.Latitude, CultureInfo.InvariantCulture)
            )
        );

        var report = new Report
        {
            Location = location,
            Date = DateTime.UtcNow,
            WarehouseZone = reportDTO.WarehouseZone,
            ProductCount = count,
            ProductId = reportDTO.ProductId,
            ImageId = image.Id,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            InventoryId = inventory.Id,
        };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        return Ok(new { reportId = report.Id });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        Report? report = await _db
            .Reports.Include(r => r.Image)
            .Where(r => r.Id == id)
            .SingleAsync();

        if (report is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find a report with id {id}"
            );
        }

        if (report.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Report with id {id} belongs to another user"
            );
        }

        System.IO.File.Delete(report.Image.Path);
        _db.Images.Remove(report.Image);
        _db.Reports.Remove(report);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<GetReportDTO> CreateReportDTO(Report report)
    {
        var imageFilename = Path.GetFileName(report.Image.Path);
        // TODO: determine image extension programatically
        string imageBase64 =
            "data:image/jpeg;base64,"
            + Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(report.Image.Path));

        var reportDTO = new GetReportDTO
        {
            Id = report.Id,
            Longitude = report.Location.X.ToString(CultureInfo.InvariantCulture),
            Latitude = report.Location.Y.ToString(CultureInfo.InvariantCulture),
            Date = report.Date.ToLocalTime(),
            WarehouseZone = report.WarehouseZone,
            ProductCount = report.ProductCount,
            ProductId = report.ProductId,
            ImageFilename = imageFilename,
            ImageBase64 = imageBase64,
            UserEmail = report.User.Email,
            InventoryId = report.InventoryId,
        };

        return reportDTO;
    }
}

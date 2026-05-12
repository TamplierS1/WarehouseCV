using System.Globalization;
using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseCV.Data;
using WarehouseCV.Models;

namespace WarehouseCV.Controllers;

[ApiController]
[Authorize]
[Route("api/inventories")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public InventoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Inventory>>> GetAll()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Select is here only because of ToLocalTime
        return await _db
            .Inventories.Where(i => i.UserId == currentUserId)
            .Select(i => new Inventory
            {
                Id = i.Id,
                Date = i.Date.ToLocalTime(),
                UserId = i.UserId,
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult> Create()
    {
        var inventory = new Inventory
        {
            Date = DateTime.UtcNow,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        };

        _db.Inventories.Add(inventory);
        await _db.SaveChangesAsync();

        return Ok(new { inventoryId = inventory.Id });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        Inventory? inventory = await _db.Inventories.FindAsync(id);

        if (inventory is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find an inventory with id {id}"
            );
        }

        if (inventory.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Inventory with id {id} belongs to another user"
            );
        }

        var reports = await _db
            .Reports.Include(r => r.Image)
            .Where(r => r.InventoryId == id)
            .ToListAsync();

        foreach (Report report in reports)
        {
            System.IO.File.Delete(report.Image.Path);
            _db.Images.Remove(report.Image);
            _db.Reports.Remove(report);
        }

        _db.Inventories.Remove(inventory);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult> DownloadAsExcel(int id)
    {
        Inventory? inventory = await _db.Inventories.FindAsync(id);
        if (inventory is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find an inventory with id {id}"
            );
        }

        if (inventory.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Inventory with id {id} belongs to another user"
            );
        }

        // Fetch all reports for this inventory including the Product entity
        var reports = await _db
            .Reports.AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.InventoryId == id)
            .ToListAsync();

        // Group reports by product and sum the product count
        var productSummaries = reports
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                g.First().Product.Mark,
                g.First().Product.Name,
                g.First().Product.Designation,
                g.First().Product.Classifier,
                g.First().Product.ClassifierGroup,
                TotalDetected = g.Sum(r => r.ProductCount),
                ReportCount = g.Count(),
            })
            .OrderBy(s => s.ProductId)
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(1);

        var table = worksheet.Cell(1, 1).InsertTable(productSummaries);

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"inventory_{id}_{inventory.Date:yyyy-MM-dd_HH-mm}.xlsx"
        );
    }
}

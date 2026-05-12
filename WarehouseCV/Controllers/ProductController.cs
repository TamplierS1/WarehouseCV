using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseCV.Data;
using WarehouseCV.Models;

namespace WarehouseCV.Controllers;

public class CreateProductDTO
{
    public string Mark { get; set; }
    public string Name { get; set; }
    public string Designation { get; set; }
    public string Classifier { get; set; }
    public string ClassifierGroup { get; set; }
}

[ApiController]
[Authorize]
[Route("api/[controller]s")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        Product? product = await _db.Products.FindAsync(id);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find a product with id {id}"
            );
        }

        return product;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        return await _db.Products.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProductDTO productDTO)
    {
        var product = new Product
        {
            Mark = productDTO.Mark,
            Name = productDTO.Name,
            Designation = productDTO.Designation,
            Classifier = productDTO.Classifier,
            ClassifierGroup = productDTO.ClassifierGroup,
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return Ok(new { productId = product.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] CreateProductDTO productDTO
    )
    {
        Product? product = await _db.Products.FindAsync(id);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find a product with id {id}"
            );
        }

        _db.Entry(product).CurrentValues.SetValues(productDTO);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        Product? product = await _db.Products.FindAsync(id);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Could not find a product with id {id}"
            );
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadAsExcel(IFormFile file)
    {
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);

            using (var workbook = new XLWorkbook(stream))
            {
                // Loop through rows skipping header
                var rows = workbook.Worksheet(1).RangeUsed().RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var product = new Product
                    {
                        Mark = row.Cell("A").GetString(),
                        Name = row.Cell("B").GetString(),
                        Designation = row.Cell("C").GetString(),
                        Classifier = row.Cell("D").GetString(),
                        ClassifierGroup = row.Cell("E").GetString(),
                    };

                    _db.Products.Add(product);
                }
            }
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("download")]
    public async Task<ActionResult> DownloadAsExcel()
    {
        var products = await _db
            .Products.AsNoTracking()
            .Select(p => new
            {
                p.Mark,
                p.Name,
                p.Designation,
                p.Classifier,
                p.ClassifierGroup,
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(1);

        var table = worksheet.Cell(1, 1).InsertTable(products);

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "products.xlsx"
        );
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace WarehouseCV.Models;

public class Report
{
    public int Id { get; set; }

    [Column(TypeName = "geography (point, 4326)")]
    public Point Location { get; set; }
    public DateTime Date { get; set; }
    public string WarehouseZone { get; set; }
    public int ProductCount { get; set; }
    public int ProductId { get; set; }
    public int ImageId { get; set; }
    public string UserId { get; set; }
    public int InventoryId { get; set; }

    public Product Product { get; set; }
    public Image Image { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
    public Inventory Inventory { get; set; }
}

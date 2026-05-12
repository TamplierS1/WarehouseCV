using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseCV.Models;

public class Inventory
{
    public int Id { get; set; }

    public DateTime Date { get; set; }
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
    public List<Report> Reports { get; set; }
}

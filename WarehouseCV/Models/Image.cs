namespace WarehouseCV.Models;

public class Image
{
    public int Id { get; set; }
    public string Path { get; set; }

    public Report Report { get; set; }
}

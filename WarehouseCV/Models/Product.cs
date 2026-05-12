namespace WarehouseCV.Models;

public class Product
{
    public int Id { get; set; }

    public string Mark { get; set; }
    public string Name { get; set; }
    public string Designation { get; set; }
    public string Classifier { get; set; }
    public string ClassifierGroup { get; set; }

    public List<Report> Reports { get; set; }
}

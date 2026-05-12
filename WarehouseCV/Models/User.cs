using Microsoft.AspNetCore.Identity;

namespace WarehouseCV.Models;

public class User : IdentityUser
{
    public List<Report> Reports { get; set; }
}

using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public virtual ICollection<BillProduct> BillProducts { get; set; } = new List<BillProduct>();
}

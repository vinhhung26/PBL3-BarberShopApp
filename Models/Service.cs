using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public virtual ICollection<BillService> BillServices { get; set; } = new List<BillService>();
}

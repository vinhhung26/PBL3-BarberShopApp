using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class BillService
{
    public int BillId { get; set; }

    public int ServiceId { get; set; }

    public int BarberId { get; set; }

    public decimal Price { get; set; }

    public virtual Barber Barber { get; set; } = null!;

    public virtual Bill Bill { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}

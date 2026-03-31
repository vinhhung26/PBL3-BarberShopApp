using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class Barber
{
    public int BarberId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public bool? IsBusy { get; set; }

    public decimal? BasicSalary { get; set; }

    public double? CommissionRate { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<BillService> BillServices { get; set; } = new List<BillService>();
}

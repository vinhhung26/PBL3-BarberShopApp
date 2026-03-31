using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? CustomerId { get; set; }

    public int? BarberId { get; set; }

    public DateTime AppointmentTime { get; set; }

    public string? Status { get; set; }

    public virtual Barber? Barber { get; set; }

    public virtual Customer? Customer { get; set; }
}

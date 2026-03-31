using System;
using System.Collections.Generic;

namespace BarberShopApp.Models;

public partial class Customer
{
	public int CustomerId { get; set; }

	public string Phone { get; set; } = null!;

	public string? FullName { get; set; }

	public DateOnly? DateOfBirth { get; set; }

	public int? AccountId { get; set; }

	public int? RewardPoints { get; set; }

	public string? CustomerTier { get; set; }

	public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
	public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
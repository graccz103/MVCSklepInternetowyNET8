using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    public decimal TotalPrice { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

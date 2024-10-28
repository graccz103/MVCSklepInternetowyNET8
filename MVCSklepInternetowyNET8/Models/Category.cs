using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please enter the category name")]
    [StringLength(50)]
    public string Name { get; set; }

    public ICollection<Product> Products { get; set; }
}

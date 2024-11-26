using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Nazwa kategorii jest wymagana.")]
    [StringLength(100)]
    public string Name { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; } // Ustawienie jako nullable przez co jest opcjonalne

    public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    public ICollection<Product>? Products { get; set; } // też nullable
}


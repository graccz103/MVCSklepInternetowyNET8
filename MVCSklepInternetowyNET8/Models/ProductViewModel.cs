﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class ProductViewModel
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Please enter the product name")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public byte[]? LargeImage { get; set; }

    [Display(Name = "New Image (optional)")]
    public IFormFile? LargeImageFile { get; set; }

    public byte[]? Thumbnail { get; set; }
    public DateTime CreatedDate { get; set; }

    // Pola dotyczące promocji
    public bool IsOnPromotion { get; set; }

    [Display(Name = "Promotion End Date")]
    public DateTime? PromotionEndDate { get; set; }
}

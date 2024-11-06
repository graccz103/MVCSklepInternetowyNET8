using System;
using System.ComponentModel.DataAnnotations;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Please enter the customer's first name")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 50 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Please enter the customer's last name")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 50 characters")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Please enter a valid email address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please enter the address")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Please enter the city")]
    public string City { get; set; }

    [Required(ErrorMessage = "Please enter the postal code")]
    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Please enter a valid postal code in the format XX-XXX")]
    public string PostalCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;


    // Relacja do ApplicationUser
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
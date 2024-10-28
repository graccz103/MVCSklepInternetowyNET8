//to moze wymagac poprawy bo aktualnie jest domyślny string.Empty a to raczej złe rozwiązanie na dluzsza mete

using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; } = string.Empty;
}

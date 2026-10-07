using System.ComponentModel.DataAnnotations;

namespace AuthService.API.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del correo es inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener un mínimo de 8 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, un número y un carácter especial.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}

using Filoroch.Template.CrossCutting.Exceptions;
using System.Text.RegularExpressions;

namespace Filoroch.Template.CrossCutting.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public static string ValidarObrigatoria(
        this string? value,
        string propertyName,
        int? minLength = null,
        int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PropriedadeObrigatoriaException(propertyName);

        if (minLength is < 0 || maxLength is < 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "Os limites não podem ser negativos.");

        if (minLength.HasValue && maxLength.HasValue && minLength > maxLength)
            throw new ArgumentException("O tamanho mínimo não pode ser maior que o tamanho máximo.");

        string normalizedValue = value.Trim();

        if (minLength.HasValue && normalizedValue.Length < minLength.Value)
            throw new PropriedadeInvalidaException(
                propertyName,
                $"deve possuir no mínimo {minLength.Value} caracteres.");

        if (maxLength.HasValue && normalizedValue.Length > maxLength.Value)
            throw new PropriedadeInvalidaException(
                propertyName,
                $"deve possuir no máximo {maxLength.Value} caracteres.");

        return normalizedValue;
    }

    public static bool IsValidEmail(this string? value)
        => !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value.Trim());
}

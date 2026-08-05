namespace PlaywrightFramework.TestData;

/// <summary>
/// Dados de entrega. Mesmo motivo do User: três campos que sempre viajam juntos
/// não são três strings soltas — são um tipo.
/// </summary>
public record DeliveryInfo(string FirstName, string LastName, string PostalCode);

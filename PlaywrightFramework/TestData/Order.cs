namespace PlaywrightFramework.TestData;

/// <summary>
/// Um pedido completo: quem compra, o que compra, para onde vai.
///
/// É aqui que o dado vira COMBINATÓRIO — usuário × N produtos × entrega — e é por isso
/// que este objeto (e não o User, de 2 campos) justifica um Builder.
///
/// Imutável de propósito: depois de construído, ninguém altera. Quem monta é o OrderBuilder.
/// </summary>
public record Order(User User, IReadOnlyList<string> Products, DeliveryInfo Delivery);

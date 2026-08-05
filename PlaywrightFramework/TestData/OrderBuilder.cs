namespace PlaywrightFramework.TestData;

/// <summary>
/// Builder de pedidos.
///
/// A ideia inteira em uma frase: <b>tudo já vem válido; o teste declara só o desvio.</b>
///
///     new OrderBuilder().WithoutPostalCode().Build()
///
/// O teste acima diz "um pedido normal, mas sem CEP" — e não precisa saber que existe
/// nome, sobrenome, usuário ou produto. Se amanhã o checkout ganhar um quarto campo,
/// o default entra aqui e nenhum teste é tocado.
/// </summary>
public class OrderBuilder
{
    // ---- O padrão sensato mora nos campos. Cada método With/Without é um DESVIO dele. ----
    private const string DefaultProduct = "Sauce Labs Backpack";

    private User _user = UsersFactory.Standard();
    private readonly List<string> _products = new();
    private string _firstName = "Ana";
    private string _lastName = "Souza";
    private string _postalCode = "01310-100";

    public OrderBuilder ForUser(User user)
    {
        _user = user;
        return this;              // devolver 'this' é o que torna a chamada encadeável
    }

    public OrderBuilder WithProduct(string productName)
    {
        _products.Add(productName);
        return this;
    }

    public OrderBuilder WithDeliveryName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public OrderBuilder WithPostalCode(string postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    // Métodos "negativos" nomeiam a INTENÇÃO do teste.
    // 'WithoutPostalCode()' diz por que o teste existe; 'WithPostalCode("")' só diz o que faz.
    public OrderBuilder WithoutPostalCode() => WithPostalCode("");

    public OrderBuilder WithoutFirstName() => WithDeliveryName("", _lastName);

    public Order Build()
    {
        // Um pedido sem produto nenhum não é um default válido — se o teste não escolheu,
        // ele ganha o produto padrão.
        var products = _products.Count == 0
            ? new List<string> { DefaultProduct }
            : new List<string>(_products);   // cópia: Build() duas vezes não compartilha lista

        return new Order(_user, products, new DeliveryInfo(_firstName, _lastName, _postalCode));
    }
}

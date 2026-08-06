using Microsoft.Playwright;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Flows;

/// <summary>
/// A jornada de compra, em PARADAS NOMEADAS.
///
/// Cada método leva até um marco real do negócio e devolve o controle para o teste.
/// Nada de CheckoutAsync(order, ateOPasso: 2) ou (order, finalizar: false) — parâmetro que
/// muda o comportamento é flag argument, e quem lê a chamada não faz ideia do que significa.
///
/// Regras que este fluxo respeita:
/// - cobre o Arrange, nunca o Act (o Act é do teste);
/// - não faz Assert (mesma regra do Page Object);
/// - não tem 'if' (um if aqui quase sempre significa que são dois fluxos);
/// - conhece Page Objects — e nunca o contrário.
/// </summary>
public class CheckoutFlow(IPage page)
{
    private readonly LoginFlow _loginFlow = new(page);   // fluxo composto de fluxo: desejável
    private readonly ProductsPage _productsPage = new(page);
    private readonly CartPage _cartPage = new(page);
    private readonly CheckoutInformationPage _informationPage = new(page);
    private readonly CheckoutOverviewPage _overviewPage = new(page);

    /// <summary>Entra, põe os produtos do pedido no carrinho e para na tela de entrega.</summary>
    public async Task GoToDeliveryInformationAsync(Order order)
    {
        await _loginFlow.SignInAsAsync(order.User);

        foreach (var product in order.Products)
        {
            await _productsPage.AddToCartAsync(product);
        }

        await _productsPage.GoToCartAsync();
        await _cartPage.StartCheckoutAsync();
    }

    /// <summary>O anterior + preenche a entrega e avança para o resumo do pedido.</summary>
    public async Task GoToOrderSummaryAsync(Order order)
    {
        await GoToDeliveryInformationAsync(order);
        await _informationPage.FillInformationAsync(order.Delivery);
        await _informationPage.ContinueAsync();
    }

    /// <summary>O anterior + confirma. A jornada completa: pedido feito.</summary>
    public async Task PlaceOrderAsync(Order order)
    {
        await GoToOrderSummaryAsync(order);
        await _overviewPage.FinishAsync();
    }
}

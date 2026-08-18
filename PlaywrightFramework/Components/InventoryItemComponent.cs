using Microsoft.Playwright;

namespace PlaywrightFramework.Components;

/// <summary>
/// O card de um produto na vitrine. Aqui o locator raiz deixa de ser detalhe e vira o
/// motivo do padrão existir: o card aparece 6 vezes na tela, e sem raiz não dá para dizer
/// "o preço DESTE card".
///
/// Efeito colateral bem-vindo: dentro da raiz, o botão é só "o botão de adicionar".
/// Sumiu a gambiarra que montava o seletor a partir do nome do produto em kebab-case.
/// </summary>
public class InventoryItemComponent(ILocator root)
{
    private ILocator AddButton => root.Locator("[data-test^='add-to-cart']");
    private ILocator RemoveButton => root.Locator("[data-test^='remove']");

    /// <summary>Estado exposto: o nome do produto.</summary>
    public ILocator Name => root.Locator("[data-test='inventory-item-name']");

    /// <summary>Estado exposto: o preço.</summary>
    public ILocator Price => root.Locator("[data-test='inventory-item-price']");

    /// <summary>Estado exposto: a descrição.</summary>
    public ILocator Description => root.Locator("[data-test='inventory-item-desc']");

    // Adicionar e remover usam o MESMO botão na tela (ele alterna). Ainda assim são dois
    // métodos, porque cada um mira o estado real do controle: chamar RemoveFromCartAsync
    // num item que não está no carrinho falha na hora, em vez de fazer o contrário calado.
    public async Task AddToCartAsync()
    {
        await AddButton.ClickAsync();
    }

    public async Task RemoveFromCartAsync()
    {
        await RemoveButton.ClickAsync();
    }
}

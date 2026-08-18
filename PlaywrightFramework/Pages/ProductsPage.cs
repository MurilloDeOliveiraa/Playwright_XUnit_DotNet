using Microsoft.Playwright;
using PlaywrightFramework.Components;

namespace PlaywrightFramework.Pages;

/// <summary>
/// POM da vitrine de produtos.
///
/// Repare no que SOBROU aqui depois dos componentes: só o que é exclusivo desta tela.
/// O cabeçalho virou componente; o card de produto virou componente. O dropdown de
/// ordenação ficou — ele mora dentro da faixa do cabeçalho, mas só existe nesta tela,
/// e é justamente por isso que ele NÃO entrou no HeaderComponent.
/// </summary>
public class ProductsPage(IPage page)
{
    private ILocator ItemCards => page.Locator(".inventory_item");

    /// <summary>O cabeçalho compartilhado (menu, carrinho, título da tela).</summary>
    public HeaderComponent Header { get; } = new(page);

    /// <summary>Estado exposto: o seletor de ordenação — exclusivo desta tela.</summary>
    public ILocator SortDropdown => page.Locator("[data-test='product-sort-container']");

    /// <summary>
    /// Devolve o card de um produto pelo nome. É a fábrica de componentes desta tela:
    /// quem chama recebe um objeto ancorado NAQUELE card e só enxerga o que há dentro dele.
    /// </summary>
    public InventoryItemComponent ItemNamed(string productName)
    {
        // Aqui eu estou passando qual o valor do locator root do InventoryItemComponent
        // No caso, esse locator vai apontar pro produto com o nome que eu quero
        return new InventoryItemComponent(ItemCards.Filter(new LocatorFilterOptions
        {
            HasText = productName
        }));
    }
}

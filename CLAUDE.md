# CLAUDE.md — Web Automation Framework (Playwright + .NET + xUnit)

> Arquivo de contexto para o **Claude Code**, na raiz do projeto.
> Resume um estudo em andamento sobre **como construir um framework de automação web de nível QA Sênior**, do zero, com foco em *entender o porquê* de cada decisão.

---

## 1. Contexto do aprendizado (leia antes de responder)

- **Objetivo**: aprender a projetar e construir um framework de automação como um QA Sênior/Especialista — não só copiar código.
- **Perfil do aluno**: voltando a estudar. C# em nível **básico** (sintaxe ok, ainda ganhando confiança com `async/await`). Já trabalha com **xUnit**.
- **Como ensinar** (o aluno pediu explicitamente):
  - **Conceito primeiro, código depois.**
  - **Dor antes do padrão**: nunca "decore este padrão". Sempre mostrar a *dor concreta* que motiva o padrão, e só então introduzi-lo.
  - **Mentor, não curso**: explicar *tradeoffs* (quase nada é "certo/errado", é "custa X, dá Y"). Propor exercícios/diagnósticos antes de dar a solução.
  - Tom direto, sem jargão.

### ⚠️ Fluxo obrigatório de cada tópico — três turnos separados

1. **Mostrar a dor.** Apontar no código existente o que dói, explicar o conceito e os tradeoffs. Termina aqui — **nenhum arquivo é criado ou editado neste turno.**
2. **Esperar as dúvidas do aluno** e respondê-las.
3. **Perguntar "posso implementar?"** e só então escrever código.

Implementar junto com a explicação tira do aluno o espaço de raciocinar antes de ver a resposta pronta. Isso já aconteceu uma vez e foi revertido.

A nota do vault e o roadmap só são atualizados **depois** que o aluno valida o resultado.

---

## 2. Stack e convenções (SIGA em todo código novo)

**.NET 8** · **xUnit v3** · **Microsoft.Playwright** · site de treino **SauceDemo** (`standard_user` / `secret_sauce` — credenciais públicas de site de treino).

### Camadas e dependências

```
Teste       → o QUE se quer           usa Flow e Page Object
Flow        → a JORNADA               usa Page Object · nunca o contrário
Page Object → a MECÂNICA de tela      usa IPage e Component
Component   → pedaço de UI ancorado   usa um locator raiz
```

### Fixtures e isolamento
- Browser é caro → sobe **1x para a suíte** via `ICollectionFixture<BrowserFixture>` (`IAsyncLifetime`).
- Context + Page são baratos → **1x por teste**, em `BaseTest : IAsyncLifetime`.
- Regra-mãe: **um browser por execução, um context por teste.**
- **Nunca** limpeza manual de estado; **nunca** campo `static` ou estado compartilhado entre testes.

### Page Object
- Seletores **privados**; ações **públicas** com nome de negócio.
- POM **não faz Assert** — expõe *estado* (um `ILocator`) para o teste verificar.
- "Uma classe por pedaço coeso de UI", não "uma classe por URL".
- **POM não guarda estado.** Locators sempre com `=>` (recriados a cada uso), nunca `=`.

### Component Object
- Ancorado num **locator raiz**; toda busca acontece dentro dele.
- Duas formas, e o critério é objetivo — *"existe mais de um deste componente na mesma tela?"*:
  - **aparece 1x por tela** → recebe `IPage` e declara a própria raiz (ex.: `HeaderComponent`);
  - **se repete na tela** → recebe `ILocator root` de quem sabe qual é (ex.: `InventoryItemComponent`).
- Cada página expõe **só os componentes que realmente tem** (`LoginPage` não tem `Header`).

### Flow (camada de fluxos de negócio)
- Cobre o **Arrange**, nunca o **Act**.
- Paradas **nomeadas**, nunca parâmetro que muda o comportamento (`ateOPasso: 2` é proibido).
- **Não dá veredito** sobre o comportamento testado · não tem `if` (um `if` quase sempre significa dois fluxos).
- **Garante a própria pós-condição** com `Assertions.Expect` (não `WaitForAsync`): se o fluxo falha, o problema é "não montei o cenário", não "o sistema tem defeito". Ex.: `LoginFlow` confere que chegou em `Products`. *(Refina a regra antiga "fluxo não faz Assert", que estava larga demais.)*
- `CheckoutFlow` ainda **não** confere as pós-condições das paradas — dívida adiada, gatilho: verificação negativa logo depois de uma parada do checkout.
- Não criar fluxo para uma única chamada de POM.
- **Nunca teste um componente através da abstração que existe para escondê-lo** — por isso `LoginTests` usa `LoginPage` direto, e não `LoginFlow`.

### Dados de teste
- **Factory** (`UsersFactory.Standard()`) para poucas variações nomeáveis.
- **Builder** (`new OrderBuilder().WithoutPostalCode().Build()`) só quando o dado é combinatório. Builder para objeto de 2–3 campos é over-engineering.
- Tipos de domínio em vez de strings soltas (`User`, `DeliveryInfo`, `Order`).
- Pasta é `TestData/` — **`Helpers/` e `Utils/` são proibidos** (significam "não soube onde isso morava").

### Composição
- `BaseTest` é o *composition root*: expõe páginas e fluxos como propriedades `=>` (construção adiada; a `Page` só existe depois do `InitializeAsync`).
- Testes não dão `new` em encanamento. O `OrderBuilder` **continua** visível no Arrange — ele é dado do teste, não encanamento.
- **Sem container de injeção de dependência** — todo objeto depende só de `IPage`. O gatilho para reavaliar está na nota 09.

### Configuração
- O que muda conforme quem roda e onde fica em `appsettings.json` + `TestSettings` — **nunca** cravado no código.
- Ordem das fontes: `appsettings.json` < User Secrets < variável de ambiente (prefixo `SAUCEDEMO_`).
- Linha de comando **não** é usada: o `dotnet test` levanta outro processo e os argumentos não chegam. Não tentar "consertar".
- Segredo nunca no repositório. `dotnet user-secrets` já está ligado (`UserSecretsId` no csproj).
- Pacotes `Microsoft.Extensions.*` fixados na **8.0.0** para acompanhar o `net8.0`.
- **Navegador:** `SAUCEDEMO_Browser=Chromium|Firefox|Webkit`, uma execução por navegador (o pipeline cria uma para cada). É uma lista fechada (`BrowserKind`).
- **Valor padrão serve para a AUSÊNCIA de escolha, nunca para disfarçar um erro.** Valor desconhecido faz a execução parar — **não** cai no Chromium. Um erro que se disfarça de sucesso é pior que um que aparece.
- `switch` sobre `BrowserKind` **sem ramo padrão** (o `_ => throw` é de propósito).
- `DisposeAsync` do fixture só fecha o que foi criado — senão uma falha na subida gera um segundo erro que esconde o verdadeiro.

### Estabilidade (flakiness)
- **Nenhuma repetição automática de teste.** Se um dia entrar, tem que ser **contada e visível** (passou na 3ª = falhou 2 vezes).
- **Modo de estresse:** `SAUCEDEMO_CpuThrottle=4` (depois `6`) deixa o *sistema* lento — só Chromium; em outro navegador a execução **para** com mensagem. Desligado por padrão (`1`).
- **`SlowMo` não serve para caçar flakiness:** atrasa o teste e *esconde* as corridas de tempo.
- 0 falhas em *n* rodadas só garante falha menor que ~3/*n*. Nunca dá para provar zero.
- Olhar **variação do tempo**, não só a média; medir pelo **efeito** (falhas), não por instrumento dentro da página lenta.

### Observabilidade (evidência na falha)
- **xUnit v3** (migrado no tópico 16): o `DisposeAsync` lê `TestContext.Current.TestState!.Result == TestResult.Failed`. `IAsyncLifetime` usa `ValueTask`; `ITestOutputHelper` não precisa de `using Xunit.Abstractions`.
- **`global.json` na raiz é obrigatório:** o SDK .NET 10 só roda xUnit v3 pelo modo novo do `dotnet test`. Não apagar. O pipeline precisa do mesmo.
- **Screenshot e vídeo só quando o teste falha.** Screenshot tirado na hora; vídeo gravado em todos e **apagado** nos que passam (não dá para filmar o que já passou).
- **Screenshot ANTES de fechar o context; vídeo DEPOIS** (é no fechamento que ele termina de gravar).
- Tirar evidência **nunca pode esconder a falha do teste**: `TryTakeScreenshotAsync` engole a falha do próprio screenshot.
- Pasta: `test-results/<execução>/<navegador>/<teste>/`, onde a execução é o `RunId` (`yyyyMMdd-HHmmss`, tirado 1x no `BrowserFixture`). **Teste verde nunca apaga evidência de outra execução** — `PrepareVideoDirectory` não apaga nada. Hora por teste (e até o minuto) foi tentada e falha: colide.
- **Retenção:** ao subir o `BrowserFixture`, `DeleteOldRuns` apaga execuções com mais de `EvidenceRetentionDays` (padrão 7; `0` = nunca). Idade vem do **nome** da pasta; pasta sem o formato é **ignorada**; falha ao apagar **não derruba** a suíte (é manutenção, não veredito).
- `RunIdFormat` é uma constante única: serve para criar e para ler o nome.
- **Logs de passos adiados** (aluno escolheu só screenshot e vídeo). Trace → tópico 17.
- `DeleteEmptyParents` é seguro só porque os testes rodam um de cada vez: com paralelismo o `Delete` precisa tolerar `IOException`.

### Testes e asserções
- **AAA**, um comportamento por teste, sem encanamento.
- **Escada de seletores:** `data-test` > papel + nome acessível (`GetByRole`) > texto/label > CSS (último recurso).
- **Nunca posição** (`nth-child`): layout muda mais que texto, e o texto ainda tem controle (`Locale` no context).
- Locator sem `data-test` entra **curto, com comentário e como dívida** ("pedir `data-test` ao time"). Mudar o locator conserta o sintoma; pedir o `data-test` muda a causa.
- `GetByTestId` + `SetTestIdAttribute("data-test")` foi **adiado de propósito** (hoje `"[data-test='...']"` aparece 27x). Gatilho: o atributo mudar de nome.
- Asserções **web-first**: `Assertions.Expect(locator).ToHaveTextAsync(...)` — esperam sozinhas, sem `sleep`. **Nunca** `Task.Delay`/`Thread.Sleep`, nunca ler o valor e comparar (`TextContentAsync` + `Assert.Equal`).
- Esperam uma **condição**, não um tempo: ações até 30 s, verificações até 5 s (padrões do Playwright).
- **Verificação negativa (`Not.ToBeVisibleAsync`) precisa de uma positiva antes**, que prove que está na tela certa — senão passa por motivo errado (a tela pode nem ter carregado). Âncora = o **título** da tela, não o cabeçalho.
- Antes de aumentar um timeout, descobrir se o elemento **nunca** aparece (defeito, o limite maior não ajuda) ou aparece tarde (lentidão real, aí sim).
- Regra do async: **`await` sempre, `.Result`/`.Wait()` nunca.**

### Um princípio que já apareceu três vezes
**Modele a realidade no tipo e deixe o compilador ser o primeiro revisor.**
3 classes de checkout (e não 1) · fluxo com pós-condição honesta · `loginPage.Header` que não compila.

### Um segundo princípio, também recorrente
**Um erro que aparece é melhor que um sucesso que mente.** O teste que fica verde sem provar nada é o pior defeito de uma suíte.
`storageState` mudo (10) · `firefx` caindo no Chromium (12) · negativa passando com o login falhando (13).

---

## 3. Estrutura atual do projeto

```
WebAutomationFramework.sln
global.json                       # modo novo do 'dotnet test' (xUnit v3 no SDK .NET 10) — obrigatório
PlaywrightFramework/
  PlaywrightFramework.csproj      # net8.0, xUnit v3 (OutputType Exe) + Microsoft.Playwright
  appsettings.json                # endereço, headless, slow-mo — sem segredo
  BaseTest.cs                     # context+page novos por teste + composition root + estresse + evidência na falha
  Configuration/
    TestSettings.cs               # tipo + pilha de fontes + CpuThrottle + EvidenceDirectory/RetentionDays
    BrowserKind.cs                # lista fechada: Chromium, Firefox, Webkit
  Fixtures/
    BrowserFixture.cs             # browser 1x para a suíte + config 1x + navegador + RunId + retenção
    PlaywrightCollection.cs       # ICollectionFixture — a cola do xUnit
  Evidence/
    FailureEvidence.cs            # screenshot + vídeo só na falha; pasta por execução; retenção
  Components/
    HeaderComponent.cs            # menu, carrinho, badge, título — 1x por tela
    InventoryItemComponent.cs     # card de produto — repete 6x, recebe a raiz
  Pages/
    LoginPage.cs                  # sem Header, de propósito
    ProductsPage.cs               # fábrica de InventoryItemComponent + sort (exclusivo)
    CartPage.cs
    CheckoutInformationPage.cs    # passo 1 — dados de entrega
    CheckoutOverviewPage.cs       # passo 2 — resumo
    CheckoutCompletePage.cs       # passo 3 — confirmação
  Flows/
    LoginFlow.cs                  # jornada "entrar no sistema" — confere que chegou em Products
    CheckoutFlow.cs               # jornada de compra em 3 paradas nomeadas
  TestData/
    User.cs · UsersFactory.cs     # Factory
    DeliveryInfo.cs · Order.cs · OrderBuilder.cs   # Builder
  Tests/
    LoginTests.cs                 # login é o ALVO → usa LoginPage direto
    CartTests.cs                  # prova de isolamento; login é pré-condição → LoginFlow
    CheckoutTests.cs              # pedido ok / sem CEP / badge no checkout / dois produtos
```

**9 testes verdes** contra o SauceDemo, nos três navegadores.

Rodar: `dotnet build` → `dotnet test` (precisa do `global.json`). Outro navegador: `SAUCEDEMO_Browser=firefox dotnet test`. Filtrar um teste: `dotnet test --filter-method "*NomeDoTeste*"`. Evidência das falhas: `bin/Debug/net8.0/test-results/`.

**Instalar o browser nesta máquina**: o comando oficial (`pwsh bin/Debug/net8.0/playwright.ps1 install`) **falha aqui** — o PowerShell está em *Constrained Language Mode* e o script não carrega o assembly. Contornar chamando o CLI do Playwright pelo Node que vem no próprio pacote:

```
.\PlaywrightFramework\bin\Debug\net8.0\.playwright\node\win32_x64\node.exe .\PlaywrightFramework\bin\Debug\net8.0\.playwright\package\cli.js install chromium firefox webkit
```

---

## 4. O que já foi aprendido (concluído)

O número é **identificador** (casa com o nome da nota no vault), não ordem de estudo.

| # | Tópico | Ideia central |
|---|---|---|
| 1 | O que é um framework | máquina de gerenciar mudança. *Uma mudança no app toca o mínimo de código — idealmente um lugar.* |
| 2 | Async/await | `await` esquecido e `.Result` geram flakiness; o auto-waiting do Playwright esconde o bug |
| 3 | Anatomia de um teste (AAA) | um comportamento por teste; o teste lê como negócio |
| 4 | Page Object Model | intenção pública / mecânica privada; POM não asserta; POM = UI coesa |
| 5 | Fixtures e isolamento | três camadas do Playwright; `IAsyncLifetime`; um browser por execução, um context por teste |
| 6 | Factory e Builder | Factory = padrão sensato + override; Builder = declare só o desvio; Builder para 2 campos é exagero |
| 10 | Camada de fluxos de negócio | o teste nomeia a jornada em vez de narrá-la; fluxo cobre o Arrange, nunca o Act |
| 8 | Component Objects | locator raiz; composição e não herança (`BasePage` é armadilha); cada página expõe o que tem |
| 9 | DI e composição | *composition root*; duas instâncias de POM não são problema — a invariante é **POM sem estado** |
| 11 | Config por ambiente + secrets | quanto mais específica e passageira a fonte, mais alto ela ganha; quem lê a config não sabe de onde ela veio |
| 12 | Múltiplos browsers | uma execução por navegador (respeita a regra-mãe); padrão é para a ausência de escolha, não para disfarçar erro |
| 13 | Auto-waiting e web-first assertions | espera **condição**, não tempo; verificação negativa precisa de positiva antes; fluxo garante a própria pós-condição |
| 14 | Combate à flakiness | alargar a janela do azar (estresse no *sistema*); repetição só se contada; 0 falhas em *n* rodadas ≈ falha < 3/*n* |
| 15 | Estratégia de seletores | escada `data-test` > papel > texto > CSS; estrutura é o mais frágil; **sem código novo, de propósito** (decisão consciente) |
| 16 | Screenshot e vídeo na falha | evidência só na falha; migração para xUnit v3 para ler o resultado do teste; pasta por execução (verde nunca apaga vermelho); retenção de 7 dias |

**Ordem real percorrida:** 1 → 2 → 3 → 4 → 5 → 6 → 10 → 8 → 9 → 11 → 12 → 13 → 14 → 15 → 16. A ordem muda de propósito, sempre atrás da dor que está doendo no código.

---

## 5. Roadmap

> O roadmap **oficial** vive no vault do Obsidian, não aqui. Não criar `ROADMAP.md` no repositório.
> `Roadmap - Web Automation Framework.md` — diagrama Mermaid, tabela de ordem de execução, "Você está aqui" e a tabela de **dívidas conscientes** (coisas erradas de propósito, cada uma ligada à fase que a resolve).

**PRÓXIMO PASSO → 17. Trace Viewer** (segundo da fase Observabilidade).
A dor a mostrar: screenshot e vídeo mostram *como a tela estava* e *como chegou até ali*, mas nenhum mostra o DOM, a rede e cada passo. O trace guarda isso. Pontos para o aluno decidir: gravar em todos e guardar só na falha (como o vídeo)? Onde ele entra na pasta de evidência e na retenção? Lembrar: mostrar a dor, parar, esperar as dúvidas, e só então perguntar se pode implementar.

**Adiados** (SauceDemo não tem o que o tópico precisa; retomar quando houver):
- tópico 7, Gestão de massa de dados — precisa de banco ou API para preparar e limpar dados;
- perfis de ambiente (`appsettings.Homologacao.json`), parte "ambientes" do tópico 12 — precisa de um segundo ambiente real;
- pós-condição das paradas do `CheckoutFlow` — gatilho na seção Flow acima;
- `GetByTestId` / `SetTestIdAttribute` — gatilho na seção Testes e asserções;
- logs de passos — gatilho: falha que screenshot, vídeo e trace não expliquem.

**A fazer:** Observabilidade (trace viewer, report — inclui mostrar o navegador na saída do teste) · Escala (paralelismo, CI/CD, tags smoke/regression, pirâmide de testes) · Qualidade do framework (anti-patterns, boas práticas).

---

## 6. Sistema de notas (Obsidian)

`C:\ObsidianVaults\Murillo's Vault\QA-SET\Projetos Pessoais\Web Automation Framework`

**Toda nota desta pasta leva a tag `webAutomationFramework`** (primeira da lista `tags:` do frontmatter), inclusive o índice e o roadmap. Nota nova já nasce com ela.

Uma nota por tópico (`NN - Título.md`), mesmo estilo: **dor → conceito → código → finezas de sênior → tradeoff honesto → "No projeto"**. Mais o índice (`00 - Índice`) e o `Roadmap`.

Ao fechar um tópico (depois da validação do aluno): criar/atualizar a nota, adicionar o link no índice, e atualizar no roadmap o diagrama, a tabela de ordem, o "Você está aqui", a árvore de arquivos e as dívidas.

> ⚠️ Arquivos do vault com **acento no nome** (ex.: `00 - Índice - ...md`) falham nas ferramentas Read/Write/Edit. Ler e escrever esses via Bash/PowerShell.

---

## 7. Prévias já plantadas no código (não são o foco ainda)

- Tudo numa única coleção serializa os testes → **paralelismo** (fase Escala).
- `storageState` para reaproveitar login → otimização futura. ⚠️ No dia em que entrar no `LoginFlow`, o `LoginTests` **não pode** passar a usar o fluxo — ele deixaria de exercitar a tela de login e continuaria verde.
- `BaseTest` acumula ciclo de vida **e** fábrica de objetos → revisitar se passar de ~15 propriedades (extrair um composition root próprio).

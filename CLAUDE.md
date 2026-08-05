# CLAUDE.md — Web Automation Framework (Playwright + .NET + xUnit)

> Arquivo de contexto para o **Claude Code**. Coloque na **raiz do projeto** `PlaywrightFramework/`.
> Ele resume um estudo em andamento sobre **como construir um framework de automação web de nível QA Sênior**, do zero, com foco em *entender o porquê* de cada decisão.

---

## 1. Contexto do aprendizado (leia antes de responder)

- **Objetivo**: aprender a projetar e construir um framework de automação como um QA Sênior/Especialista — não só copiar código.
- **Perfil do aluno**: voltando a estudar. C# em nível **básico** (sintaxe ok, ainda ganhando confiança com `async/await`). Já trabalha com **xUnit**.
- **Como ensinar** (importante — o aluno pediu explicitamente):
  - **Conceito primeiro, código depois.**
  - **Dor antes do padrão**: nunca "decore este padrão". Sempre mostrar a *dor concreta* que motiva o padrão, e só então introduzi-lo.
  - **Mentor, não curso**: explicar *tradeoffs* (quase nada é "certo/errado", é "custa X, dá Y"). Propor exercícios/diagnósticos antes de dar a solução.
  - Manter o tom direto e sem encher de jargão.

---

## 2. Stack e convenções (SIGA em todo código novo)

- **.NET 8**, **xUnit**, **Microsoft.Playwright**. Site de treino: **SauceDemo** (`https://www.saucedemo.com`, `standard_user` / `secret_sauce`).
- **Fixtures / isolamento**:
  - Browser é caro → sobe **1x para a suite** via `ICollectionFixture<BrowserFixture>` (implementa `IAsyncLifetime`).
  - Context + Page são baratos → **1x por teste** via classe-base `BaseTest : IAsyncLifetime` (`InitializeAsync`/`DisposeAsync`).
  - Regra-mãe: **um browser por execução, um context por teste.**
  - **Nunca** limpeza manual de estado; **nunca** campo `static`/estado compartilhado entre testes.
- **Page Object Model**:
  - Seletores **privados**; ações **públicas** com nome de negócio (expõe intenção, esconde mecânica).
  - Page Object **não faz Assert** — expõe *estado* (ex.: um `ILocator`) para o teste verificar.
  - "Uma classe por pedaço coeso de UI", não "uma classe por URL".
- **Testes**: estrutura **AAA** (Arrange/Act/Assert), **um comportamento por teste**, sem encanamento (a fixture cuida do setup/teardown).
- **Seletores**: preferir `data-test` (mais estáveis que classe CSS). *(Aprofundar na fase 5.)*
- **Asserções**: **web-first** via `Expect(locator).ToHaveTextAsync(...)` etc. — esperam sozinhas, sem `sleep`, sem flakiness. Regra do async: **`await` sempre, `.Result`/`.Wait()` nunca.**

---

## 3. Estrutura atual do projeto

```
WebAutomationFramework.sln
PlaywrightFramework/
  PlaywrightFramework.csproj      # net8.0, xUnit + Microsoft.Playwright
  BaseTest.cs                     # classe-base: context + page NOVOS por teste (IAsyncLifetime)
  Fixtures/
    BrowserFixture.cs             # sobe o browser 1x (IAsyncLifetime)
    PlaywrightCollection.cs       # [CollectionDefinition] liga o BrowserFixture à coleção
  Pages/
    LoginPage.cs                  # POM da tela de login
    ProductsPage.cs               # POM da tela de produtos (expõe CartBadge p/ asserção)
  Tests/
    LoginTests.cs                 # login válido / senha errada
    CartTests.cs                  # DEMONSTRA isolamento (par de testes que colidiria sem context novo)
```

Rodar: `dotnet restore` → `dotnet build` → instalar o browser → `dotnet test`.

**Instalar o browser nesta máquina**: o comando oficial (`pwsh bin/Debug/net8.0/playwright.ps1 install`)
falha aqui — o PowerShell está em *Constrained Language Mode* e o script não consegue carregar o assembly.
Contornar chamando o CLI do Playwright pelo Node que vem no próprio pacote:

```
.\PlaywrightFramework\bin\Debug\net8.0\.playwright\node\win32_x64\node.exe .\PlaywrightFramework\bin\Debug\net8.0\.playwright\package\cli.js install chromium
```

---

## 4. O que já foi aprendido (concluído)

1. **O que é um framework** — máquina de gerenciar mudança/complexidade. Camadas (teste / negócio / page objects / infra / transversais). Regra de ouro: *uma mudança no app toca o mínimo de código — idealmente um lugar*.
2. **Async/await** — modelo mental (a "lanchonete"), `Task`/`await`/`async`. Erros que geram flakiness: `await` esquecido e `.Result`. Armadilha do auto-waiting do Playwright (esconde bug de `await` faltando).
3. **Anatomia de um teste (AAA)** — um comportamento por teste; o teste lê como negócio, sem seletor/clique.
4. **Page Object Model** — derivado da dor dos "20 testes com seletor copiado". Intenção pública / mecânica privada. Três finezas de sênior (seletores privados, POM não asserta, POM = UI coesa).
5. **Fixtures e isolamento (xUnit)** — dor do estado compartilhado; três camadas do Playwright; xUnit cria instância nova por teste (isolamento estrutural, sem a pegadinha de reuso do NUnit); `IClassFixture`/`ICollectionFixture` vs `IAsyncLifetime`. Exemplo prático rodável contra o SauceDemo.

---

## 5. Roadmap — onde estamos

Concluído: Fundamentos (1–3) · Page Object (4) · **Fixtures e isolamento (5)**.

**PRÓXIMO PASSO → Factory e Builder (dados de teste).**
Dor que abre o tópico: o `Arrange` vira um monstro quando precisa de variação de dados (`standard_user`, `locked_out_user`, usuário com carrinho cheio, pedido com N itens). Factory (padrão sensato + overrides) e Builder (`new UserBuilder().WithEmail(...).Build()`) resolvem legibilidade e manutenção.

Fases seguintes (a fazer): Component Objects · DI/composição · camada de fluxos de negócio · **Config/ambiente** · **Confiabilidade** (auto-waiting, web-first assertions, estratégia de seletores) · **Observabilidade** (logs, trace viewer, report) · **Escala** (paralelismo, CI/CD, pirâmide de testes) · **Qualidade do framework** (anti-patterns — ex.: pasta `Helpers` é cheiro de código).

---

## 6. Sistema de notas (Obsidian)

O aluno mantém notas de estudo em um vault do Obsidian (uma nota por tópico, com wikilinks e um roadmap em Mermaid):
`C:\ObsidianVaults\Murillo's Vault\QA-SET\Projetos Pessoais\Web Automation Framework`

> Diferente do chat, o **Claude Code roda localmente e pode escrever direto nessa pasta**. Ao fechar cada novo tópico, gere/atualize a nota `.md` correspondente (mesmo estilo: dor → conceito → código → finezas de sênior) e atualize o `Roadmap`.

---

## 7. Prévias já plantadas no código (não são o foco ainda)

- `data-test` selectors → tópico **estratégia de seletores** (fase Confiabilidade).
- `Expect(...).ToHaveTextAsync/ToBeVisibleAsync` → **web-first assertions** (fase Confiabilidade).
- Tudo numa única coleção serializa os testes → **paralelismo** (fase Escala).
- `storageState` para reaproveitar login → otimização futura.

namespace PlaywrightFramework.TestData;

/// <summary>
/// Factory de usuários: o único lugar do projeto que sabe QUAIS são os usuários do SauceDemo.
///
/// Cada método é uma variação NOMEADA — o teste pede intenção ("um usuário bloqueado"),
/// não dados ("a string locked_out_user").
/// </summary>
public static class UsersFactory
{
    // O padrão sensato mora aqui, uma vez só.
    private const string DefaultPassword = "secret_sauce";

    /// <summary>O usuário feliz: loga e tudo funciona. É o default de 90% dos testes.</summary>
    public static User Standard() => new("standard_user", DefaultPassword);

    /// <summary>Usuário bloqueado pelo sistema — não consegue entrar.</summary>
    public static User LockedOut() => new("locked_out_user", DefaultPassword);

    /// <summary>Usuário com bugs de UI propositais (imagens trocadas, campos que não preenchem).</summary>
    public static User Problem() => new("problem_user", DefaultPassword);

    /// <summary>Usuário em que o app responde devagar — útil pra testar espera/timeout.</summary>
    public static User PerformanceGlitch() => new("performance_glitch_user", DefaultPassword);

    /// <summary>
    /// Override sobre o padrão: mesmo usuário, senha errada.
    /// A expressão 'with' copia o record trocando só o que você pediu — este é o
    /// "padrão sensato + override" que define uma Factory.
    /// </summary>
    public static User WithWrongPassword() => Standard() with { Password = "senha_errada" };
}

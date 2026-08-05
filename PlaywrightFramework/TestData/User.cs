namespace PlaywrightFramework.TestData;

/// <summary>
/// Um usuário do sistema. Username e senha andam sempre juntos → são UM tipo, não duas strings.
///
/// É um 'record' de propósito:
/// - imutável (nenhum teste consegue alterar o usuário de outro teste);
/// - igualdade por valor (dois users com os mesmos dados são iguais);
/// - ganha a expressão 'with', que é o mecanismo de override da Factory.
/// </summary>
public record User(string Username, string Password);

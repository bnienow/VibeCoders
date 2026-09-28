namespace Backend.Domain.Exceptions;

// Regra de negócio violada (ex.: "Limite de R$ 250,00 atingido").
// O Program.cs transforma em resposta 400 com a mensagem, que o front mostra na tela.
public class RegraException(string mensagem) : Exception(mensagem);

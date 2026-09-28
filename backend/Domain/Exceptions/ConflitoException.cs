namespace Backend.Domain.Exceptions;

// Recurso já existe (ex.: "E-mail já cadastrado"). O Program.cs responde 409 com a mensagem.
public class ConflitoException(string mensagem) : Exception(mensagem);

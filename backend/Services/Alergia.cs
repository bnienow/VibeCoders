namespace Backend.Services;

// Alérgenos e restrições ficam no banco como texto separado por vírgula: "Gluten,Lactose"
public static class Alergia
{
    // "Gluten,Lactose" → ["Gluten", "Lactose"]
    public static string[] ParaLista(string texto) =>
        texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    // ["Gluten", "Lactose"] → "Gluten,Lactose"
    public static string ParaTexto(string[] lista) =>
        string.Join(",", lista);

    // True se algum alérgeno do item está nas restrições do aluno
    public static bool TemConflito(string alergenosDoItem, string restricoesDoAluno) =>
        ParaLista(alergenosDoItem).Intersect(ParaLista(restricoesDoAluno)).Any();
}

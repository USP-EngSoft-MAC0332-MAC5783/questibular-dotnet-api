namespace Portfolio.Domain.Entities;

/// <summary>Individual response sent by the client when finish the exam.</summary>
public sealed class QuestionAnswer
{
    public int Index { get; private set; }
    public string CorrectAlternative  { get; private set; } = default!;
    public string SelectedAlternative { get; private set; } = default!;

    public bool IsCorrect =>
        string.Equals(CorrectAlternative, SelectedAlternative, StringComparison.OrdinalIgnoreCase);

    private QuestionAnswer() { }

    public static QuestionAnswer Create(int index, string correctAlternative, string selectedAlternative)
    {
        if (string.IsNullOrWhiteSpace(correctAlternative))
            throw new ArgumentException("Alternativa correta inválida.", nameof(correctAlternative));
        if (string.IsNullOrWhiteSpace(selectedAlternative))
            throw new ArgumentException("Alternativa selecionada inválida.", nameof(selectedAlternative));

        return new QuestionAnswer
        {
            Index               = index,
            CorrectAlternative  = correctAlternative.Trim().ToUpperInvariant(),
            SelectedAlternative = selectedAlternative.Trim().ToUpperInvariant()
        };
    }
}
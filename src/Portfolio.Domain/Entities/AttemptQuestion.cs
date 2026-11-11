namespace Portfolio.Domain.Entities;

/// <summary>Metadata of a question used in a exam attempt.</summary>
public sealed record AttemptQuestion(int Index, string Discipline, string? Language);
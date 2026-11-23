namespace Portfolio.DataAccess.Models;

internal sealed class TeacherClassDbModel
{
    public string Id { get; set; } = default!;
    public string TeacherId { get; set; } = default!;
    public string Code { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
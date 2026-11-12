namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class ExamAttemptDbModelConfiguration : IEntityTypeConfiguration<ExamAttemptDbModel>
{
    public void Configure(EntityTypeBuilder<ExamAttemptDbModel> builder)
    {
        builder.ToTable("ExamAttempts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasMaxLength(32);
        builder.Property(a => a.ExamId).IsRequired().HasMaxLength(32);
        builder.Property(a => a.AnonymousToken).HasMaxLength(64);

        builder.HasIndex(a => a.ExamId);

        builder.HasIndex(a => new { a.IsAnonymous, a.ExpiresAt }); 

        builder.Property(a => a.ServedQuestions).HasColumnType("jsonb");
        builder.Property(a => a.Answers).HasColumnType("jsonb");
    }
}
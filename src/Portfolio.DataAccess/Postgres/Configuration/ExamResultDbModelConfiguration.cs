namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class ExamResultDbModelConfiguration : IEntityTypeConfiguration<ExamResultDbModel>
{
    public void Configure(EntityTypeBuilder<ExamResultDbModel> builder)
    {
        builder.ToTable("ExamResults");
        builder.HasKey(result => result.Id);
        builder.Property(result => result.StudentId).HasMaxLength(32).IsRequired();
        builder.Property(result => result.Title).HasMaxLength(300).IsRequired();
        builder.Property(result => result.Language).HasMaxLength(50).IsRequired();
        builder.Property(result => result.AnonymousToken).HasMaxLength(128);
        builder.Property(result => result.Answers).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(result => new { result.StudentId, result.FinishAt });
        builder.HasOne<UserDbModel>()
            .WithMany()
            .HasForeignKey(result => result.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

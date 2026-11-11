namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class ExamDbModelConfiguration : IEntityTypeConfiguration<ExamDbModel>
{
    public void Configure(EntityTypeBuilder<ExamDbModel> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(32);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Year).IsRequired().HasMaxLength(4);
        builder.HasIndex(e => e.Year).IsUnique(); 
        builder.HasIndex(e => e.IsSynced);        

        builder.Property(e => e.Languages).HasColumnType("jsonb");
        builder.Property(e => e.Disciplines).HasColumnType("jsonb");
    }
}
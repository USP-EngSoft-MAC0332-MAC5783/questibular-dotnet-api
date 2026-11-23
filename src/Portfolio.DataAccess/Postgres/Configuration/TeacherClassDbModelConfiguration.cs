namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class TeacherClassDbModelConfiguration : IEntityTypeConfiguration<TeacherClassDbModel>
{
    public void Configure(EntityTypeBuilder<TeacherClassDbModel> builder)
    {
        builder.ToTable("TeacherClasses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasMaxLength(32);
        builder.Property(c => c.TeacherId).IsRequired().HasMaxLength(32);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(32);

        // Garante unicidade global do código da turma no banco
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.TeacherId);

        builder.HasOne<UserDbModel>()
            .WithMany()
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
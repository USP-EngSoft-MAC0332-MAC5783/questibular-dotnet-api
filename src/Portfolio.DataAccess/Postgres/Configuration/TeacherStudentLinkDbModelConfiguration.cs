namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class TeacherStudentLinkDbModelConfiguration
    : IEntityTypeConfiguration<TeacherStudentLinkDbModel>
{
    public void Configure(EntityTypeBuilder<TeacherStudentLinkDbModel> builder)
    {
        builder.ToTable("TeacherStudentLinks");
        builder.HasKey(link => new { link.TeacherId, link.StudentId });
        builder.HasIndex(link => link.StudentId);
        builder.HasOne<UserDbModel>()
            .WithMany()
            .HasForeignKey(link => link.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<UserDbModel>()
            .WithMany()
            .HasForeignKey(link => link.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

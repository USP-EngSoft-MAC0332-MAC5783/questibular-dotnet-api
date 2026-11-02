namespace Portfolio.DataAccess.Postgres.Configurations;

internal sealed class UserDbModelConfiguration : IEntityTypeConfiguration<UserDbModel>
{
    public void Configure(EntityTypeBuilder<UserDbModel> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasMaxLength(32);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Roles).HasColumnType("jsonb");
        builder.Property(u => u.TwoFactorRecoveryCodes).HasColumnType("jsonb");

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.TwoFactorSecret).HasMaxLength(512);
    }
}
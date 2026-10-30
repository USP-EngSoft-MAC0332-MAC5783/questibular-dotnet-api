namespace Portfolio.DataAccess.Postgres.Configurations;

public class RefreshTokenDbModelConfiguration : IEntityTypeConfiguration<RefreshTokenDbModel>
{
    public void Configure(EntityTypeBuilder<RefreshTokenDbModel> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(t => t.TokenHash).IsUnique();
        
        builder.HasOne<UserDbModel>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
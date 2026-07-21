using Microsoft.EntityFrameworkCore;

public class AuthServiceAPIContext(DbContextOptions<AuthServiceAPIContext> options) : DbContext(options)
{
    public DbSet<AuthService.API.Models.User> User { get; set; } = default!;
    public DbSet<AuthService.API.Models.OtpCode> OtpCodes { get; set; } = default!;
}

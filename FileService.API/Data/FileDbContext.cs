using FileService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FileService.API.Data
{
    // Database context for managing FileRecord entities in SQL Server
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }

        // Table for storing file metadata
        public DbSet<FileRecord> Files { get; set; } = null!;
    }
}

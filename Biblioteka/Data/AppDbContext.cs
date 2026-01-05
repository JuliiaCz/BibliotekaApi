using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LibraryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuthorEntity> Authors => Set<AuthorEntity>();
    public DbSet<BookEntity> Books => Set<BookEntity>();
}

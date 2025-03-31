using Microsoft.EntityFrameworkCore;

public class HumanDbContext : DbContext
{
    public HumanDbContext(DbContextOptions<HumanDbContext> options) : base(options) { };
}


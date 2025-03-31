using Microsoft.EntityFrameworkCore;

namespace Integration_System;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options)
    {}
}

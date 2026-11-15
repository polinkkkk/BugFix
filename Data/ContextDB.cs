using BugFix.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugFix.Api.Data;

public class ContextDB : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ContextDB(DbContextOptions<ContextDB> options) : base(options) {}
}
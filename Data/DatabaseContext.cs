using Microsoft.EntityFrameworkCore;

using chat_backend.Models;

namespace chat_backend.Data;
public class DatabaseContext: DbContext {

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options){
    }
    public DbSet<User> Users {get;set;}
    public DbSet<Group> Groups {get;set;}
    public DbSet<Message> Messages {get;set;}

}

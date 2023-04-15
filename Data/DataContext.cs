


namespace VerifyApiForgetApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }



        public DbSet<User> Users => Set<User>();
    }

}


// you can import using case to the program
// check the program
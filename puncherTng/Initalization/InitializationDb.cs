using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using puncherTng.ContextDB;

namespace puncherTng.Initalization
{
    public class InitializationDb : IDInitializationDb
    {
        private readonly Context _db;

        public InitializationDb(Context db)
        {
            _db = db;
        }
        public void Initalize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializa Database", ex);
            }
        }
    }
}

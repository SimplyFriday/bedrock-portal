using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftWrapper.Data
{
    public class SystemRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemRepository (ApplicationDbContext context)
        {
            _context = context;
        }

        public void SaveApplicationLogs ( IEnumerable<ApplicationLog> logs )
        {
            foreach (var log in logs)
            {
                _context.Add ( log );
            }

            _context.SaveChanges ();
        }

        public IEnumerable<NewsItem> GetRecentNewsItems ( int numberToReturn )
        {
            return _context.NewsItem
                .Where ( ni => ni.DateExpires == null || ni.DateExpires >= DateTime.UtcNow )
                .OrderByDescending ( ni => ni.DateActive )
                .Take ( numberToReturn );
        }
    }
}

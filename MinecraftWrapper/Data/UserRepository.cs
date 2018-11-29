using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Data
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context )
        {
            _context = context;
        }

        public AuthorizationKey GetAuthorizationKeyByToken ( string authorizationToken )
        {
            return _context.AuthorizationKey.SingleOrDefault ( key => key.AuthorizationToken == authorizationToken );
        }
    }
}

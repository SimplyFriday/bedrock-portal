using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AuthorizationKeysController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorizationKeysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AuthorizationKeys
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AuthorizationKey.Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AuthorizationKeys/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var authorizationKey = await _context.AuthorizationKey
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AuthorizationKeyId == id);
            if (authorizationKey == null)
            {
                return NotFound();
            }

            return View(authorizationKey);
        }

        // GET: AuthorizationKeys/Create
        public IActionResult Create()
        {
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewBag.Token = GetToken ( 16 );

            return View();
        }

        private string GetToken ( int length )
        {
            char[] validChars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var chars = new char[ 16 ];
            var rnd = new Random ();

            for ( int i = 0; i < length; i++ )
            {
                var c = validChars[ rnd.Next ( 0, validChars.Length ) ];
                chars[ i ] = c;
            }

            return new string ( chars );
        }

        // POST: AuthorizationKeys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorizationKeyId,AuthorizationToken,UserId")] AuthorizationKey authorizationKey)
        {
            if (ModelState.IsValid)
            {
                authorizationKey.AuthorizationKeyId = Guid.NewGuid();
                _context.Add(authorizationKey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", authorizationKey.UserId);
            return View(authorizationKey);
        }

        // GET: AuthorizationKeys/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var authorizationKey = await _context.AuthorizationKey.FindAsync(id);
            if (authorizationKey == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", authorizationKey.UserId);
            return View(authorizationKey);
        }

        // POST: AuthorizationKeys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AuthorizationKeyId,AuthorizationToken,UserId")] AuthorizationKey authorizationKey)
        {
            if (id != authorizationKey.AuthorizationKeyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(authorizationKey);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorizationKeyExists(authorizationKey.AuthorizationKeyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", authorizationKey.UserId);
            return View(authorizationKey);
        }

        // GET: AuthorizationKeys/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var authorizationKey = await _context.AuthorizationKey
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AuthorizationKeyId == id);
            if (authorizationKey == null)
            {
                return NotFound();
            }

            return View(authorizationKey);
        }

        // POST: AuthorizationKeys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var authorizationKey = await _context.AuthorizationKey.FindAsync(id);
            _context.AuthorizationKey.Remove(authorizationKey);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorizationKeyExists(Guid id)
        {
            return _context.AuthorizationKey.Any(e => e.AuthorizationKeyId == id);
        }
    }
}

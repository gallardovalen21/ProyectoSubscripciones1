using Clasess;
using Clasess.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace proyecto.Pages.Houses
{
    public class CreateModel : PageModel
    {
        private readonly SubDbContext _db;

        public CreateModel(SubDbContext db) => _db = db;

        [BindProperty]
        public string? Name { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError(nameof(Name), "Required");
                return Page();
            }

            var exists = _db.Houses.FirstOrDefault(h => h.Name != null && h.Name.ToLower() == Name.ToLower());
            if (exists == null)
            {
                _db.Houses.Add(new House { Name = Name });
                _db.SaveChanges();
            }

            return RedirectToPage("/Index");
        }
    }
}

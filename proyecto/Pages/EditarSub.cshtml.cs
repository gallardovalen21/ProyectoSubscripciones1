using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Clasess;
using Clasess.Services;

namespace proyecto.Pages
{
    public class EditarSubModel : PageModel
    {
        private readonly SubscriptionService _service;

        public EditarSubModel(SubscriptionService service) => _service = service;

        [BindProperty]
        public Subscription SubscriptionToEdit { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            // Buscamos la suscripción real en la DB
            var sub = _service.GetSubscriptionByID(id);

            if (sub == null) return RedirectToPage("Index");

            SubscriptionToEdit = sub;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            _service.UpdateSubscription(SubscriptionToEdit);
            return RedirectToPage("Index");
        }
    }
}
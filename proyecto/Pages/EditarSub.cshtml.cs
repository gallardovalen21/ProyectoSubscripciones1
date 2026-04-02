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

        [BindProperty]
        public string? NextBillingDateString { get; set; }

        public List<House> Houses { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            // Buscamos la suscripción real en la DB
            var sub = _service.GetSubscriptionByID(id);

            if (sub == null) return RedirectToPage("Index");

            SubscriptionToEdit = sub;
            Houses = _service.GetAllHouses();
            NextBillingDateString = SubscriptionToEdit.NextBillingDate.ToString("dd/MM/yyyy");
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Houses = _service.GetAllHouses();
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(NextBillingDateString))
            {
                if (DateTime.TryParseExact(NextBillingDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    SubscriptionToEdit.NextBillingDate = parsed;
                }
            }

            _service.UpdateSubscription(SubscriptionToEdit);
            return RedirectToPage("Index");
        }
    }
}
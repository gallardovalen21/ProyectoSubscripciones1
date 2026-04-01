using Clasess;
using Clasess.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace proyecto.Pages
{
    public class PaymentHistoryModel : PageModel
    {
        private readonly SubscriptionService _service;

        public PaymentHistoryModel(SubscriptionService service)
        {
            _service = service;
        }

        public Subscription? Subscription { get; set; }
        public List<Payment>? Payments { get; set; }

        public IActionResult OnGet(int id)
        {
            var sub = _service.GetSubscriptionByID(id);
            if (sub == null) return RedirectToPage("Index");

            Subscription = sub;
            Payments = sub.Payments.OrderByDescending(p => p.Date).ToList();
            return Page();
        }
    }
}

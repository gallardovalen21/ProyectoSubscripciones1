using Microsoft.AspNetCore.Mvc.RazorPages;
using Clasess;
using Clasess.Services;
using Microsoft.AspNetCore.Mvc;

namespace proyecto.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SubscriptionService _service;
        // Inyectamos el servicio
        public IndexModel(SubscriptionService service)
        {
            _service = service;
        }
        public IActionResult OnPostDelete(int id)
        {
            _service.DeactivateSubscription(id);
            return RedirectToPage();
        }
        public IActionResult OnPostUndo(int id)
        {
            _service.UndoLastPayment(id);
            return RedirectToPage();
        }

        public IActionResult OnPostPay(int id)
        {
            _service.RegisterPaymentAndCycle(id);
            TempData["JustPaidId"] = id;
            return RedirectToPage();
        } 
        public List<Subscription> Subscriptions { get; set; } = new();
        public decimal TotalMonthly { get; set; }
        public decimal TotalYearly { get; set; }

        public int? JustPaidId { get; set; }
        public void OnGet()
        {
            if (TempData["JustPaidId"] != null)
            {
                JustPaidId = (int)TempData["JustPaidId"];
            }

            _service.ProcessAutoPayments();

            var allSubscriptions = _service.GetAllSubscriptionWithPayments()
                ?? new List<Subscription>();

            Subscriptions = allSubscriptions
                .Where(s => s.Status == "Activa")
                .ToList();

            var now = DateTime.Now;

            TotalMonthly = Subscriptions
                .SelectMany(s => s.Payments ?? Enumerable.Empty<Payment>())
                .Where(p => p.Date.Year == now.Year && p.Date.Month == now.Month)
                .Sum(p => p.Amount);

            TotalYearly = Subscriptions
    .SelectMany(s => s.Payments ?? Enumerable.Empty<Payment>())
    .Where(p => p.Date.Year == now.Year)
    .Sum(p => p.Amount);
        }


    }
}
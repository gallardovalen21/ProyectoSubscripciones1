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

        public List<House> Houses { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public int? SelectedHouseId { get; set; }

        public int? JustPaidId { get; set; }
        public void OnGet()
        {
            if (TempData["JustPaidId"] != null)
            {
                JustPaidId = (int)TempData["JustPaidId"];
            }

            _service.ProcessAutoPayments();

            Houses = _service.GetAllHouses();

            // Accept older query key "houseId" as well as the bound SelectedHouseId
            if (SelectedHouseId == null)
            {
                var q = Request.Query["houseId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(q) && int.TryParse(q, out var parsed))
                {
                    SelectedHouseId = parsed;
                }
            }

            var allSubscriptions = _service.GetAllSubscriptionWithPayments()
                ?? new List<Subscription>();

            Subscriptions = allSubscriptions
                .Where(s => s.Status == "Activa")
                .Where(s => SelectedHouseId == null || s.HouseId == SelectedHouseId)
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
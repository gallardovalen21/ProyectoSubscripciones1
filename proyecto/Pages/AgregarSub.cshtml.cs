using Clasess;
using Clasess.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

    namespace proyecto.Pages
    {
        public class AgregarSubModel : PageModel
        {
            private readonly SubscriptionService _service;
            private readonly IWebHostEnvironment _env;
        public AgregarSubModel(SubscriptionService service, IWebHostEnvironment env)
            {
                _service = service;
                _env = env;
            }
            [BindProperty]
            public Subscription NewSubscription { get; set; } = new();
        [BindProperty]
        public string? NextBillingDateString { get; set; }

        public List<House> Houses { get; set; } = new();
        public string? ScrapedPricesJson { get; set; }


        public void OnGet()
            {
                NewSubscription.NextBillingDate = DateTime.Now;
                NewSubscription.Currency = "ARS";


            var filePath = Path.Combine(_env.ContentRootPath, "Scripts", "latest_prices.json");
            if (System.IO.File.Exists(filePath))
            {
                ScrapedPricesJson = System.IO.File.ReadAllText(filePath);
            }
            Houses = _service.GetAllHouses();
            NextBillingDateString = NewSubscription.NextBillingDate.ToString("dd/MM/yyyy");
        }


            public IActionResult OnPost()
            {
                if (!ModelState.IsValid)
                {
                    // reload supporting data for the form
                    var filePath = Path.Combine(_env.ContentRootPath, "Scripts", "latest_prices.json");
                    if (System.IO.File.Exists(filePath))
                    {
                        ScrapedPricesJson = System.IO.File.ReadAllText(filePath);
                    }
                    Houses = _service.GetAllHouses();
                    return Page();
                }

                NewSubscription.Status = "Activa";
                // Parse NextBillingDate from the user input (dd/MM/yyyy)
                if (!string.IsNullOrWhiteSpace(NextBillingDateString))
                {
                    if (DateTime.TryParseExact(NextBillingDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                    {
                        NewSubscription.NextBillingDate = parsed;
                    }
                }

                // Simply add the subscription; houses must be created in the Houses page
                _service.AddSubscription(NewSubscription);

                return RedirectToPage("Index");
            }
        }
    }


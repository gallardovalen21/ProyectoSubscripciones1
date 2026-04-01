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

                // Simply add the subscription; houses must be created in the Houses page
                _service.AddSubscription(NewSubscription);

                return RedirectToPage("Index");
            }
        }
    }


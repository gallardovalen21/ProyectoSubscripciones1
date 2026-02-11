using Clasess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace proyecto.Pages
{
    public class MensualModel : PageModel
    {
        private readonly SubDbContext _context;

        public MensualModel(SubDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Now.Month;

        public string MesNombre =>
            new DateTime(Year, Month, 1)
                .ToString("MMMM yyyy", new CultureInfo("es-ES"));

        // Navigation helpers used by the view
        public int PrevYear => Month == 1 ? Year - 1 : Year;
        public int PrevMonth => Month == 1 ? 12 : Month - 1;
        public int NextYear => Month == 12 ? Year + 1 : Year;
        public int NextMonth => Month == 12 ? 1 : Month + 1;

        public decimal TotalMes { get; set; }

        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();

        public async Task OnGet()
        {
            var pagos = await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Date.Year == Year && p.Date.Month == Month)
                .ToListAsync();

            var agrupado = pagos
                .GroupBy(p => p.Subscription!.ServiceName)
                .Select(g => new
                {
                    Subscription = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .ToList();

            Labels = agrupado.Select(x => x.Subscription!).ToList();
            Values = agrupado.Select(x => x.Total).ToList();
            TotalMes = Values.Sum();
        }
    }
}

    

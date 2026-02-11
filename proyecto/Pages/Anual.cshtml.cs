using Clasess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace proyecto.Pages
{
    public class AnualModel : PageModel
    {
        private readonly SubDbContext _context;

        public AnualModel(SubDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        public List<string> Meses { get; set; } = new();
        public Dictionary<string, List<decimal>> DatosPorSuscripcion { get; set; } = new();

        public async Task OnGet()
        {
           
            var pagos = await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Date.Year == Year)
                .ToListAsync();

            
            var datos = pagos
                .GroupBy(p => new
                {
                    p.Date.Year,
                    p.Date.Month,
                    p.Subscription.ServiceName
                })
                .Select(g => new
                {
                    MesNumero = g.Key.Month,
                    MesNombre = new DateTime(Year, g.Key.Month, 1)
                        .ToString("MMM", new CultureInfo("es-ES")),
                    Suscripcion = g.Key.ServiceName,
                    Total = g.Sum(x => x.Amount) 
                })
                .ToList();

            Meses = Enumerable.Range(1, 12)
                .Select(m => new DateTime(Year, m, 1)
                    .ToString("MMM", new CultureInfo("es-ES")))
                .ToList();

            var suscripciones = datos
                .Select(x => x.Suscripcion)
                .Distinct();

            foreach (var sub in suscripciones)
            {
                DatosPorSuscripcion[sub] = Meses
                    .Select((m, index) =>
                        datos.FirstOrDefault(x =>
                            x.MesNumero == index + 1 &&
                            x.Suscripcion == sub)?.Total ?? 0
                    ).ToList();
            }
        }

    }
}

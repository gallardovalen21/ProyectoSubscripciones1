using Clasess;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace proyecto.Pages.Calendar
{
    public class CalendarModel : PageModel
    {
        private readonly Clasess.SubDbContext _db;
        public CalendarModel(Clasess.SubDbContext db) => _db = db;

        public IActionResult OnGet()
        {
            var subs = _db.Subscriptions
                .Where(s => s.Status == "Activa")
                .Include(s => s.House)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//subs//proyecto//EN");

            foreach (var s in subs)
            {
                // Main event on the next billing date
                var mainDt = s.NextBillingDate;
                var mainUtc = mainDt.ToUniversalTime();
                var mainStamp = mainUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
                var mainUid = $"sub-{s.Id}@proyecto";

                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{mainUid}");
                sb.AppendLine($"DTSTAMP:{mainStamp}");
                sb.AppendLine($"DTSTART:{mainStamp}");
                sb.AppendLine($"SUMMARY:Pago - {s.ServiceName}");
                sb.AppendLine($"DESCRIPTION:Monto {s.Amount} {s.Currency} - Casa: {s.House?.Name}");
                sb.AppendLine("END:VEVENT");

                // Reminder event calculated from Recordatorio (days before)
                if (s.Recordatorio > 0)
                {
                    try
                    {
                        var reminderDt = s.NextBillingDate.AddDays(-s.Recordatorio);
                        // Only add a separate reminder event if it's a different datetime
                        if (reminderDt.Date != s.NextBillingDate.Date || reminderDt != s.NextBillingDate)
                        {
                            var remUtc = reminderDt.ToUniversalTime();
                            var remStamp = remUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
                            var remUid = $"sub-{s.Id}-reminder-{s.Recordatorio}@proyecto";

                            sb.AppendLine("BEGIN:VEVENT");
                            sb.AppendLine($"UID:{remUid}");
                            sb.AppendLine($"DTSTAMP:{remStamp}");
                            sb.AppendLine($"DTSTART:{remStamp}");
                            sb.AppendLine($"SUMMARY:Recordatorio - {s.ServiceName}");
                            sb.AppendLine($"DESCRIPTION:Recordatorio {s.Recordatorio} días antes. Monto {s.Amount} {s.Currency} - Casa: {s.House?.Name}");
                            sb.AppendLine("END:VEVENT");
                        }
                    }
                    catch
                    {
                        // ignore invalid date math
                    }
                }
            }

            sb.AppendLine("END:VCALENDAR");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/calendar", "calendar.ics");
        }
    }
}

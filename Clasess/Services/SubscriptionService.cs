using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Clasess.Services
{
    public class SubscriptionService
    {
        private readonly SubDbContext _db;


        public SubscriptionService(SubDbContext sub)
        {
            _db = sub;
            
        }
        public Subscription AddSubscription(Subscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            _db.Subscriptions.Add(subscription);
            _db.SaveChanges();

            return subscription;
        }
        public void UpdateSubscription(Subscription sub)
        {
            var existingSub = _db.Subscriptions.Find(sub.Id);
            if (existingSub != null)
            {
                existingSub.ServiceName = sub.ServiceName;
                existingSub.Amount = sub.Amount;
                existingSub.Currency = sub.Currency;
                existingSub.BillingCycle = sub.BillingCycle;
                existingSub.Category = sub.Category;
                existingSub.NextBillingDate = sub.NextBillingDate;
                existingSub.Status = sub.Status;
                existingSub.IsTrial = sub.IsTrial;
                existingSub.AutoPayment = sub.AutoPayment;
                existingSub.Recordatorio = sub.Recordatorio;
                _db.SaveChanges();
            }
        }

        public Subscription? GetSubscriptionByID(int id)
        {
            return _db.Subscriptions.FirstOrDefault(s => s.Id == id);
        }



        public List<Subscription>? GetAllSubscription()
        {
            return _db.Subscriptions.ToList();
        }

        public bool DeactivateSubscription(int? SuscriptionID)
        {
            if (SuscriptionID == null) throw new ArgumentNullException(nameof(SuscriptionID));

            var suscrip = _db.Subscriptions.FirstOrDefault(temp => temp.Id == SuscriptionID);

            if (suscrip == null) return false;

            // En lugar de borrar, cambiamos el estado
            suscrip.Status = "Inactiva";

            _db.SaveChanges();
            return true;
        }

        public void UndoLastPayment(int subscriptionId)
        {
            var sub = _db.Subscriptions
                         .Include(s => s.Payments)
                         .FirstOrDefault(s => s.Id == subscriptionId);

            if (sub != null && sub.Payments.Any())
            {
                // 1. Buscamos el pago más reciente
                var lastPayment = sub.Payments.OrderByDescending(p => p.Date).First();

                // 2. Lo borramos
                _db.Payments.Remove(lastPayment);

                // 3. RETROCEDEMOS la fecha de facturación
                sub.NextBillingDate = sub.BillingCycle switch
                {
                    "Mensual" => sub.NextBillingDate.AddMonths(-1),
                    "Trimestral" => sub.NextBillingDate.AddMonths(-3),
                    "Anual" => sub.NextBillingDate.AddYears(-1),
                    _ => sub.NextBillingDate.AddMonths(-1)
                };

                _db.SaveChanges();
            }
        }

        public List<Subscription> GetAllSubscriptionWithPayments()
        {
            // Usamos .Include para que la propiedad "Payments" no sea null
            return _db.Subscriptions
                       .Include(s => s.Payments)
                       .ToList();
        }

        private void RunPriceUpdater()
        {
            string jsonPath = Path.Combine( "Scripts", "latest_prices.json");

            // Solo ejecutamos Python si el archivo no existe o tiene más de 12 horas
            if (File.Exists(jsonPath) && File.GetLastWriteTime(jsonPath) > DateTime.Now.AddHours(-12))
            {
                return;
            }

            // ... lógica de Process.Start para ejecutar update_prices.py
        }
        private Dictionary<string, decimal> LoadPricesFromJson()
        {
            var dictionary = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            string jsonPath = Path.Combine( "Scripts", "latest_prices.json");

            if (File.Exists(jsonPath))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
                var data = doc.RootElement.GetProperty("data");

                foreach (var category in data.EnumerateObject())
                {
                    foreach (var item in category.Value.EnumerateArray())
                    {
                        string name = item.GetProperty("servicio").GetString();
                        decimal price = item.GetProperty("precio_final_ars").GetDecimal();
                        dictionary[name] = price;
                    }
                }
            }
            return dictionary;
        }
        public void ProcessAutoPayments()
        {
            // 1. EJECUCIÓN ÚNICA: Actualizamos el JSON antes de empezar el lote
            RunPriceUpdater();

            // 2. CARGA DE PRECIOS: Cargamos el JSON en memoria para comparar rápido
            var latestPrices = LoadPricesFromJson();

            // 3. SELECCIÓN: Buscamos suscripciones que vencen hoy o antes
            var dueSubs = _db.Subscriptions
                .Include(s => s.Payments)
                .Where(s => s.Status == "Activa" && s.AutoPayment == true)
                .ToList();

            bool changed = false;

            foreach (var sub in dueSubs)
            {
                while (DateTime.Today >= sub.NextBillingDate.Date)
                {
                    // 4. ACTUALIZACIÓN DE PRECIO MAESTRO:
                    // Buscamos si el servicio existe en los datos scrapeados de impuestito.org
                    if (latestPrices.TryGetValue(sub.ServiceName, out decimal newPrice))
                    {
                        sub.Amount = newPrice; // Actualizamos el precio de la suscripción
                    }

                    // 5. REGISTRO DEL PAGO: Usamos el monto (ya actualizado)
                    sub.Payments.Add(new Payment
                    {
                        Date = sub.NextBillingDate,
                        Amount = sub.Amount
                    });

                   
               
                    sub.NextBillingDate = sub.BillingCycle switch
                    {
                        "Mensual" => sub.NextBillingDate.AddMonths(1),
                        "Bimenstral" => sub.NextBillingDate.AddMonths(2),
                        "Trimestral" => sub.NextBillingDate.AddMonths(3),
                        "Anual" => sub.NextBillingDate.AddYears(1),
                        _ => sub.NextBillingDate.AddMonths(1)
                    };

                    changed = true;
                }
            }

            if (changed) _db.SaveChanges();
        }

        public void RegisterPaymentAndCycle(int subscriptionId)
        {
            var sub = _db.Subscriptions.Include(s => s.Payments).FirstOrDefault(s => s.Id == subscriptionId);

            if (sub != null)
            {
                sub.Payments.Add(new Payment
                {
                    Date = sub.NextBillingDate,
                    Amount = sub.Amount
                });

                sub.NextBillingDate = sub.BillingCycle switch
                {
                    "Mensual" => sub.NextBillingDate.AddMonths(1),
                    "Bimenstral" => sub.NextBillingDate.AddMonths(2),
                    "Trimestral" => sub.NextBillingDate.AddMonths(3),
                    "Anual" => sub.NextBillingDate.AddYears(1),
                    _ => sub.NextBillingDate.AddMonths(1) 
                };

                _db.SaveChanges();
            }
        }

        public bool DeleteSubscription(int? SuscriptionID)
        {
            if (SuscriptionID == null) throw new ArgumentNullException(nameof(SuscriptionID));

            // Lo buscamos una sola vez
            Subscription? suscrip = _db.Subscriptions.FirstOrDefault(temp => temp.Id == SuscriptionID);

            if (suscrip == null) return false;

            // Usamos el objeto que ya tenemos en memoria
            _db.Subscriptions.Remove(suscrip);
            _db.SaveChanges();

            return true;
        }

    }
}

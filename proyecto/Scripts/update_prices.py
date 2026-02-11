import subprocess
import json
import os
import sys
from datetime import datetime

# 1. LOCALIZACIÓN: Detectamos la carpeta del script
base_dir = os.path.dirname(os.path.abspath(__file__))
scraper_path = os.path.join(base_dir, 'scrap_impuestitos.py')
output_file = os.path.join(base_dir, 'latest_prices.json')

# 2. EJECUCIÓN CON ENCODING:
# Agregamos encoding='utf-8' para que capture correctamente la salida del scraper
result = subprocess.run(
    [sys.executable, scraper_path],
    capture_output=True,
    text=True,
    encoding='utf-8'  # <--- MODIFICACIÓN CRUCIAL
)

if result.returncode == 0 and result.stdout.strip():
    try:
        prices_data = json.loads(result.stdout)

        # 3. PERSISTENCIA EN UTF-8:
        # Abrimos con encoding='utf-8' y usamos ensure_ascii=False
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump({
                'data': prices_data,
                'updated_at': datetime.now().isoformat()
            }, f, ensure_ascii=False, indent=2)  # <--- MODIFICACIÓN CRUCIAL

        print(f"✅ Precios actualizados correctamente en: {output_file}")

    except json.JSONDecodeError as e:
        print(f"❌ Error al procesar JSON: {e}")
        print(f"Salida recibida (posible error de formato): {result.stdout}")
else:
    # Si el scraper falló, imprimimos el error del sistema (stderr)
    print(f"❌ Error ejecutando el scraper: {result.stderr}")
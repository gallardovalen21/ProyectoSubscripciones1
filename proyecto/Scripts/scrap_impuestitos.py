import re
import json
import requests
from bs4 import BeautifulSoup
import sys

if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

def extraer_por_servicio(url, palabra_clave):
    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
    }

    try:
        response = requests.get(url, headers=headers)
        response.encoding = 'utf-8'  # Forzamos UTF-8 para evitar caracteres raros
        soup = BeautifulSoup(response.content, 'html.parser', from_encoding='utf-8')
        scripts = soup.find_all('script', type='application/ld+json')

        for script in scripts:
            data = json.loads(script.string)
            if data.get('@type') == 'Article':
                # 1. Quitamos etiquetas HTML para tener texto plano y limpio
                texto_con_html = data.get('articleBody', '')
                texto_plano = BeautifulSoup(texto_con_html, 'html.parser').get_text(separator=' ')

                # 2. REGEX DINÁMICA:
                # Busca la palabra clave, captura el nombre hasta el $,
                # luego captura el precio hasta el /m
                patron = rf"({palabra_clave}[\w\s\+]*?)\s*\$\s*([\d.]+)\s*/m\s*\(sin\s*imp\.\s*:\s*\$\s*([\d.]+)\)"

                matches = re.findall(patron, texto_plano, re.IGNORECASE)

                resultados = []
                for nombre_completo, precio_final, precio_base in matches:
                    resultados.append({
                        "servicio": nombre_completo.strip(),
                        "precio_final_ars": int(precio_final.replace('.', '')),
                        "precio_base_ars": int(precio_base.replace('.', ''))
                    })
                return resultados
    except Exception as e:
        print(f"Error procesando {palabra_clave}: {e}")

    return []


# --- EJECUCIÓN ---
url_netflix = "https://www.impuestito.org/suscripciones/cual-es-el-precio-de-netflix-con-impuestos-en-argentina"
url_youtube = "https://www.impuestito.org/suscripciones/cual-es-el-precio-de-youtube-premium-con-impuestos-en-argentina"

datos = {
    "netflix": extraer_por_servicio(url_netflix, "Netflix"),
    "youtube_premium": extraer_por_servicio(url_youtube, "YouTube")
}

print(json.dumps(datos, ensure_ascii=False, indent=4))
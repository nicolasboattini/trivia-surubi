import os
import re
#import yaml
import codecs

def extract_data(monobehaviour_section):
    # Extraer la pregunta y las opciones
    question_match = re.search(r'text:\s*"((?:\\.|[^"\\])*)', monobehaviour_section)
    options_lines = re.findall(r'- text: (.*)', monobehaviour_section)
    correct_lines = re.findall(r'correct:\s+(\d)', monobehaviour_section)

    if not question_match or not options_lines or not correct_lines:
        raise ValueError("No se pudo extraer la pregunta u opciones.")

    question = codecs.decode(question_match.group(1), 'unicode_escape').replace('\n', ' ')
    # Decodificar la pregunta y reemplazar múltiples espacios por uno solo
    question = re.sub(r'\s+', ' ', codecs.decode(question_match.group(1), 'unicode_escape')).strip()
    options = [codecs.decode(option, 'unicode_escape').replace('"', '') for option in options_lines]

    correct_indices = [i for i, correct in enumerate(correct_lines) if correct == '1']
    
    if not correct_indices:
        raise ValueError("No se encontró una respuesta correcta.")
    
    # Formatear la salida    
    options = [option.replace("'", "") for option in options]
    formatted_options = ';'.join(options).replace('\n', ' ')
    # Eliminar las comillas simples de las opciones
    
    return f'{question.strip()};{formatted_options.strip()};{correct_indices[0]}'

# Directorio donde se encuentra el script.py
directory = os.path.dirname(os.path.abspath(__file__))

# Ruta al archivo de salida
output_file = os.path.join(directory, "output.txt")

# Lista para almacenar los resultados
results = []

# Iterar sobre los archivos .prefab en el directorio
for filename in os.listdir(directory):
    if filename.endswith(".prefab"):
        prefab_file = os.path.join(directory, filename)
        try:
            with open(prefab_file, 'r', encoding='utf-8') as file:
                prefab_content = file.read()
                match = re.search(r'MonoBehaviour:(.*?)(?=(?:---|\Z))', prefab_content, re.DOTALL)
                if match:
                    monobehaviour_section = match.group(1)
                    parsed_data = extract_data(monobehaviour_section)
                    results.append(parsed_data)
                else:
                    print(f"No se encontró la sección MonoBehaviour en {filename}.")
        except Exception as e:
            print(f"Error al parsear el archivo {filename}: {e}")

# Escribir los resultados en el archivo de salida
with open(output_file, 'w', encoding='utf-8') as f:
    for result in results:
        f.write(result + "\n")
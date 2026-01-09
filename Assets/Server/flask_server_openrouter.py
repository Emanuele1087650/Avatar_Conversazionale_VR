import os, base64, requests, json
from flask import Flask, request, jsonify, Response
from flask_cors import CORS
from dotenv import load_dotenv

load_dotenv()
app = Flask(__name__)
CORS(app)

OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY", "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")
OPENROUTER_HOST = "https://openrouter.ai/api/v1/chat/completions"
MODEL = os.getenv("OPENROUTER_MODEL", "google/gemma-3-4b-it:free")

def build_user_content(prompt_text: str, image_urls_or_datauris=None) -> dict:
    parts = []
    if prompt_text:
        parts.append({"type": "text", "text": prompt_text})
    if image_urls_or_datauris:
        for img in image_urls_or_datauris:
            parts.append({"type": "image_url", "image_url": {"url": img}})
    return {"role": "user", "content": parts}

def file_to_data_uri(file_storage) -> str:
    b64 = base64.b64encode(file_storage.read()).decode("utf-8")
    mime = file_storage.mimetype or "image/jpeg"
    return f"data:{mime};base64,{b64}"

def call_openrouter(messages, stream: bool = False):
    headers = {
        "Authorization": f"Bearer {OPENROUTER_API_KEY}",
        "Content-Type": "application/json",
    }
    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": stream,
    }
    # Nota: puoi usare anche data=json.dumps(payload); qui vado con json= per semplicità
    resp = requests.post(OPENROUTER_HOST, headers=headers, json=payload, stream=stream, timeout=60)
    resp.raise_for_status()
    return resp

@app.post("/chat-multipart")
def chat_multipart():
    """
    Form fields:
      prompt: string
      image_file: file (PNG/JPG), ripetibile
      stream: "true"/"false"
    """
    BASE_PROMPT = "Rispondi brevemente in italiano. "
    prompt = BASE_PROMPT + request.form.get("prompt", "")
    image_data_uris = [file_to_data_uri(f) for f in request.files.getlist("images")]
    messages = [
        build_user_content(prompt, image_data_uris)
    ]
    try:
        resp = call_openrouter(messages)
        return jsonify(resp.json())
    except requests.exceptions.HTTPError as e:
        try:
            body = e.response.json()
        except Exception:
            body = e.response.text[:2000]
        return jsonify({"status": e.response.status_code, "error": body}), e.response.status_code
    except requests.exceptions.RequestException as e:
        return jsonify({"status": 500, "error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, threaded=True)

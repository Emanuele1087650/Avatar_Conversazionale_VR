from flask import Flask, request, jsonify, Response
from flask_cors import CORS
import os, base64, requests
from dotenv import load_dotenv

load_dotenv()
app = Flask(__name__)
OLLAMA_HOST = os.getenv("OLLAMA_HOST", "http://localhost:11434")
MODEL = os.getenv("OLLAMA_MODEL", "gemma3:4b-it-qat")

def _call_ollama(messages, stream=False):
    url = f"{OLLAMA_HOST}/api/chat"
    payload = {
        "model": "gemma3:4b-it-qat",
        "messages": messages,
        "stream": stream
    }
    return requests.post(url, json=payload, stream=stream, timeout=None)

@app.post("/chat-multipart")
def chat_multipart():
    """
    Alternativa: invio multipart (comodo da Unity).
    Form fields:
      prompt: string
      image_file: file (PNG/JPG), ripetibile
      stream: "true"/"false"
    """
    system_prompt = """
    Rispondi brevemente in italiano 
    """
    user_prompt = request.form.get("prompt", "")
    images_b64 = []
    for f in request.files.getlist("images"):
        b = f.read()
        images_b64.append(base64.b64encode(b).decode("utf-8"))
    messages = []
    if system_prompt:
        messages.append({"role": "system", "content": system_prompt})

    user_msg = {"role": "user", "content": user_prompt}
    if images_b64:
        user_msg["images"] = images_b64
    messages.append(user_msg)
    resp = _call_ollama(messages)
    
    return jsonify(resp.json())

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, threaded=True)

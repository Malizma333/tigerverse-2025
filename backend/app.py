from flask import Flask, request, render_template, Response, jsonify
from pymongo import MongoClient, DESCENDING
import gridfs
import os
from dotenv import load_dotenv
from flask_cors import CORS
from bson import ObjectId

load_dotenv()

app = Flask(__name__)
CORS(app)

MONGO_URI = os.getenv("MONGO_URI")
client = MongoClient(MONGO_URI)
db = client['image_db']
fs = gridfs.GridFS(db)

@app.route('/upload', methods=['POST'])
def upload_image():
    if 'image' not in request.files:
        return 'No file part', 400

    file = request.files['image']
    if file.filename == '':
        return 'No selected file', 400

    image_id = fs.put(file, filename=file.filename)
    return jsonify({"id": str(image_id)}), 200

@app.route('/image/<id>')
def get_image(id):
    try:
        file = fs.get(ObjectId(id))
        return Response(file.read(), mimetype='image/jpeg')
    except:
        return 'Image not found', 404

@app.route('/image/latest')
def get_latest_image():
    latest = db.fs.files.find().sort('uploadDate', DESCENDING).limit(1)
    latest_file = next(latest, None)
    if not latest_file:
        return 'No images found', 404

    image_id = latest_file['_id']
    file = fs.get(image_id)
    return Response(file.read(), mimetype='image/jpeg')

@app.route('/images')
def list_images():
    files = db.fs.files.find().sort('uploadDate', DESCENDING)
    image_list = [{"id": str(f["_id"])} for f in files]
    return jsonify(image_list)

from flask import Flask, request, render_template, Response, jsonify
from pymongo import MongoClient, DESCENDING
import gridfs
import os
from dotenv import load_dotenv
from flask_cors import CORS
from bson import ObjectId
import cv2 as cv
import numpy as np
import pickle  # Import pickle for serialization

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

    file.stream.seek(0)
    image_data = file.read()

    np_img = np.frombuffer(image_data, np.uint8)
    img = cv.imdecode(np_img, cv.IMREAD_GRAYSCALE)

    if img is None:
        return 'Could not decode image', 400

    denoised_img = cv.bilateralFilter(img, d=9, sigmaColor=75, sigmaSpace=75)

    # Logic 1: Blue edges
    t_lower1 = 100
    t_upper1 = 200
    aperture_size1 = 3
    edges1 = cv.Canny(denoised_img, t_lower1, t_upper1, apertureSize=aperture_size1)

    transparent_bg_blue = np.zeros((edges1.shape[0], edges1.shape[1], 4), dtype=np.uint8)
    transparent_bg_blue[np.where(edges1 > 0)] = [0, 0, 255, 255] # Blue RGBA

    success_blue, buffer_blue = cv.imencode('.png', transparent_bg_blue)
    if not success_blue:
        return 'Failed to encode blue edge image', 500

    # Logic 2: Yellow edges
    t_lower2 = 50
    t_upper2 = 150
    aperture_size2 = 5
    edges2 = cv.Canny(denoised_img, t_lower2, t_upper2, apertureSize=aperture_size2)

    transparent_bg_yellow = np.zeros((edges2.shape[0], edges2.shape[1], 4), dtype=np.uint8)
    transparent_bg_yellow[np.where(edges2 > 0)] = [255, 255, 0, 255] # Yellow RGBA

    success_yellow, buffer_yellow = cv.imencode('.png', transparent_bg_yellow)
    if not success_yellow:
        return 'Failed to encode yellow edge image', 500

    # Save all images to GridFS
    original_id = fs.put(image_data, filename=f"original_{file.filename}")
    blue_edges_id = fs.put(buffer_blue.tobytes(), filename=f"blue_edges_{file.filename}.png")
    yellow_edges_id = fs.put(buffer_yellow.tobytes(), filename=f"yellow_edges_{file.filename}.png")

    # Create a parent document in MongoDB
    parent_document = {
        "original": str(original_id),
        "blue_edges": str(blue_edges_id),
        "yellow_edges": str(yellow_edges_id),
        "filename": file.filename
    }
    try:
        parent_id = db.image_metadata.insert_one(parent_document).inserted_id
    except Exception as e:
        fs.delete(original_id)
        fs.delete(blue_edges_id)
        fs.delete(yellow_edges_id)
        return f"Failed to insert metadata into MongoDB: {e}", 500

    return jsonify({
        "parent_id": str(parent_id),
        "original_id": str(original_id),
        "blue_edges_id": str(blue_edges_id),
        "yellow_edges_id": str(yellow_edges_id)
    }), 200


@app.route('/image/<id>')
def get_image(id):
    try:
        file = fs.get(ObjectId(id))
        return Response(file.read(), mimetype='image/jpeg')
    except:
        return 'Image not found', 404

@app.route('/image/latest')
def get_latest_image():
    latest = db.image_metadata.find().sort('_id', DESCENDING).limit(1)
    latest_doc = next(latest, None)
    if not latest_doc:
        return jsonify({'error': 'No images found'}), 404

    return jsonify({
        "parent_id": str(latest_doc["_id"]),
        "filename": latest_doc.get("filename", ""),
        "original": latest_doc.get("original", ""),
        "detailed": latest_doc.get("blue_edges", ""),
        "textured": latest_doc.get("yellow_edges", "")
    }), 200


@app.route('/images')
def list_images():
    files = db.image_metadata.find().sort('_id', DESCENDING)
    image_list = []
    for f in files:
        image_list.append({
            "parent_id": str(f["_id"]),
            "filename": f.get("filename", ""),
            "original": f.get("original", ""),
            "detailed": f.get("blue_edges", ""),
            "textured": f.get("yellow_edges", "")
        })
    return jsonify(image_list)

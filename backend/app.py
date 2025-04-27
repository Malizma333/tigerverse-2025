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

    # Logic 1: Blue edges with transparent background
    denoised_img = cv.bilateralFilter(img, d=9, sigmaColor=75, sigmaSpace=75)
    t_lower = 100
    t_upper = 200
    aperture_size = 3
    edges = cv.Canny(denoised_img, t_lower, t_upper, apertureSize=aperture_size)

    transparent_bg_blue = np.zeros((edges.shape[0], edges.shape[1], 4), dtype=np.uint8)
    transparent_bg_blue[np.where(edges > 0)] = [0, 0, 255, 255]  # Blue RGBA

    _, buffer_blue = cv.imencode('.png', transparent_bg_blue)

    # Logic 2: Yellow edges with transparent background
    transparent_bg_yellow = np.zeros((edges.shape[0], edges.shape[1], 4), dtype=np.uint8)
    transparent_bg_yellow[np.where(edges > 0)] = [255, 255, 0, 255]  # Yellow RGBA

    _, buffer_yellow = cv.imencode('.png', transparent_bg_yellow)

    # Save all images to GridFS
    original_id = fs.put(image_data, filename=f"original_{file.filename}")
    blue_edges_id = fs.put(buffer_blue.tobytes(), filename=f"blue_edges_{file.filename}")
    yellow_edges_id = fs.put(buffer_yellow.tobytes(), filename=f"yellow_edges_{file.filename}")

    # Create a parent document in MongoDB to associate all images
    parent_document = {
        "original": str(original_id),
        "blue_edges": str(blue_edges_id),
        "yellow_edges": str(yellow_edges_id),
        "filename": file.filename
    }
    parent_id = db.image_metadata.insert_one(parent_document).inserted_id

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

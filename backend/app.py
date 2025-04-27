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

    # Combine all images into a single dictionary
    images = {
        "original": image_data,
        "blue_edges": buffer_blue.tobytes(),
        "yellow_edges": buffer_yellow.tobytes()
    }

    # Serialize the dictionary using pickle
    serialized_data = pickle.dumps(images)

    # Save the serialized data to GridFS
    combined_image_id = fs.put(serialized_data, filename=f"combined_{file.filename}")

    return jsonify({
        "combined_id": str(combined_image_id)
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

from flask import Flask, request, render_template, Response, jsonify
from pymongo import MongoClient, DESCENDING
import gridfs
import os
from dotenv import load_dotenv
from flask_cors import CORS
from bson import ObjectId
import cv2 as cv
import numpy as np

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

    # Save the original image to GridFS
    original_image_id = fs.put(file, filename=file.filename)

    # Reset the file stream to read the image again
    file.stream.seek(0)
    image_data = file.read()

    # Convert the image data to a NumPy array
    np_img = np.frombuffer(image_data, np.uint8)

    # Decode the image (assuming it's in JPEG format)
    img = cv.imdecode(np_img, cv.IMREAD_GRAYSCALE)

    # Apply Gaussian blur to reduce noise
    blurred_img = cv.GaussianBlur(img, (5, 5), 0)

    # Apply Canny edge detection
    t_lower = 100
    t_upper = 200
    aperture_size = 3
    edges = cv.Canny(blurred_img, t_lower, t_upper, apertureSize=aperture_size)

    # Encode the processed image back to JPEG format
    _, buffer = cv.imencode('.jpg', edges)

    # Save the processed image to GridFS
    processed_image_id = fs.put(buffer.tobytes(), filename=f"processed_{file.filename}")

    return jsonify({
        "id": str(original_image_id),
        "processed_id": str(processed_image_id)
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

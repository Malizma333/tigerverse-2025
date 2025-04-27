from flask import Flask, request, jsonify
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
        return jsonify({"error": "No file part"}), 400

    file = request.files['image']
    if file.filename == '':
        return jsonify({"error": "No selected file"}), 400

    try:
        original_image_id = fs.put(file, filename=file.filename)

        file.stream.seek(0)
        image_data = file.read()

        np_img = np.frombuffer(image_data, np.uint8)
        img = cv.imdecode(np_img, cv.IMREAD_GRAYSCALE)

        if img is None:
            return jsonify({"error": "Could not decode image"}), 400

        blurred_img = cv.GaussianBlur(img, (5, 5), 0)

        t_lower = 50
        t_upper = 150
        aperture_size = 3

        edges = cv.Canny(blurred_img, t_lower, t_upper,
                         apertureSize=aperture_size,
                         L2gradient=True)

        transparent_bg = np.zeros((edges.shape[0], edges.shape[1], 4), dtype=np.uint8)

        # Set color to light yellow (BGR: 0, 255, 255) with full opacity (Alpha: 255)
        transparent_bg[np.where(edges > 0)] = [0, 255, 255, 255]

        success, buffer = cv.imencode('.png', transparent_bg)
        if not success:
             return jsonify({"error": "Could not encode processed image"}), 500

        processed_image_id = fs.put(buffer.tobytes(),
                                    filename=f"processed_{os.path.splitext(file.filename)[0]}.png",
                                    contentType="image/png")

        return jsonify({
            "id": str(original_image_id),
            "processed_id": str(processed_image_id)
        }), 200

    except gridfs.errors.NoFile:
         return jsonify({"error": "Original file not found after upload attempt (should not happen)"}), 500
    except Exception as e:
        app.logger.error(f"Error during image processing: {e}")
        return jsonify({"error": f"An internal error occurred: {e}"}), 500


@app.route('/image/<id>')
def get_image(id):
    try:
        obj_id = ObjectId(id)
        file_info = db.fs.files.find_one({"_id": obj_id})

        if not file_info:
             raise gridfs.errors.NoFile

        file = fs.get(obj_id)

        mimetype = 'image/png'
        if 'contentType' in file_info:
             mimetype = file_info['contentType']
        elif not file_info.get('filename', '').startswith('processed_'):
             mimetype = 'image/jpeg' # Fallback assumption for non-processed

        return Response(file.read(), mimetype=mimetype)
    except gridfs.errors.NoFile:
        return jsonify({"error": "Image not found"}), 404
    except Exception as e:
        app.logger.error(f"Error fetching image {id}: {e}")
        return jsonify({"error": "Invalid ID or error fetching image"}), 404


@app.route('/image/latest')
def get_latest_image():
    latest = db.fs.files.find().sort('uploadDate', DESCENDING).limit(1)
    latest_file = next(latest, None)
    if not latest_file:
        return jsonify({"error": "No images found"}), 404

    image_id = latest_file['_id']

    try:
        file = fs.get(image_id)
        mimetype = latest_file.get('contentType', 'image/jpeg')
        return Response(file.read(), mimetype=mimetype)
    except gridfs.errors.NoFile:
         return jsonify({"error": "Latest image file inconsistency"}), 404
    except Exception as e:
        app.logger.error(f"Error fetching latest image: {e}")
        return jsonify({"error": "Error fetching latest image"}), 500


@app.route('/images')
def list_images():
    try:
        files = db.fs.files.find().sort('uploadDate', DESCENDING)
        image_list = []
        for f in files:
            image_list.append({
                "id": str(f["_id"]),
                "filename": f.get("filename", "N/A"),
                "uploadDate": f.get("uploadDate"),
                "contentType": f.get("contentType", "N/A")
            })
        return jsonify(image_list)
    except Exception as e:
        app.logger.error(f"Error listing images: {e}")
        return jsonify({"error": "Error listing images"}), 500

if __name__ == '__main__':
    app.run(debug=True)
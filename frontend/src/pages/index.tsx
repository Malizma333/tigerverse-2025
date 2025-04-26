import Image from "next/image";
import Logo from "../components/logo";
import React, { useState } from "react";

export default function Home() {
  const [selectedImage, setSelectedImage] = useState(null);
  const fileInputId = "image-upload";

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedImage) {
       console.log("No image selected");
       return;
    }

    const formData = new FormData();
    formData.append("image", selectedImage);

    try {
        const response = await fetch("https://tigerverse-2025.onrender.com/upload", {
          method: "POST",
          body: formData,
        });

        const contentType = response.headers.get("content-type");
        if (response.ok) {
            if (contentType && contentType.includes("application/json")) {
              const data = await response.json();
              console.log("Upload success:", data);
            } else {
              const text = await response.text();
              console.log("Upload success (non-JSON response):", text);
            }
        } else {
            if (contentType && contentType.includes("application/json")) {
                const errorData = await response.json();
                console.error("Upload failed (JSON error):", response.status, errorData);
            } else {
                const errorText = await response.text();
                console.error("Upload failed:", response.status, errorText);
            }
        }
    } catch (error) {
        console.error("Network or other error during upload:", error);
    }
  };

  return (
    <div className="bg-neutral-300 relative w-full min-h-screen flex justify-center items-center px-8 font-inter">
      <div className="absolute z-10 bg-[url('/tiger.webp')] bg-size-[auto_75px] w-full h-full opacity-3"></div>
      <div className="z-20 flex flex-col justify-center items-center">
        <div className="text-neutral-950 font-bold py-4 w-full flex justify-center items-center gap-8">
          <Logo />
        </div>
        <form onSubmit={handleSubmit} className="flex flex-col w-full items-center gap-8 py-16 relative">
          <div className="bg-[url('/border.webp')] bg-no-repeat bg-contain bg-center p-16 group">
            <label
              htmlFor={fileInputId}
              className="rounded-md w-fit z-10 group-hover:scale-105 transition-all duration-100 relative cursor-pointer"
            >
              <img
                className="absolute w-40 animate-bouncey"
                src="./top.svg"
                alt="Upload graphic top part"
              />
              <img
                className="w-40 mt-1"
                src="./bottom.svg"
                alt="Upload graphic bottom part"
              />
              <input
                id={fileInputId}
                className="hidden"
                type="file"
                name="image"
                accept="image/png, image/jpeg, image/webp"
                onChange={(event) => {
                  if (event.target.files && event.target.files[0]) {
                    setSelectedImage(event.target.files[0]);
                    console.log("File selected:", event.target.files[0].name);
                  } else {
                    setSelectedImage(null);
                  }
                }}
              />
            </label>
          </div>

          {selectedImage && (
             <div className="text-sm -mt-10 font-extralight text-neutral-700">
                 Selected: {selectedImage.name}
             </div>
          )}

          <button
            className={`bg-neutral-950 w-full text-white text-sm font-semibold px-6 py-2 rounded-md shadow hover:bg-neutral-600 transition-all hover:cursor-pointer duration-100 disabled:bg-neutral-400 disabled:cursor-not-allowed`}
            type="submit"
            disabled={!selectedImage}
           >
            Upload
          </button>
        </form>
      </div>
    </div>
  );
}

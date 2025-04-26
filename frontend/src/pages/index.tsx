import Image from 'next/image';
import Logo from '../components/logo';
import React, { useState } from 'react';

export default function Home() {
	const [selectedImage, setSelectedImage] = useState<any>(null);
	const fileInputId = 'image-upload';

	const handleSubmit = async (e: any) => {
		e.preventDefault();
		if (!selectedImage) {
			console.log('No image selected');
			return;
		}

		const formData = new FormData();
		formData.append('image', selectedImage);

		try {
			const response = await fetch('https://tigerverse-2025.onrender.com/upload', {
				method: 'POST',
				body: formData,
			});

			const contentType = response.headers.get('content-type');
			if (response.ok) {
				if (contentType && contentType.includes('application/json')) {
					const data = await response.json();
					console.log('Upload success:', data);
          alert("Uploaded image successfully.");
				} else {
					const text = await response.text();
					console.log('Upload success (non-JSON response):', text);
          alert("Uploaded image successfully.");
				}
			} else {
				if (contentType && contentType.includes('application/json')) {
					const errorData = await response.json();
					console.error('Upload failed (JSON error):', response.status, errorData);
				} else {
					const errorText = await response.text();
					console.error('Upload failed:', response.status, errorText);
				}
			}
		} catch (error) {
			console.error('Network or other error during upload:', error);
		}
	};

	return (
    <div className="bg-white relative w-full min-h-screen flex justify-center items-center px-8 font-inter">
      <img
        src="bg-art/amongus.svg"
        alt=""
        className="absolute -top-24 -left-16 h-1/3 animate-spin"
      />
      <div className="w-full z-20 flex items-center justify-between flex-col md:flex-row">
        <div className="text-neutral-950 font-bold py-4 w-full flex flex-col justify-center items-center gap-4">
          <img className="w-100" src="logo.svg"></img>
          <div className="font-drawing text-6xl text-center leading-20">
            Make the world <br></br>
            <span className="text-pink-400 relative mr-12">
              <object
                className="absolute -top-8 -left-10"
                data="circle.svg"
                type="image/svg+xml"
                width={200}
                height={150}
              />
              your
            </span>
            canvas.
          </div>
        </div>
        <form
          onSubmit={handleSubmit}
          className="flex flex-col w-full items-center gap-4 py-16 relative"
        >
          <label
            htmlFor={fileInputId}
            className="rounded-md w-fit z-10 transition-all duration-100 cursor-pointer"
          >
            <div className="bg-[url('/border.webp')] bg-no-repeat bg-contain bg-center p-16 group">
              <div className="group-hover:scale-110 transition-all duration-100">
                <img
                  className="absolute w-40 animate-bouncey"
                  src="/top.svg"
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
              </div>
            </div>
          </label>

          {selectedImage && (
            <div className="text-sm font-extralight text-neutral-700 font-drawing">
              Selected: {selectedImage.name}
            </div>
          )}

          <button
            className={`
              p-12
              bg-[url('/button.svg')]
              bg-center
              bg-no-repeat
              bg-contain
              text-center
              font-semibold
              text-white
              flex items-center justify-center
              ${selectedImage ? "hover:scale-105" : ""}
              transition-all
              hover:cursor-pointer
              duration-100
              disabled:cursor-not-allowed
              disabled:scale-100
            `}
            type="submit"
            disabled={!selectedImage}
          >
            <div className="text-neutral-950 font-drawing text-xl">Submit</div>
          </button>
        </form>
      </div>
    </div>
  );
}

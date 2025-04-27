import React, { useState, useEffect } from "react";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { motion } from "motion/react";

const CIRCLE_SVG_DELAY_MS = 2500; // Example delay: 750 milliseconds

export default function Home() {
  const [selectedImage, setSelectedImage] = useState<any>(null);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [submitting, setSubmitting] = useState(false);
  const [showCircleSvg, setShowCircleSvg] = useState(false); // State for delaying the circle SVG
  const fileInputId = "image-upload";

  // Effect to delay the circle SVG rendering
  useEffect(() => {
    const timerId = setTimeout(() => {
      setShowCircleSvg(true);
    }, CIRCLE_SVG_DELAY_MS);

    return () => {
      clearTimeout(timerId);
    };
  }, []); // Runs only once on mount

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const { clientX, clientY } = e;
    setMousePosition({ x: clientX, y: clientY });
  };

  const backgroundPosition = {
    backgroundPosition: `${mousePosition.x / 50}px ${mousePosition.y / 50}px`,
  };

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    if (!selectedImage) {
      console.log("No image selected");
      return;
    }

    setSubmitting(true);

    const formData = new FormData();
    formData.append("image", selectedImage);

    try {
      const response = await fetch(
        "https://tigerverse-2025.onrender.com/upload",
        {
          method: "POST",
          body: formData,
        }
      );

      const contentType = response.headers.get("content-type");
      if (response.ok) {
        if (contentType && contentType.includes("application/json")) {
          const data = await response.json();
          console.log("Upload success:", data);
          toast.success("Submitted!");
        } else {
          const text = await response.text();
          console.log("Upload success (non-JSON response):", text);
          toast.success("Submitted!");
        }
      } else {
        if (contentType && contentType.includes("application/json")) {
          const errorData = await response.json();
          console.error(
            "Upload failed (JSON error):",
            response.status,
            errorData
          );
          toast.error("Failed to upload image.");
        } else {
          const errorText = await response.text();
          console.error("Upload failed:", response.status, errorText);
          toast.error("Failed to upload image.");
        }
      }
    } catch (error) {
      console.error("Network or other error during upload:", error);
      toast.error("An error occurred during upload.");
    } finally {
      setSubmitting(false);
      setSelectedImage(null);
    }
  };

  return (
    <div
      className="bg-white relative w-full min-h-screen flex justify-center items-center px-8 overflow-hidden"
      onMouseMove={handleMouseMove}
    >
      <ToastContainer />
      <img
        src="bg-art/amongus.svg"
        alt=""
        className="absolute -top-24 -left-16 h-1/3 z-10 animate-spin"
      />
      <div
        className="absolute w-full h-full bg-[url('/background.png')] bg-size-[1000px] bg-center opacity-10"
        style={backgroundPosition}
      ></div>
      <div className="w-full z-20 flex items-center justify-between flex-col md:flex-row">
        <div className="text-neutral-900 font-bold py-4 w-full flex flex-col justify-center items-center gap-10">
          <object className="w-100" data="logo.svg" type="image/svg+xml" />
          <motion.div
            className="font-drawing text-7xl text-center leading-20"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{
              scale: 1,
              opacity: 100,
              transition: { duration: 0.25, ease: "easeInOut" },
            }}
          >
            Make the world <br></br>
            <span className="text-pink-400 relative mr-12">
              {/* Conditionally render the circle SVG */}
              {showCircleSvg && (
                <object
                  className="absolute -top-6 -left-13"
                  data="circle.svg"
                  type="image/svg+xml"
                  width={250}
                  height={150}
                  aria-hidden="true" // Added for accessibility if purely decorative
                />
              )}
              your
            </span>
            canvas.
          </motion.div>
        </div>
        <motion.div
          className="flex flex-col w-full items-center gap-4 py-16 relative"
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{
            scale: 1,
            opacity: 100,
            transition: { duration: 0.25, ease: "easeInOut" },
          }}
        >
          <form onSubmit={handleSubmit} className="">
            <label
              htmlFor={fileInputId}
              className="rounded-md w-fit z-10 transition-all duration-100 cursor-pointer"
            >
              <div className="text-center text-neutral-950 font-drawing text-5xl mb-4">
                Upload Image
              </div>
              <div className="bg-[url('/border.webp')] bg-no-repeat bg-contain bg-center p-14 group">
                <div className="group-hover:scale-110 transition-all duration-100">
                  <img
                    className="absolute w-50 animate-bouncey"
                    src="/top.svg"
                    alt="Upload graphic top part"
                  />
                  <img
                    className="w-50 mt-1"
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
                        console.log(
                          "File selected:",
                          event.target.files[0].name
                        );
                      } else {
                        setSelectedImage(null);
                      }
                    }}
                  />
                </div>
              </div>
            </label>

            {selectedImage && (
              <>
                <div className="text-2xl -mt-4 font-extralight text-neutral-700 font-drawing">
                  Selected: {selectedImage.name}
                </div>
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
                  disabled={!selectedImage || submitting}
                >
                  <div className="text-neutral-950 font-drawing text-3xl">
                    {submitting ? "Submitting..." : "Submit"}
                  </div>
                </button>
              </>
            )}
          </form>
        </motion.div>
      </div>
    </div>
  );
}

import React, { useState, useEffect } from "react";
import { motion } from "motion/react";

const API_BASE_URL = "https://tigerverse-2025.onrender.com";

interface ImageInfo {
  detailed: string;
  filename: string;
  original: string;
  parent_id: string;
  textured: string;
}

export default function Gallery() {
  const [mounted, setMounted] = useState(false);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [imageInfos, setImageInfos] = useState<ImageInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    const fetchImageData = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await fetch(`${API_BASE_URL}/images`);
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();

        if (
          Array.isArray(data) &&
          (data.length === 0 ||
            (data.length > 0 &&
              typeof data[0]?.original === "string" &&
              typeof data[0]?.parent_id === "string"))
        ) {
          setImageInfos(data as ImageInfo[]);
        } else {
          console.error(
            "API did not return an array of objects with 'original' and 'parent_id' properties:",
            data
          );
          throw new Error("Received invalid data format for image list.");
        }
      } catch (e) {
        console.error("Failed to fetch image data:", e);
        setError(
          e instanceof Error
            ? e.message
            : "Failed to load image list. Please try again later."
        );
      } finally {
        setLoading(false);
      }
    };

    fetchImageData();
  }, []);

  if (!mounted) {
    return null;
  }

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const { clientX, clientY } = e;
    setMousePosition({ x: clientX, y: clientY });
  };

  const backgroundPosition = {
    backgroundPosition: `${mousePosition.x / 50}px ${mousePosition.y / 50}px`,
  };

  return (
    <div
      className="w-full min-h-screen bg-white relative overflow-hidden"
      onMouseMove={handleMouseMove}
    >
      <div
        className="absolute w-full h-full bg-[url('/background.png')] bg-[length:1000px_auto] bg-center opacity-10 pointer-events-none"
        style={backgroundPosition}
      ></div>
      <a
        href="/"
        className="fixed bottom-0 right-0 flex justify-center items-center m-12 z-100 hover:scale-105 "
      >
        <div
          className={`
                  p-4
                  bg-[url('/button.svg')]
                  bg-center
                  bg-no-repeat
                  bg-contain
                  text-center
                  font-semibold
                  flex items-center justify-center
                  group-hover:scale-105
                  transition-all
                  duration-100
                  cursor-pointer
                  text-neutral-950 font-drawing text-xl
                `}
        >
          Home
        </div>
      </a>

      <div className="relative z-10 p-4 sm:p-8 flex flex-col items-center w-full">
        <motion.div
          className="text-center mb-8"
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.25, ease: "easeInOut" }}
        >
          <h1 className="text-3xl sm:text-4xl text-neutral-950 font-bold mb-4">
            Welcome to the Gallery
          </h1>
          <p className="text-lg text-neutral-600">
            Explore our collection of images.
          </p>
        </motion.div>

        <div className="w-full max-w-6xl">
          {loading && (
            <p className="text-center text-neutral-500">Loading images...</p>
          )}
          {error && <p className="text-center text-red-600">Error: {error}</p>}
          {!loading &&
            !error &&
            (imageInfos.length === 0 ? (
              <p className="text-center text-neutral-500">No images found.</p>
            ) : (
              <div className="flex flex-wrap justify-center gap-4">
                {imageInfos.map((imgInfo) => (
                  <div
                    key={imgInfo.parent_id}
                    className="bg-gray-100 rounded shadow overflow-hidden flex flex-col items-center p-1"
                  >
                    <img
                      src={`${API_BASE_URL}/image/${imgInfo.original}`}
                      alt={imgInfo.filename || `Image ${imgInfo.original}`}
                      className="block w-full max-w-[150px] sm:max-w-[200px] h-auto object-cover aspect-square"
                      loading="lazy"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.onerror = null;
                        target.alt = `Failed to load ${
                          imgInfo.filename || imgInfo.original
                        }`;
                        target.style.border = "1px solid red";
                        target.style.backgroundColor = "#f0f0f0";
                        console.warn(
                          `Failed to load image with original ID: ${imgInfo.original}`
                        );
                      }}
                    />
                  </div>
                ))}
              </div>
            ))}
        </div>
      </div>
    </div>
  );
}

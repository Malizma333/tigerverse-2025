import React, { useState, useEffect } from "react";
import { motion } from "framer-motion";

const API_BASE_URL = "https://tigerverse-2025.onrender.com";

interface ImageInfo {
  detailed: string;
  filename: string;
  original: string;
  parent_id: string;
  textured: string;
}

// Variants for the gallery animation
const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.08,
    },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20, scale: 0.95 },
  visible: {
    opacity: 1,
    y: 0,
    scale: 1,
    transition: {
      duration: 0.3,
      ease: "easeOut",
    },
  },
};

export default function Gallery() {
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [imageInfos, setImageInfos] = useState<ImageInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

  // --- Start: Unchanged Background Logic ---
  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const { clientX, clientY } = e;
    setMousePosition({ x: clientX, y: clientY });
  };

  const backgroundPosition = {
    backgroundPosition: `${mousePosition.x / 50}px ${mousePosition.y / 50}px`,
  };
  // --- End: Unchanged Background Logic ---

  return (
    <div
      className="w-full min-h-screen bg-white relative overflow-hidden"
      onMouseMove={handleMouseMove} // Keep mouse move handler on the main div
    >
      {/* --- Start: Unchanged Background Div --- */}
      <div
        className="absolute w-full h-full bg-[url('/background.png')] bg-[length:1000px_auto] bg-center opacity-10 pointer-events-none z-10" // Kept bg-center and original styles
        style={backgroundPosition} // Apply the calculated style
      ></div>
      {/* --- End: Unchanged Background Div --- */}
      <a
        href="/"
        className="fixed bottom-0 right-0 flex justify-center items-center m-8 sm:m-12 z-50 hover:scale-105 transition-transform duration-100"
      >
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.25, ease: "easeInOut" }}
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
        </motion.div>
      </a>

      <div className="relative mb-12 z-10 p-4 sm:p-8 flex flex-col items-center w-full">
        <motion.div
          className="text-center mb-8"
          // Apply animation to the header section
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4, ease: "easeOut", delay: 0.1 }}
        >
          <h1 className="text-3xl sm:text-4xl mt-12 text-neutral-950 font-bold mb-4">
            Welcome to the Gallery
          </h1>
          <p className="text-lg text-neutral-600">
            Explore our collection of images.
          </p>
        </motion.div>

        <div className="w-full max-w-6xl mx-auto">
          {loading && (
            <p className="text-center text-neutral-500 py-10">
              Loading images...
            </p>
          )}
          {error && (
            <p className="text-center text-red-600 py-10">Error: {error}</p>
          )}
          {!loading &&
            !error &&
            (imageInfos.length === 0 ? (
              <p className="text-center text-neutral-500 py-10">
                No images found.
              </p>
            ) : (
              // Apply container variants for staggered animation
              <motion.div
                className="flex flex-wrap justify-center gap-6 sm:gap-8"
                variants={containerVariants}
                initial="hidden"
                animate="visible"
              >
                {imageInfos.map((imgInfo) => (
                  // Apply item variants to each image card
                  <motion.div
                    key={imgInfo.parent_id}
                    variants={itemVariants}
                    className="bg-neutral-100 rounded-lg overflow-hidden flex flex-col items-center p-2 shadow-md hover:shadow-lg transition-shadow duration-200"
                  >
                    <img
                      src={`${API_BASE_URL}/image/${imgInfo.original}`}
                      alt={imgInfo.filename || `Image ${imgInfo.original}`}
                      // Adjusted styling for consistency
                      className="block w-[150px] h-[150px] sm:w-[200px] sm:h-[200px] object-cover"
                      loading="lazy"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.onerror = null;
                        target.alt = `Failed to load ${
                          imgInfo.filename || imgInfo.original
                        }`;
                        // Simplified error display directly on the img container
                        target.style.display = "none"; // Hide broken image icon
                        const parent = target.parentElement;
                        if (parent) {
                          // Add a placeholder div inside the motion.div
                          const errorPlaceholder =
                            document.createElement("div");
                          errorPlaceholder.textContent = "Error";
                          errorPlaceholder.className =
                            "flex items-center justify-center w-[150px] h-[150px] sm:w-[200px] sm:h-[200px] bg-neutral-200 text-red-500 text-xs font-semibold";
                          parent.appendChild(errorPlaceholder);
                        }
                        console.warn(
                          `Failed to load image with original ID: ${imgInfo.original}`
                        );
                      }}
                    />
                  </motion.div>
                ))}
              </motion.div>
            ))}
        </div>
      </div>
    </div>
  );
}

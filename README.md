# ARtist
Make the canvas **your** world.
Submitted for TigerVerse 2025 held at the University of Missouri.
## Inspiration
We all share the frustration of trying to translate a mental or reference image onto a physical surface. **ARtist** uses the power of AR to overlay a perfectly scalable digital guide onto any real-world canvas, bridging the gap between vision and execution. Our goal is to inspire anyone to confidently bring their ideas to life and experience the joy of creation by making the process intuitive and accessible.

## What It Does
**ARtist** acts as a digital stencil, allowing users to select any image and project it virtually onto a surface like a wall, canvas, or piece of paper. Users can freely scale and position the projected image. By looking through a VR headset, they see the image overlaid on their chosen surface, providing a clear guide to trace—making it significantly easier to transfer complex shapes and proportions.

## How We Built It
- **Frontend**: Built with Next.js and TypeScript
- **Backend**: Built with Flask, using GridFS and MongoDB to handle image metadata and storage
- **Image Processing**: OpenCV for edge detection and filtering (e.g., Canny algorithm)
- **AR/VR Development**: Unity for scene building and C# for scripting and backend integration

## Challenges We Ran Into
Setting up and testing Unity scenes with the Meta Quest 3 was extremely challenging. Quest Link often failed to load, wasting countless hours. After almost 10 hours of troubleshooting (and even buying a new laptop), we discovered that Quest Link has compatibility issues with Windows 11 and requires the latest updates. A successful setup is indicated when Quest Link boots within 5–10 seconds.

## Accomplishments We're Proud Of
Despite the technical hurdles, one team member set out to collect as many doodles as possible from hackers at the event—capturing the spirit of creativity that ARtist is all about.

## What We Learned
- How to build an AR app from scratch in Unity for the Meta Quest 3
- How to integrate a web app backend with Unity

## What's Next for ARtist
We want to expand ARtist into a collaborative space where multiple people can work on the same piece of art simultaneously. We also want to add the ability for users to snap art directly onto surfaces for better alignment.

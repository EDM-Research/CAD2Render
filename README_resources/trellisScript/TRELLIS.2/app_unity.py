import os
os.environ['OPENCV_IO_ENABLE_OPENEXR'] = '1'
os.environ["PYTORCH_CUDA_ALLOC_CONF"] = "expandable_segments:True"  # Can save GPU memory

from PIL import Image
from trellis2.pipelines import Trellis2ImageTo3DPipeline, Trellis2TexturingPipeline
import o_voxel

import cv2
from tkinter import filedialog
import sys

# Handle command line arguments for file destinations
if len(sys.argv) > 3:
    filename = sys.argv[1]
    fileDestination = sys.argv[2] + "/"
    testDestination = sys.argv[3] + "/"
else:
    fileDestination = "/home/dfl-user/Documents/TRELLIS.2/GeneratedMeshesUnity/"
    filename = str(len(os.listdir(fileDestination)))
    testDestination = "/home/dfl-user/Documents/TRELLIS.2-tests/" + filename + "/"
if not os.path.exists(testDestination):
    os.makedirs(testDestination)

captured_images = []
if len(sys.argv) > 4:
    # If file paths are provided as command line arguments, use them
    file_paths = sys.argv[4:]
    for file_path in file_paths:
        pil_image = Image.open(file_path).convert("RGB")
        captured_images.append(pil_image)

if len(captured_images) == 0:
    # Select images - if no image selected then use webcam for capturing
    file_paths = filedialog.askopenfilenames(title="Select images", filetypes=[("Image File", ('*.png', '*.jpg', '*.jpeg')), ("All files", "*.*")])
    for file_path in file_paths:
        pil_image = Image.open(file_path).convert("RGB")
        captured_images.append(pil_image)

if len(captured_images) == 0:
    # Handle UI for capturing images
    cap = cv2.VideoCapture(0)

    if not cap.isOpened():
        print("Cannot open camera")
        exit()

    print("Press 's' to capture an image.\nPress 'p' to process.\nPress 'q' to quit.")
    while True:
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame")
            break

        # Display the webcam feed
        cv2.imshow("Webcam", frame)

        key = cv2.waitKey(1) & 0xFF
        
        if key == ord('s'):  # Press 's' to save the frame
            # Convert from OpenCV BGR to RGB
            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            # Convert to PIL Image
            pil_image = Image.fromarray(frame_rgb)
            # Append to list
            captured_images.append(pil_image)
            print(f"Image captured. Total images: {len(captured_images)}")

        elif key == ord('q'):  # Press 'q' to quit
            print("Quitting.")
            exit()
            
        elif key == ord('p'):  # Press 'q' to quit
            print("Starting processing.")
            break
    cap.release()
    cv2.destroyAllWindows()

# Load a pipeline from a model folder or a Hugging Face model hub
pipeline = Trellis2ImageTo3DPipeline.from_pretrained('microsoft/TRELLIS.2-4B')
pipeline.cuda()

# Run the pipeline
if len(captured_images) >= 2:
    mesh = pipeline.run_multi_image(captured_images, seed=1)[0]
elif len(captured_images) == 1:
    mesh = pipeline.run(captured_images[0], seed=1)[0]
else:
    print("No images captured. Exiting.")
    exit()
mesh.simplify(16777216) # nvdiffrast limit

# GLB files can be extracted from the outputs
glb_raw = o_voxel.postprocess.to_glb(
    vertices            =   mesh.vertices,
    faces               =   mesh.faces,
    attr_volume         =   mesh.attrs,
    coords              =   mesh.coords,
    attr_layout         =   mesh.layout,
    voxel_size          =   mesh.voxel_size,
    aabb                =   [[-0.5, -0.5, -0.5], [0.5, 0.5, 0.5]],
    decimation_target   =   1000000,
    texture_size        =   4096,
    remesh              =   True,
    remesh_band         =   1,
    remesh_project      =   0,
    verbose             =   True
)

# Texture
texturing_pipeline = Trellis2TexturingPipeline.from_pretrained("microsoft/TRELLIS.2-4B", config_file="texturing_pipeline.json")
texturing_pipeline.cuda()
glb = texturing_pipeline.run(glb_raw, captured_images[0])

# Export glb and save inputs / outputs in a test folder
glb_filepath = fileDestination + filename + ".glb"
glb_test_filepath = testDestination + filename + ".glb"
glb_raw_filepath = testDestination + filename + "_raw" + ".glb"

for folder in [
    os.path.dirname(glb_filepath),
    os.path.dirname(glb_test_filepath),
    os.path.dirname(glb_raw_filepath)
]:
    if not os.path.exists(folder):
        os.makedirs(folder)

glb.export(glb_test_filepath)
glb_raw.export(glb_raw_filepath)
if len(captured_images) == 1:
    captured_images[0].save(testDestination + filename + "_img.png")
else:
    for i in range(len(captured_images)):
        captured_images[i].save(testDestination + filename + "_img-" + str(i) + ".png")
glb.export(glb_filepath)
exit()


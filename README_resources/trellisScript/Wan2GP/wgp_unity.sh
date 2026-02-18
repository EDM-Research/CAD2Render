#!/bin/bash
# Script to run Wan2GP and generate images for Unity import
# $1: Text prompt for image generation
# $2: (Optional) Directory to save output images
# $3: (Optional) Directory for test results
if [ $# -lt 1 ]; then
    touch GeneratedImagesUnity/wan2gpProcessing.lock
    echo "Usage: $0 <text to image prompt> (<directory for output images> <directory for test results>)"
    rm GeneratedImagesUnity/wan2gpProcessing.lock
    exit 1
fi
if [ $# -lt 2 ]; then
    touch GeneratedImagesUnity/wan2gpProcessing.lock
    conda run --no-capture-output -n wan2gp python wgp_unity_queue.py "$1" outputs/unity_queue.zip
    conda run --no-capture-output -n wan2gp python wgp.py --process outputs/unity_queue.zip --output-dir GeneratedImagesUnity
    rm GeneratedImagesUnity/wan2gpProcessing.lock
else
    touch "${2}/wan2gpProcessing.lock"
    if [ $# -lt 3 ]; then
        conda run --no-capture-output -n wan2gp python wgp_unity_queue.py "$1" outputs/unity_queue.zip
        conda run --no-capture-output -n wan2gp python wgp.py --process outputs/unity_queue.zip --output-dir "$2"
    else
        conda run --no-capture-output -n wan2gp python wgp_unity_queue.py "$1" "${3}/unity_queue.zip"
        conda run --no-capture-output -n wan2gp python wgp.py --process "${3}/unity_queue.zip" --output-dir "$3"
    fi
    rm "${2}/wan2gpProcessing.lock"
fi


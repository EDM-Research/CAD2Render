# !/bin/bash
# Args:
# $1 - (Optional) filename
# $2 - (Optional) destination folder
# $3 - (Optional) test destination folder
# $4...$n - (Optional) image file paths (currently only supports one image)
if [ $# -lt 3 ]; then
    touch GeneratedMeshesUnity/trellisProcessing.lock
    conda run --no-capture-output -n trellis2 python app_unity.py
    rm GeneratedMeshesUnity/trellisProcessing.lock
else
    touch "${2}/trellisProcessing.lock"
    if [ $# -lt 4 ]; then
        conda run --no-capture-output -n trellis2 python app_unity.py "$1" "$2" "$3"
    else
        conda run --no-capture-output -n trellis2 python app_unity.py "$1" "$2" "$3" "$4"
    fi
    rm "${2}/trellisProcessing.lock"
fi


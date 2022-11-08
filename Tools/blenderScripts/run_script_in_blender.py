import subprocess
blenderPath = 'C:/Progra~1/Blender Foundation/Blender/blender.exe'




# STEP 1 convert obj dataset to bop ply dataset format
def convertOffToObj(inputFile, outputFile):
    print("Converting \"%s\" to \"%s\"..."%(inputFile,outputFile))
    import subprocess
    list_files = subprocess.run(["meshlabserver", '-i', inputFile, '-o', outputFile, '-m', 'sa', 'vn', 'vc', 'vt', 'fn'])
    
    if list_files.returncode is not 0:
        print("ERROR: failed converting OFF to OBJ: ", inputFile, " --> ", outputFile)
        
        

# print('__________________________________________________________________________')
# print('STEP 1: CONVERTING OFF TO OBJ')
# mesh_path = 'aeroplane'
# model = '01'
# # ensure_dir(outputDatasetPath)
# if mesh_path is not None:
    # convertOffToObj(mesh_path + '/%s.off'%model, mesh_path+'/%s.obj'%model)
# else:
    # print('ERROR: input mesh directory \"' + mesh_path + '\" does not exists')
        

    
    
# STEP 2 run script for loading and applying automatic uv mapping in blender
list_files = subprocess.run([blenderPath, "-b", "--python", "blender_uvmap_and_export_obj.py", "--", "models/aeroplane/01.obj", "models/aeroplane/01_uv.obj"])


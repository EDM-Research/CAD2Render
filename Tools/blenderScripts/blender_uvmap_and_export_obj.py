import bpy



def execute_uv_project(context, ProjectionType = "1",centerObjects = False, splitObjects = False):
        
    ob = bpy.data.objects[0]
    # for ob in bpy.context.view_layer.objects:
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)

    #set the center of teh mesh on the origin
    if(centerObjects):
        bpy.ops.object.origin_set(type='GEOMETRY_ORIGIN', center='MEDIAN')
    
    #switch to edit mode
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    
    #do the uv mapping
    if ProjectionType == "1": #Smart UV Projection
        bpy.ops.uv.smart_project()
    elif ProjectionType == "2": #Cube Projection
        bpy.ops.uv.cube_project()
    elif ProjectionType == "3": #Cylinder Projection
        bpy.ops.uv.cylinder_project()
    else: #Sphere Projection
        bpy.ops.uv.sphere_project()

    #switch back to object mode
    bpy.ops.object.editmode_toggle()
    ob.select_set(False)
    return {'FINISHED'}
        

def main(input_obj_file, output_obj_file):
    print('test')
    
    #delete default scene
    object_to_delete = bpy.data.objects['Cube']
    bpy.data.objects.remove(object_to_delete, do_unlink=True)
    object_to_delete = bpy.data.objects['Light']
    bpy.data.objects.remove(object_to_delete, do_unlink=True)
    object_to_delete = bpy.data.objects['Camera']
    bpy.data.objects.remove(object_to_delete, do_unlink=True)
    

    # mesh_path = 'models/aeroplane/01.obj'
    bpy.ops.import_scene.obj("EXEC_DEFAULT",filepath=input_obj_file)
    
    execute_uv_project(bpy.context)
    
    # output_mesh_path = 'models/aeroplane/01_uv.obj'
    bpy.ops.export_scene.obj("EXEC_DEFAULT", filepath=output_obj_file, use_materials=False, check_existing=False)
    
    
    # uv = FixUV(bpy.types.Operator())
    # uv.execute(bpy.context)



if __name__ == "__main__":
    import sys
    argv = sys.argv
    argv = argv[argv.index("--") + 1:]  # get all args after "--"

    main(argv[0], argv[1])

import bpy

class FixUV(bpy.types.Operator):
    ''''''
    bl_idname = "object.simple_operator"
    bl_label = "Simple Object Operator"
    bl_options = {'REGISTER', 'UNDO'}
    
    
    ProjectionType : bpy.props.EnumProperty(
        name="UV projection type",
        items=(
               ("1", 'Smart UV Projection', 'Use the smart uv projection to unwrap the object'),
               ("2", 'Cube Projection', 'Project the uv vertices on a cube'),
               ("3", 'Cylinder Projection', 'Project the uv vertices on a Cylinder'),
               ("4", 'Sphere Projection', 'Project the uv vertices on a Sphere')
            ),
        )
    #ObjectPath : bpy.props.StringProperty(name="Object Path", subtype='DIR_PATH')
    splitObjects: bpy.props.BoolProperty(name="Split meshes", description="Splits enclosed meshes and uv maps them speratly")
    centerObjects: bpy.props.BoolProperty(name="Center meshes", description="Centers all objects around the origin")

    def invoke(self, context, event):
        wm = context.window_manager
        wm.invoke_props_dialog(self)
        return {'RUNNING_MODAL'}

    def execute(self, context):
        #split huls from eachoter
        if(self.splitObjects):
            for ob in bpy.context.view_layer.objects:
                bpy.context.view_layer.objects.active = ob
                ob.select_set(True)
                bpy.ops.mesh.separate(type="LOOSE")
                bpy.ops.object.select_all(action='DESELECT')
                
        for ob in bpy.context.view_layer.objects:
            bpy.context.view_layer.objects.active = ob
            ob.select_set(True)

            #set the center of teh mesh on the origin
            if(self.centerObjects):
                bpy.ops.object.origin_set(type='GEOMETRY_ORIGIN', center='MEDIAN')
            
            #switch to edit mode
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.mesh.select_all(action='SELECT')
            
            #do the uv mapping
            if self.ProjectionType == "1": #Smart UV Projection
                bpy.ops.uv.smart_project()
            elif self.ProjectionType == "2": #Cube Projection
                bpy.ops.uv.cube_project()
            elif self.ProjectionType == "3": #Cylinder Projection
                bpy.ops.uv.cylinder_project()
            else: #Sphere Projection
                bpy.ops.uv.sphere_project()

            #switch back to object mode
            bpy.ops.object.editmode_toggle()
            ob.select_set(False)
        return {'FINISHED'}


class UVFixer(bpy.types.Panel):
    bl_label = "UV Fixer"
    bl_idname = "PILS_PT_UV_Fixer"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "Tool"
    
    

    def draw(self, context):
        executeButton = self.layout.row()
        executeButton.operator("object.simple_operator", text="fix UV mapping")



def register():
    bpy.utils.register_class(UVFixer)
    bpy.utils.register_class(FixUV)

def unregister():
    bpy.utils.unregister_class(UVFixer)
    bpy.utils.unregister_class(FixUV)

if __name__ == "__main__":
    register()

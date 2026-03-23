import bpy
import pathlib
import re

# Important notice. Most of this script is made trough the use of chatgpt due to me having
# no idea of how to script in blender

# Some modifications were done tho, moved from os to pathlib and changed most of the code
# for loading the target. Else, chat has taken the liberty of creating it


# ---------------- SETTINGS ----------------
meshpattern = re.compile(r'^(\.\.|[A-Za-z]+\.\d{3})$')

blend_path : pathlib.Path = pathlib.Path(bpy.data.filepath)

if not blend_path.exists():
    raise Exception("Blend file must be saved before exporting!")

folder : pathlib.Path = blend_path.parent.resolve()
TARGET_FOLDER = pathlib.Path(folder, "visemes/")   # Folder containing .target files

EXPORT_FILE = pathlib.Path(folder,blend_path.stem+".fbx")

# ------------------------------------------

def deselect_all():
    bpy.ops.object.select_all(action='DESELECT')


def duplicate_character():
    # Duplicate all visible objects
    deselect_all()
    for obj in bpy.context.scene.objects:
        if obj.type in {'MESH', 'ARMATURE'}:
            obj.select_set(True)

    bpy.ops.object.duplicate()
    duplicates = bpy.context.selected_objects
    return duplicates


def make_active(obj):
    bpy.context.view_layer.objects.active = obj


def run_mpfb_merge(body_obj, duplicates = []):
    make_active(body_obj)
    bpy.ops.mpfb.bake_shapekeys()          # MPFB merge keys
    bpy.ops.mpfb.delete_helpers()          # Remove helper geometry
    

    # Make sure vertex color layer exists
    if not body_obj.data.vertex_colors:
        body_obj.data.vertex_colors.new(name="Mask")
        
    vcol_layer = body_obj.data.vertex_colors.active
    
    # Step 1: Set all loops to white (visible)
    vcol_layer.data.foreach_set("color", [1.0, 1.0, 1.0, 1.0] * len(vcol_layer.data))
    
    for obj in duplicates:
        
        # Corresponding vertex group name
        vg_name = f"Delete.{obj.name.split('.')[1]}"
        print(vg_name)
        vg = body_obj.vertex_groups.get(vg_name)
        print(vg)
        if not vg:
            continue
        
        verts_in_group = {
            v.index for v in body_obj.data.vertices
            if vg.index in [g.group for g in v.groups]
        }

        # Paint all loops belonging to those vertices black
        for poly in body_obj.data.polygons:
            for loop_index in poly.loop_indices:
                loop = body_obj.data.loops[loop_index]
                if loop.vertex_index in verts_in_group:
                    vcol_layer.data[loop_index].color = (0.0, 0.0, 0.0, 1.0)



def load_targets(mesh_obj):
    make_active(mesh_obj)
    target_files = [{"name": str(f.name)} for f in TARGET_FOLDER.iterdir() if f.suffix == ".target"]
    bpy.ops.mpfb.load_target(directory=str(TARGET_FOLDER), files=target_files)

def export_selected_fbx():
    bpy.ops.export_scene.fbx(
        filepath=str(EXPORT_FILE),
        use_selection=True,
        use_mesh_modifiers=False,
        add_leaf_bones=False,
        use_armature_deform_only=True
    )


# ================= PIPELINE =================

duplicates = duplicate_character()

# Separate mesh and armature
mesh_obj = next((obj for obj in duplicates if meshpattern.match(obj.name)), None)
armature = next((o for o in duplicates if o.type == 'ARMATURE'), None)

if mesh_obj is None:
    raise Exception("No mesh found in duplicate!")

# Parent mesh to armature if needed
if armature:
    mesh_obj.parent = armature

# Run MPFB operations
run_mpfb_merge(mesh_obj, duplicates)
load_targets(mesh_obj)

# Export only duplicates
deselect_all()
for obj in duplicates:
    obj.select_set(True)

export_selected_fbx()

# ---------------- CLEANUP ----------------

deselect_all()

for obj in duplicates:
    if obj.name in bpy.data.objects:
        obj.select_set(True)

bpy.ops.object.delete()


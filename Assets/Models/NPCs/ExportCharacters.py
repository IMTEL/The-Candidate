import bpy
import pathlib

# Important notice. Most of this script is made trough the use of chatgpt due to me having
# no idea of how to script in blender

# Some modifications were done tho, moved from os to pathlib and changed most of the code
# for loading the target. Else, chat has taken the liberty of creating it


# ---------------- SETTINGS ----------------

blend_path : pathlib.Path = pathlib.Path(bpy.data.filepath)

if not blend_path.exists():
    raise Exception("Blend file must be saved before exporting!")

folder : pathlib.Path = blend_path.parent.resolve()
TARGET_FOLDER = pathlib.Path(folder, "visemes\\")   # Folder containing .target files

EXPORT_FILE = pathlib.Path(folder,blend_path.stem+"_exported.fbx")

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


def run_mpfb_merge(mesh_obj):
    make_active(mesh_obj)
    bpy.ops.mpfb.bake_shapekeys()          # MPFB merge keys
    bpy.ops.mpfb.delete_helpers()           # Remove helper geometry


def load_targets(mesh_obj):
    make_active(mesh_obj)
    target_files = [{"name": str(f.name)} for f in TARGET_FOLDER.iterdir() if f.suffix == ".target"]
    bpy.ops.mpfb.load_target(directory=str(TARGET_FOLDER), files=target_files)

def clean_shapekey_names(mesh_obj):
    keys = mesh_obj.data.shape_keys

    if not keys:
        print("No shapekeys found")
        return

    for key in keys.key_blocks:
        old_name = key.name

        # Skip Basis key
        if old_name.lower() == "basis":
            continue

        new_name = old_name

        # Remove prefix
        if new_name.lower().startswith("viseme_"):
            new_name = new_name[len("viseme_"):]

        # Uppercase
        new_name = new_name.upper()

        print(f"Renaming {old_name} → {new_name}")
        key.name = new_name

def export_selected_fbx():
    bpy.ops.export_scene.fbx(
        filepath=str(EXPORT_FILE),
        use_selection=True,
        apply_unit_scale=True,
        use_mesh_modifiers=False,
        add_leaf_bones=False,
        use_armature_deform_only=True
    )


# ================= PIPELINE =================

duplicates = duplicate_character()

# Separate mesh and armature
mesh_obj = next((o for o in duplicates if o.type == 'MESH'), None)
armature = next((o for o in duplicates if o.type == 'ARMATURE'), None)

if mesh_obj is None:
    raise Exception("No mesh found in duplicate!")

# Parent mesh to armature if needed
if armature:
    mesh_obj.parent = armature

# Run MPFB operations
run_mpfb_merge(mesh_obj)
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


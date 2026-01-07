from pprint import pprint
import numpy as np
import plotly.graph_objects as go
import numpy.typing as npt

data_text = ""
with open("grid.txt") as f:
    data_text = f.read()

layers: list[list[list[int]]] = []
current_layer: list[list[int]] = []
for line in data_text.split('\n'):
    line = line.strip()
    if line.startswith('[') and ']' in line:
        nums: list[int] = [int(x.strip()) for x in line.strip('[]').split(',')]
        current_layer.append(nums)
        if len(current_layer) == 16:
            layers.append(current_layer)
            current_layer = []

voxels: npt.NDArray[np.int_] = np.array(layers)

# Idk how the rest of this works but its just a visualizer so its fine for now

all_x, all_y, all_z = [], [], []
all_i, all_j, all_k = [], [], []
vertex_offset = 0

def is_solid(z: int, y: int, x: int) -> bool:
    if z < 0 or z >= voxels.shape[0]: return False
    if y < 0 or y >= voxels.shape[1]: return False
    if x < 0 or x >= voxels.shape[2]: return False
    return bool(voxels[z, y, x] == 1) # pyright: ignore

def add_face(vertices, face):
    global vertex_offset, all_x, all_y, all_z, all_i, all_j, all_k
    for v in vertices:
        all_x.append(v[0])
        all_y.append(v[1])
        all_z.append(v[2])
    for tri in face:
        all_i.append(tri[0] + vertex_offset)
        all_j.append(tri[1] + vertex_offset)
        all_k.append(tri[2] + vertex_offset)
    vertex_offset += len(vertices)

s = 0.5
face_count = 0

# Define faces as (normal_offset, vertex_template, triangles)
faces_def = [
    ((-1, 0, 0), [[-s,-s,-s], [+s,-s,-s], [+s,+s,-s], [-s,+s,-s]], [[0,1,2], [0,2,3]]),  # Bottom
    ((+1, 0, 0), [[-s,-s,+s], [+s,-s,+s], [+s,+s,+s], [-s,+s,+s]], [[0,2,1], [0,3,2]]),  # Top
    ((0, -1, 0), [[-s,-s,-s], [+s,-s,-s], [+s,-s,+s], [-s,-s,+s]], [[0,1,2], [0,2,3]]),  # Front
    ((0, +1, 0), [[-s,+s,-s], [+s,+s,-s], [+s,+s,+s], [-s,+s,+s]], [[0,2,1], [0,3,2]]),  # Back
    ((0, 0, -1), [[-s,-s,-s], [-s,+s,-s], [-s,+s,+s], [-s,-s,+s]], [[0,1,2], [0,2,3]]),  # Left
    ((0, 0, +1), [[+s,-s,-s], [+s,+s,-s], [+s,+s,+s], [+s,-s,+s]], [[0,2,1], [0,3,2]]),  # Right
]

for z in range(voxels.shape[0]):
    for y in range(voxels.shape[1]):
        for x in range(voxels.shape[2]):
            if voxels[z, y, x] != 1:
                continue
            
            for (dz, dy, dx), vertex_template, triangles in faces_def:
                if not is_solid(z + dz, y + dy, x + dx):
                    vertices = [[x + vx, y + vy, z + vz] for vx, vy, vz in vertex_template]
                    add_face(vertices, triangles)
                    face_count += 1

print(f"Rendered {face_count} exposed faces")

# Create mesh
fig = go.Figure(data=[go.Mesh3d(
    x=all_x, y=all_y, z=all_z,
    i=all_i, j=all_j, k=all_k,
    color='cyan',
    opacity=1.0
)])

# Set camera to make Y axis point up
camera = dict(
    up=dict(x=0, y=1, z=0),  # Y axis points up
    eye=dict(x=1.25, y=1.25, z=1.25)
)

fig.update_layout(
    scene=dict(
        xaxis_title='X (Right)',
        yaxis_title='Y (Up)',
        zaxis_title='Z (Towards Screen)',
        aspectmode='data',
        camera=camera
    )
)

fig.show()

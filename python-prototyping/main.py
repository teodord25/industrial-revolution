import os
import time

template = [
    [' ', 'X', ' ', 'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X', ' ', ' ', ' '],
    [' ', 'X', ' ', 'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
    [' ', 'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X', ' ', ' ', ' '],
    [' ', 'X', ' ', 'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X', ' ', ' '],
    [' ', ' ', ' ', 'X', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'X', ' ', ' ', ' '],
]


class Grid:
    start: tuple[int, int] = (3, 4)
    width: int = 15
    height: int = 5

    # nodes on the edge of the cloud
    on_edge: set[tuple[int, int]]
    occupied: set[tuple[int, int]]
    data: list[list[str]]

    def __init__(self):
        # self.data = [
        #     [" " for _ in range(self.width)]
        #     for _ in range(self.height)
        # ]
        #
        self.data = template

        self.set(self.start, "S")

        self.occupied = {self.start}
        self.on_edge = {self.start}

    def in_bounds(self, pos: tuple[int, int]) -> bool:
        x, y = pos[0], pos[1]

        if x not in range(0, self.width):
            return False

        if y not in range(0, self.height):
            return False

        return True

    def set(self, pos: tuple[int, int], chr: str):
        if not self.in_bounds(pos): return

        x, y = pos[0], pos[1]
        self.data[y][x] = chr

    def get(self, pos: tuple[int, int]):
        if not self.in_bounds(pos): return

        x, y = pos[0], pos[1]
        return self.data[y][x]

    def display(self):
        _ = os.system('clear')
        for row in self.data:
            print()
            for cell in row:
                print(f"[{cell}]", end="")
        print()
        time.sleep(0.2)

    def is_free(self, pos: tuple[int, int]):
        return self.get(pos) == " "

def next(pos: tuple[int, int], grid: Grid):
    l = (pos[0] - 1, pos[1])
    r = (pos[0] + 1, pos[1])
    u = (pos[0], pos[1] - 1)
    d = (pos[0], pos[1] + 1)

    def try_set(pos: tuple[int, int]):
        if pos in grid.occupied: return
        if not grid.is_free(pos): return

        grid.set(pos, "S")
        grid.occupied.add(pos)
        grid.on_edge.add(pos)

        grid.display()

    try_set(l)
    try_set(r)
    try_set(u)
    try_set(d)


grid = Grid()
LIMIT = 100

def traverse(pos: tuple[int, int], grid: Grid):
    if len(grid.occupied) > LIMIT: return

    next(pos, grid)

    # remove pos after processing
    grid.on_edge.discard(pos)

    edge_nodes = set(grid.on_edge)
    for node in edge_nodes:
        traverse(node, grid)

traverse(grid.start, grid)

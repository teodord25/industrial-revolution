import os
from queue import Queue
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
    to_check: Queue[tuple[int, int]]
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
        self.to_check = Queue()

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

def faces(pos: tuple[int, int]):
    return [
        (pos[0] - 1, pos[1]),
        (pos[0] + 1, pos[1]),
        (pos[0], pos[1] - 1),
        (pos[0], pos[1] + 1),
    ]


grid = Grid()
LIMIT = 100

def expand(start: tuple[int, int], grid: Grid):
    if len(grid.occupied) == 0: grid.occupied.add(start)
    if grid.to_check.qsize() == 0: grid.to_check.put(start)

    while len(grid.occupied) < LIMIT and grid.to_check.qsize() > 0:
        curr = grid.to_check.get() # pop node being processed

        for pos in faces(curr):

            if pos in grid.occupied: continue
            if not grid.is_free(pos): continue

            grid.set(pos, "S")
            grid.occupied.add(pos)
            grid.to_check.put(pos)

        grid.display()

expand(grid.start, grid)

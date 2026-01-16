namespace IndustrialRevolution.util;

using System.Collections.Generic;
using System;

public class General
{
    /// NOTE: "getNeighbors(T) T" must only return valid (bounded) coordinates
    public static void BreadthFirstSearch<Pos, XYZ>(
        HashSet<Pos> occupied, Queue<XYZ> toCheck, XYZ root, int limit,
        Func<XYZ, XYZ[]> getNeighbors, Func<XYZ, Pos> toPos,
        Action<XYZ, Pos, XYZ, Pos>? onVisit
    )
    {
        if (occupied.Count == 0) occupied.Add(toPos(root));
        if (toCheck.Count == 0) toCheck.Enqueue(root);

        while (occupied.Count < limit && toCheck.Count > 0)
        {
            XYZ currXYZ = toCheck.Dequeue();
            Pos currPos = toPos(currXYZ);

            foreach (XYZ neighXYZ in getNeighbors(currXYZ))
            {
                Pos neighPos = toPos(neighXYZ);

                if (occupied.Contains(neighPos)) continue;

                if (onVisit == null)
                {
                    occupied.Add(neighPos);
                    toCheck.Enqueue(neighXYZ);
                }
                else
                {
                    onVisit.Invoke(currXYZ, currPos, neighXYZ, neighPos);
                }
            }
        }
    }
}

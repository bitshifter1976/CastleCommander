using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Line
{
    public Vector2 Normal
    {
        get
        {
            float dx = End.x - Start.x;
            float dy = End.y - Start.y;
            // counter clockwise normal
            return new Vector2(dy, -dx);
        }
    }

    public Vector2 Center
    {
        get
        {
            Vector2 v = new Vector2();
            v.x = Math.Min(Start.x, End.x) + Math.Abs(End.x - Start.x) / 2;
            v.y = Math.Min(Start.y, End.y) + Math.Abs(End.y - Start.y) / 2;
            return v;
        }
    }

    public Vector2 Start { get; set; }

    public Vector2 End { get; set; }

    public Line(float x1, float y1, float x2, float y2)
    {
        Start = new Vector2(x1, y1);
        End = new Vector2(x2, y2);
    }

    public Line(Vector2 pos, Vector2 velocity)
    {
        Start = pos;
        End = Start + velocity;
    }

    public static Line[] GetFromRectangle(Rectangle rect)
    {
        Line[] lines = new Line[4];
        lines[0] = new Line(rect.Left, rect.Top, rect.Right, rect.Top);
        lines[1] = new Line(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
        lines[2] = new Line(rect.Left, rect.Bottom, rect.Left, rect.Top);
        lines[3] = new Line(rect.Right, rect.Top, rect.Right, rect.Bottom);
        return lines;
    }

    public static Line[] GetFromRectangle(RectangleF rect)
    {
        Line[] lines = new Line[4];
        lines[0] = new Line(rect.Left, rect.Top, rect.Right, rect.Top);
        lines[1] = new Line(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
        lines[2] = new Line(rect.Left, rect.Bottom, rect.Left, rect.Top);
        lines[3] = new Line(rect.Right, rect.Top, rect.Right, rect.Bottom);
        return lines;
    }

    public Line MakeEndless()
    {
        Vector2 direction = End - Start;
        direction.Normalize();
        direction *= 100;
        Vector2 newStart = Start - direction;
        Vector2 newEnd = End + direction;
        return new Line(newStart.x, newStart.y, newEnd.x, newEnd.y);
    }

    public bool ContainsPoint(Vector2 point)
    {
        float bottomY = Math.Min(Start.y, End.y);
        float topY = Math.Max(Start.y, End.y);
        bool heightIsRight = point.y >= bottomY &&
                             point.y <= topY;
        //Vertical line, slope is divideByZero error!
        if (Start.x == End.x)
        {
            if (point.x == Start.x && heightIsRight)
            {
                return true;
            }
            return false;
        }
        float slope = (End.x - Start.x) / (End.y - Start.y);
        bool onLine = (Start.y - point.y) == (slope * (Start.x - point.x));
        if (onLine && heightIsRight)
        {
            return true;
        }
        return false;
    }

    public Vector2 GetClosestPoint(Vector2 point)
    {
        Vector2 ap = point - Start;
        Vector2 ab = End - Start;
        float ab2 = ab.x * ab.x + ab.y * ab.y;
        float ap_ab = ap.x * ab.x + ap.y * ab.y;
        float t = ap_ab / ab2;

        if (t < 0.0f)
            return Start;
        if (t > 1.0f)
            return End;

        Vector2 closest = Start + ab * t;
        return closest;
    }

    public Vector2 GetDirectionVector()
    {
        Vector2 delta = End - Start;

        float distance = delta.magnitude;

        if (distance == 0.0f)
        {
            return Start;
        }
        Vector2 direction = delta / distance;
        return Start + direction * distance;
    }

    public List<Vector2> GetPoints(int quantity)
    {
        var points = new List<Vector2>();
        var xdiff = End.x - Start.x;
        var ydiff = End.y - Start.y;
        var slope = ydiff / xdiff;
        for (float i = 0; i < quantity; i++)
        {
            var y = slope == 0f ? 0 : ydiff * (i / quantity);
            var x = slope == 0f ? xdiff * (i / quantity) : y / slope;
            points.Add(new Vector2(x + Start.x, y + Start.y));
        }
        return points;
    }


    /// <summary>
    /// This is based off an explanation and expanded math presented by Paul Bourke:
    /// It takes two lines as inputs and returns true if they intersect, false if they don't.
    /// If they do, ptIntersection returns the point where the two lines intersect.  
    /// </summary>
    /// <param name="L1">The first line</param>
    /// <param name="line">The second line</param>
    /// <param name="ptIntersection">The point where both lines intersect (if they do).</param>
    /// <returns></returns>
    /// <remarks>See http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/</remarks>
    public bool IntersectLine(Line line, out Vector2 intersectionPoint)
    {
        intersectionPoint = new Vector2();
        // Denominator for ua and ub are the same, so store this calculation
        float d = (line.End.y - line.Start.y) * (End.x - Start.x) - (line.End.x - line.Start.x) * (End.y - Start.y);
        // Make sure there is not a division by zero - this also indicates that the lines are parallel.  
        // If n_a and n_b were both equal to zero the lines would be on top of each other (coincidental).  
        // This check is not done because it is not necessary for this implementation (the parallel check accounts for this).
        if (d == 0)
            return false;
        //n_a and n_b are calculated as seperate values for readability
        float n_a = (line.End.x - line.Start.x) * (Start.y - line.Start.y) - (line.End.y - line.Start.y) * (Start.x - line.Start.x);
        float n_b = (End.x - Start.x) * (Start.y - line.Start.y) - (End.y - Start.y) * (Start.x - line.Start.x);
        // Calculate the intermediate fractional point that the lines potentially intersect.
        float ua = n_a / d;
        float ub = n_b / d;
        // The fractional point will be between 0 and 1 inclusive if the lines
        // intersect.  If the fractional calculation is larger than 1 or smaller
        // than 0 the lines would need to be longer to intersect.
        if (ua >= 0d && ua <= 1d && ub >= 0d && ub <= 1d)
        {
            intersectionPoint.x = Start.x + (ua * (End.x - Start.x));
            intersectionPoint.y = Start.y + (ua * (End.y - Start.y));
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return Start + " - " + End;
    }
}

﻿using BMAPI;
using BMAPI.v1.HitObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_AutoBeatmapConstructor
{
    public class PatternsGenerator
    {
        private MapContextAwareness mapContext;
        private Random random;

        public PatternsGenerator(MapContextAwareness mapContext)
        {
            this.mapContext = mapContext;
            this.random = new Random();
        }

        public List<CircleObject> generateStreams(int points, int number, int spacing, int shift)
        {
            List<CircleObject> result = new List<CircleObject>();

            if (shift == 0)
            {
                shift = (int)(spacing * 1.0 / points / Math.Sqrt(2));
            }

            int shiftx = shift;
            int shifty = shift;

            double angle = 0;
            bool flag = false;
            for (int i = 0; i < number; ++i)
            {
                Point2 from = new Point2(mapContext.X, mapContext.Y);

                Point2 to = new Point2(from.X + spacing * (float)Math.Cos(angle), from.Y + spacing * (float)Math.Sin(angle));
                if (!PatternGenerator.checkCoordinateLimits(to))
                {
                    flag = true;
                }
                while (flag)
                {
                    angle = random.NextDouble() * Math.PI * 2;

                    to = new Point2(from.X + spacing * (float)Math.Cos(angle), from.Y + spacing * (float)Math.Sin(angle));

                    if (PatternGenerator.checkCoordinateLimits(to))
                    {
                        flag = false;
                    }
                }

                Point2 mid = new Point2((to.X + from.X) / 2, (to.Y + from.Y) / 2);

                double shift_delta = 0.2 + 0.4 * random.NextDouble();
                if (random.NextDouble() > 0.5)
                    shift_delta = -shift_delta;

                double delta = shift_delta * spacing;

                mid.X += (float)(delta * Math.Sin(angle));
                mid.Y += (float)(delta * Math.Cos(angle));

                var pattern = PatternGenerator.stream(points, from, to, mid);
                pattern[0].Type |= BMAPI.v1.HitObjectType.NewCombo;
                foreach(var obj in pattern)
                {
                    obj.StartTime = (int)mapContext.offset;
                    mapContext.offset += mapContext.bpm / 2;
                }

                result.AddRange(pattern);

                if (!PatternGenerator.checkCoordinateLimits(to.X + shift, to.Y + shift))
                {
                    Point2 next = PatternGenerator.findNextPosition(mapContext.X, mapContext.Y, shift);
                    shiftx = (int)(next.X - mapContext.X);
                    shifty = (int)(next.Y - mapContext.Y);
                }

                mapContext.X = (int)to.X + shift;
                mapContext.Y = (int)to.Y + shift;
            }

            return result;
        }

        public List<CircleObject> generateRandomJumps(int numberOfNotes, int spacing)
        {
            var result = new List<CircleObject>();

            for(int i = 0; i < numberOfNotes; ++i)
            {
                Point2 next = PatternGenerator.findNextPosition(mapContext.X, mapContext.Y, spacing);
                CircleObject note = new CircleObject();

                if (i % 4 == 0)
                    note.Type |= BMAPI.v1.HitObjectType.NewCombo;

                note.Location = next;
                result.Add(note);

                mapContext.X = (int)next.X;
                mapContext.Y = (int)next.Y;
            }

            foreach (var obj in result)
            {
                obj.StartTime = (int)mapContext.offset;
                mapContext.offset += mapContext.bpm;
            }

            return result;
        }

        public List<CircleObject> generatePolygons(int points, int number, int spacing, int rotation, int shift, bool randomize)
        {
            var result = new List<CircleObject>();

            double angle = 0;
            int X = mapContext.X;
            int Y = mapContext.Y;

            int shiftx = shift;
            int shifty = shift;

            for (int i = 0; i < number; ++i)
            {
                var ps = PatternGenerator.polygon(points, new Point2(X, Y), spacing, angle);

                if (randomize)
                {
                    ps = PatternGenerator.shuffleOrder(ps);
                }

                foreach (var p in ps)
                {
                    p.Type = BMAPI.v1.HitObjectType.Circle;
                }
                ps[0].Type |= BMAPI.v1.HitObjectType.NewCombo;

                foreach (var obj in ps)
                {
                    obj.StartTime = (int)mapContext.offset;
                    mapContext.offset += mapContext.bpm;
                }

                result.AddRange(ps);
                angle += PatternGenerator.ConvertToRadians(rotation);
                if (!PatternGenerator.checkCoordinateLimits(X, Y, points, spacing, shiftx, shifty))
                {
                    Point2 next = PatternGenerator.findNextPosition(X, Y, shift);
                    shiftx = (int)(next.X - X);
                    shifty = (int)(next.Y - Y);
                }
                X += shiftx;
                Y += shifty;
            }

            double nextShift = Math.Sqrt(2 * spacing * spacing * (1.0 - Math.Cos(Math.PI * 2 / points)));
            Point2 nextPosition = PatternGenerator.findNextPosition(X,Y, nextShift);

            mapContext.X = (int)nextPosition.X;
            mapContext.Y = (int)nextPosition.Y;

            return result;
        }
    }
}
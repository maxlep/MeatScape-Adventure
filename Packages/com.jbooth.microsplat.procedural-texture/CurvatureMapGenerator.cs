using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   // Adapted from Scrawk
   // https://github.com/Scrawk/Terrain-Topology-Algorithms
   public class CurvatureMapGenerator
   {
      // curve params
      public static float m_limit = 10000;

      // flow params
      private static int LEFT = 0;
      private static int RIGHT = 1;
      private static int BOTTOM = 2;
      private static int TOP = 3;
      private static float TIME = 0.2f;
      public static int m_iterations = 8;

      static float Horizontal(float dx, float dy, float dxx, float dyy, float dxy)
      {
         float kh = -2.0f * (dy * dy * dxx + dx * dx * dyy - dx * dy * dxy);
         kh /= dx * dx + dy * dy;

         if (float.IsInfinity(kh) || float.IsNaN(kh)) kh = 0.0f;

         if (kh < -m_limit) kh = -m_limit;
         if (kh > m_limit) kh = m_limit;

         kh /= m_limit;
         kh = kh * 0.5f + 0.5f;

         return kh;
      }

      static float Vertical(float dx, float dy, float dxx, float dyy, float dxy)
      {
         float kv = -2.0f * (dx * dx * dxx + dy * dy * dyy + dx * dy * dxy);
         kv /= dx * dx + dy * dy;

         if (float.IsInfinity(kv) || float.IsNaN(kv)) kv = 0.0f;

         if (kv < -m_limit) kv = -m_limit;
         if (kv > m_limit) kv = m_limit;

         kv /= m_limit;
         kv = kv * 0.5f + 0.5f;

         return kv;
      }

      static float Average(float dx, float dy, float dxx, float dyy, float dxy)
      {
         float kh = Horizontal(dx, dy, dxx, dyy, dxy);
         float kv = Vertical(dx, dy, dxx, dyy, dxy);

         return (kh + kv) * 0.5f;
      }



      // flow mapping
      static void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height)
      {

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               float flowOUT = outFlow[x, y, 0] + outFlow[x, y, 1] + outFlow[x, y, 2] + outFlow[x, y, 3];
               float flowIN = 0.0f;

               //Flow in is inflow from neighour cells. Note for the cell on the left you need 
               //thats cells flow to the right (ie it flows into this cell)
               flowIN += (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT];
               flowIN += (x == width - 1) ? 0.0f : outFlow[x + 1, y, LEFT];
               flowIN += (y == 0) ? 0.0f : outFlow[x, y - 1, TOP];
               flowIN += (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM];

               float ht = waterMap[x, y] + (flowIN - flowOUT) * TIME;
               if (ht < 0.0f) ht = 0.0f;

               //Result is net volume change over time
               waterMap[x, y] = ht;
            }
         }

      }

      static void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height)
      {

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               float dl = (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT] - outFlow[x, y, LEFT];

               float dr = (x == width - 1) ? 0.0f : outFlow[x, y, RIGHT] - outFlow[x + 1, y, LEFT];

               float dt = (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM] - outFlow[x, y, TOP];

               float db = (y == 0) ? 0.0f : outFlow[x, y, BOTTOM] - outFlow[x, y - 1, TOP];

               float vx = (dl + dr) * 0.5f;
               float vy = (db + dt) * 0.5f;

               velocityMap[x, y] = Mathf.Sqrt(vx * vx + vy * vy);
            }

         }

      }

      static void NormalizeMap(float[,] map, int width, int height)
      {

         float min = float.PositiveInfinity;
         float max = float.NegativeInfinity;

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               float v = map[x, y];
               if (v < min) min = v;
               if (v > max) max = v;
            }
         }

         float size = max - min;

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               float v = map[x, y];

               if (size < 1e-12f)
                  v = 0;
               else
                  v = (v - min) / size;

               map[x, y] = v;
            }
         }

      }

      static void FillWaterMap(float amount, float[,] waterMap, int width, int height)
      {
         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               waterMap[x, y] = amount;
            }
         }
      }

      static void ComputeOutflow(float[,] waterMap, float[,,] outFlow, float[,] heightMap, int width, int height)
      {

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               int xn1 = (x == 0) ? 0 : x - 1;
               int xp1 = (x == width - 1) ? width - 1 : x + 1;
               int yn1 = (y == 0) ? 0 : y - 1;
               int yp1 = (y == height - 1) ? height - 1 : y + 1;

               float waterHt = waterMap[x, y];
               float waterHts0 = waterMap[xn1, y];
               float waterHts1 = waterMap[xp1, y];
               float waterHts2 = waterMap[x, yn1];
               float waterHts3 = waterMap[x, yp1];

               float landHt = heightMap[x, y];
               float landHts0 = heightMap[xn1, y];
               float landHts1 = heightMap[xp1, y];
               float landHts2 = heightMap[x, yn1];
               float landHts3 = heightMap[x, yp1];

               float diff0 = (waterHt + landHt) - (waterHts0 + landHts0);
               float diff1 = (waterHt + landHt) - (waterHts1 + landHts1);
               float diff2 = (waterHt + landHt) - (waterHts2 + landHts2);
               float diff3 = (waterHt + landHt) - (waterHts3 + landHts3);

               //out flow is previous flow plus flow for this time step.
               float flow0 = Mathf.Max(0, outFlow[x, y, 0] + diff0);
               float flow1 = Mathf.Max(0, outFlow[x, y, 1] + diff1);
               float flow2 = Mathf.Max(0, outFlow[x, y, 2] + diff2);
               float flow3 = Mathf.Max(0, outFlow[x, y, 3] + diff3);

               float sum = flow0 + flow1 + flow2 + flow3;

               if (sum > 0.0f)
               {
                  //If the sum of the outflow flux exceeds the amount in the cell
                  //flow value will be scaled down by a factor K to avoid negative update.
                  float K = waterHt / (sum * TIME);
                  if (K > 1.0f) K = 1.0f;
                  if (K < 0.0f) K = 0.0f;

                  outFlow[x, y, 0] = flow0 * K;
                  outFlow[x, y, 1] = flow1 * K;
                  outFlow[x, y, 2] = flow2 * K;
                  outFlow[x, y, 3] = flow3 * K;
               }
               else
               {
                  outFlow[x, y, 0] = 0.0f;
                  outFlow[x, y, 1] = 0.0f;
                  outFlow[x, y, 2] = 0.0f;
                  outFlow[x, y, 3] = 0.0f;
               }

            }
         }

      }


      public static void CreateMap(float[,] heights, Texture2D curveMap)
      {
         int width = curveMap.width;
         int height = curveMap.height;
         float ux = 1.0f / (width - 1.0f);
         float uy = 1.0f / (height - 1.0f);


         // flow
         float[,] waterMap = new float[width, height];
         float[,,] outFlow = new float[width, height, 4];
         FillWaterMap(0.0001f, waterMap, width, height);

         for (int i = 0; i < m_iterations; i++)
         {
            ComputeOutflow(waterMap, outFlow, heights, width, height);
            UpdateWaterMap(waterMap, outFlow, width, height);
         }

         float[,] velocityMap = new float[width, height];

         CalculateVelocityField(velocityMap, outFlow, width, height);
         NormalizeMap(velocityMap, width, height);

         for (int y = 0; y < height; y++)
         {
            for (int x = width - 1; x >= 0; x--)
            {
            
               int xp1 = (x == width - 1) ? x : x + 1;
               int xn1 = (x == 0) ? x : x - 1;

               int yp1 = (y == height - 1) ? y : y + 1;
               int yn1 = (y == 0) ? y : y - 1;

               float v = heights[x, y];

               float l = heights[xn1, y];
               float r = heights[xp1, y];

               float b = heights[x, yn1];
               float t = heights[x, yp1];

               float lb = heights[xn1, yn1];
               float lt = heights[xn1, yp1];

               float rb = heights[xp1, yn1];
               float rt = heights[xp1, yp1];

               float dx = (r - l) / (2.0f * ux);
               float dy = (t - b) / (2.0f * uy);

               float dxx = (r - 2.0f * v + l) / (ux * ux);
               float dyy = (t - 2.0f * v + b) / (uy * uy);

               float dxy = (rt - rb - lt + lb) / (4.0f * ux * uy);

               float curve = 0.0f;

               curve = Average(dx, dy, dxx, dyy, dxy);
               float flow = velocityMap[x, y];

               curveMap.SetPixel(y, x, new Color(0, curve, 0, flow));
            }

         }

         curveMap.Apply();

      }
   }

}
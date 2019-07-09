using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MapRegion
{
    public static class RegionUtility
    {
        public struct Area
        {
            public int XMin { get; set; }
            public int YMin { get; set; }
            public int XMax { get; set; }
            public int YMax { get; set; }
            public int Size { get; set; }
            public override string ToString()
            {
                return string.Format("Size {0} XMin {1} XMax {2}", Size, XMin, XMax);
            }
        }
        //将Byte转换为结构体类型
        public static byte[] StructToBytes(object structObj, int size)
        {
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷贝到byte 数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return bytes;

        }

        //将Byte转换为结构体类型
        public static object ByteToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            //分配结构体内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        public static bool IsPointInRect(Vector2 pt, Rect rc)
        {
            return (pt.x >= rc.xMin && pt.x < rc.xMax && pt.y >= rc.yMin && pt.y < rc.yMax);
        }

        public static void a3d_TransformMatrix(Vector3 vecDir, Vector3 vecUp, Vector3 vecPos, out Matrix4x4 mat)
        {
            Vector3 vecXAxis, vecYAxis, vecZAxis;

            vecZAxis = vecDir;
            vecZAxis.Normalize();
            vecYAxis = vecUp;
            vecYAxis.Normalize();
            vecXAxis = Vector3.Cross(vecYAxis, vecZAxis);
            vecXAxis.Normalize();

            mat = new Matrix4x4();
            mat[0, 0] = vecXAxis.x;
            mat[1, 0] = vecXAxis.y;
            mat[2, 0] = vecXAxis.z;
            mat[3, 0] = 0.0f;

            mat[0, 1] = vecYAxis.x;
            mat[1, 1] = vecYAxis.y;
            mat[2, 1] = vecYAxis.z;
            mat[3, 1] = 0.0f;

            mat[0, 2] = vecZAxis.x;
            mat[1, 2] = vecZAxis.y;
            mat[2, 2] = vecZAxis.z;
            mat[3, 2] = 0.0f;

            mat[0, 3] = vecPos.x;
            mat[1, 3] = vecPos.y;
            mat[2, 3] = vecPos.z;
            mat[3, 3] = 1.0f;
        }

        // Returns area of the largest rectangle with all 1s in 
        // A[][] 
        public static Area GetMaxRectangle(int rowCount, int columnCount, int[][] matrix)
        {
            // Calculate area for first row and initialize it as 
            // result 
            var maxArea = MaxHist(rowCount, columnCount, matrix[0], 0);
            int result = maxArea.Size;

            // iterate over row to find maximum rectangular area 
            // considering each row as histogram 
            for (int i = 1; i < rowCount; i++)
            {

                for (int j = 0; j < columnCount; j++)
                {
                    // if A[i][j] is 1 then add A[i -1][j] 
                    if (matrix[i][j] == 1)
                    {
                        matrix[i][j] += matrix[i - 1][j];
                    }
                }



                // Update result if area with current row (as last 
                // row of rectangle) is more 
                var tmpArea = MaxHist(rowCount, columnCount, matrix[i], i);
                if (tmpArea.Size > result)
                {
                    maxArea = tmpArea;
                }
                result = Math.Max(result, tmpArea.Size);

            }

            return maxArea;
        }

        // Finds the maximum area under the histogram represented 
        // by histogram. See below article for details. 
        // https://www.geeksforgeeks.org/largest-rectangle-under-histogram/ 
        private static Area MaxHist(int rowCount, int columnCount, int[] row, int roleIndex)
        {
            // Create an empty stack. The stack holds indexes of 
            // hist[] array/ The bars stored in stack are always 
            // in increasing order of their heights. 
            Stack<int> result = new Stack<int>();

            int top_val;     // Top of stack 

            // Initialize max area in current 
            // row (or histogram) 
            Area maxArea = new Area
            {
                YMax = roleIndex
            };

            int area = 0; // Initialize area with current top 

            // Run through all bars of given histogram (or row) 
            int i = 0;

            while (i < columnCount)
            {
                // If this bar is higher than the bar on top stack, 
                // push it to stack 
                if (result.Count <= 0 || row[result.Peek()] <= row[i])
                    result.Push(i++);

                else
                {
                    // If this bar is lower than top of stack, then 
                    // calculate area of rectangle with stack top as 
                    // the smallest (or minimum height) bar. 'i' is 
                    // 'right index' for the top and element before 
                    // top in stack is 'left index' 
                    top_val = row[result.Peek()];
                    result.Pop();
                    area = top_val * i;

                    if (result.Count != 0)
                        area = top_val * (i - result.Peek() - 1);

                    if (area > maxArea.Size)
                    {

                        maxArea.XMax = i - 1;
                        if (result.Count > 0)
                        {
                            maxArea.XMin = result.Peek() + 1;
                        }
                        else
                        {
                            maxArea.XMin = 0;
                        }

                    }

                    maxArea.Size = Math.Max(area, maxArea.Size);
                }
            }

            // Now pop the remaining bars from stack and calculate 
            // area with every popped bar as the smallest bar 
            while (result.Count > 0)
            {
                top_val = row[result.Peek()];
                result.Pop();
                area = top_val * i;
                if (result.Count > 0)
                {
                    area = top_val * (i - result.Peek() - 1);
                }

                if (area > maxArea.Size)
                {
                    if (result.Count > 0)
                    {
                        maxArea.XMax = (i - result.Peek()) > (i - 1) ? (i - 1) : (i - result.Peek());
                        maxArea.XMin = result.Peek() + 1;
                    }
                    else
                    {
                        maxArea.XMax = i - 1;
                        maxArea.XMin = 0;
                    }
                }

                maxArea.Size = Math.Max(area, maxArea.Size);
            }

            maxArea.YMin = (maxArea.YMax + 1 - (maxArea.Size / (maxArea.XMax - maxArea.XMin + 1)));
            //Console.WriteLine("maxArea.Size {0}, maxArea.XMin {1}, maxArea.XMax {2}, maxArea.YMin {3}", maxArea.Size, maxArea.XMin, maxArea.XMax, maxArea.YMin, maxArea.YMax);
            return maxArea;
        }

    }
}

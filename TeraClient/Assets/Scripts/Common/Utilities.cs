using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using UnityEngine;
using System.Text.RegularExpressions;

#if SERVER_USE
using System.Net;
using System.Diagnostics;
#endif
// ReSharper disable once CheckNamespace
namespace Common
{
	public static class Utilities
	{

#if SERVER_USE
		private static LogHelp log = LogHelp.getLogger(typeof(Utilities));
#endif
		public const float FloatDeviation = 1e-6f;
        public const char EditorSplitChar = '*';
		////两套职业掩码的对应关系：
		////职业掩码：profession表中是1、2、3、4，
		////某职业是否可使用这件物品时的职业掩码是1、2、4、8 因为历史遗留问题，所以 有两套
  //      //2018/04/02 与职业表进行统一，由 1,4,2,8 --->1,2,4,8
		//private static Dictionary<int, int> Profession2Mask = new Dictionary<int, int>()
		//{
		//	{ 1, 1}, {2, 2 }, {3, 4 }, {4, 8 },{ 5,16}
		//};

        //属性与模块属性对应关系
        private static Dictionary<int, int> ModulePropertyContrast = new Dictionary<int, int>()
        {
            {1,144},
            {3,145},
            {5,146},
            {7,147},
            {9,148},
            {11,149},
            {19,150},
            {21,151},
            {23,152},
            {25,153},
            {27,154},
            {29,155},
            {31,156},
            {33,157},
            {35,158},
            {37,159},
            {39,160},
            {41,161},
            {43,162},
            {45,163},
            {111,165},
            {115,166},
            {117,167},
            {119,168},
            {121,169},
            {123,170},
            {125,171},
            {59,172},
            {61,173},
            {63,174},
            {101,175},
            {104,176},
            {106,177},
            {109,178}
        };


        public static int getModulePropertyId(int propertyId)
        {
            int ret;
            if (ModulePropertyContrast.TryGetValue(propertyId, out ret))
                return ret;
            return 0;
        }

        public static bool ValidParamVector3(Vector3 v)
		{
			if (!ValidParamFloat(v.x)) return false;
			if (!ValidParamFloat(v.y)) return false;
			if (!ValidParamFloat(v.z)) return false;
			return true;
		}

#if SERVER_USE
        public static bool ValidParamVector3(PB.vector3 v)
		{
			if (!ValidParamFloat(v.x)) return false;
			if (!ValidParamFloat(v.y)) return false;
			if (!ValidParamFloat(v.z)) return false;
			return true;
		}
#endif

        //保留几位小数
        public static Vector3 CastVector3(Vector3 src, int digit)
        {
            if (digit <= 0 ) return src;
            Vector3 tmp = Vector3.zero;
            tmp.x = (float)Math.Round(src.x, digit);
            tmp.y = (float)Math.Round(src.y, digit);
            tmp.z = (float)Math.Round(src.z, digit);
            return tmp;
        }

        public static bool ValidParamFloat(float f)
		{
			if (float.IsNaN(f)) return false;
			if (float.IsInfinity(f)) return false;
			return true;
		}

		public static int GetHigh(long l)
		{
			return (int)(l >> 32);
		}

		public static uint GetLow(long l)
		{
			return (uint)(l & 0xFFFFFFFFL);
		}

        public static bool FloatEqual(float l, float r)
        {
            return (Math.Abs(l - r) <= FloatDeviation);
        }

        public static Vector3 RotationToOrientation(Vector3 rotation)
		{
#if SERVER_USE
			var quaternion = MTools.Quaternion.Euler(rotation.x, rotation.y, rotation.z);
#else
			var quaternion = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
#endif
			return quaternion * Vector3.forward;
		}

		public static Vector3 OrientationToRotation(Vector3 orientation)
		{
#if SERVER_USE
			var quaternion = MTools.Quaternion.identity;
#else
			var quaternion = Quaternion.identity;
#endif
			quaternion.SetFromToRotation(Vector3.forward, orientation);
			return quaternion.eulerAngles;
		}

        public static Vector3 GetRandomPostionRingRadius(System.Random random, Vector3 originPosition, float randomRadius,float ringRadius)
        {
            ringRadius = Mathf.Clamp01(ringRadius);
            var radius = ((float)random.NextDouble() * ringRadius + (1-ringRadius)) * randomRadius;
            var angle = (float)random.NextDouble() * 2 * Mathf.PI;
            var offsetX = radius * Mathf.Cos(angle);
            var offsetZ = radius * Mathf.Sin(angle);
            var position = originPosition;
            position.x += offsetX;
            position.z += offsetZ;
            return position;
        }

		public static Vector3 GetRandomPostion(System.Random random, Vector3 originPosition, float randomRadius)
		{
			return GetRandomPostionRingRadius(random,originPosition,randomRadius,0.5f);
		}

		public static Vector3 GetRandomPostion(System.Random random, Vector3 originPosition, float length, float width)
		{
			if (length < 0) length = 0;
			if (width < 0) width = 0;
			var offsetX = (float)random.NextDouble() * length - length / 2;
			var offsetZ = (float)random.NextDouble() * width - width / 2;
			var position = originPosition;
			position.x += offsetX;
			position.z += offsetZ;
			return position;
		}

		public static DateTime ConvertIntDateTime(int second)
		{
#if SERVER_USE
			var startDateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#else
			var startDateTime = TimeZone.CurrentTimeZone.ToLocalTime(Main.DateTimeBegin);
#endif
			var dateTime = startDateTime.AddSeconds(second);
			return dateTime;
		}

		public static int ConvertDateTimeInt(DateTime dateTime)
		{
#if SERVER_USE
			var startDateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#else
			var startDateTime = TimeZone.CurrentTimeZone.ToLocalTime(Main.DateTimeBegin);
#endif
			var second = (dateTime - startDateTime).TotalSeconds;
			return (int)second;
		}

        public static long UtcMillisecond(DateTime dateTime, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            DateTime utcBegin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long utcLocalTimeGap = (long)(DateTime.Now - DateTime.UtcNow).TotalMilliseconds;

            if (dateTimeKind == DateTimeKind.Utc)
            {
                return (long)(dateTime - utcBegin).TotalMilliseconds;
            }
            else
            {
                return (long)(dateTime - utcBegin).TotalMilliseconds - utcLocalTimeGap;
            }

        }


        public static byte[] GetDeviceUniqueId()
		{
			foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
			{
				var mac = adapter.GetPhysicalAddress().ToString();
				if (!string.IsNullOrEmpty(mac))
				{
					return adapter.GetPhysicalAddress().GetAddressBytes();
				}
			}
			return null;
		}

#if SERVER_USE
        /// <summary>
        /// 后端增加了PB.vector3和Vector3之间的类型转换操作符，不必再使用该函数
        /// </summary>
        /// <param name="pbpos"></param>
        /// <returns></returns>
		public static Vector3 ChangePbVecToVector3(PB.vector3 pbpos)
		{
			var pos = Vector3.zero;
			if (pbpos == null) return pos;
			pos.Set(pbpos.x, pbpos.y, pbpos.z);
			return pos;
		}

        /// <summary>
        /// 后端增加了PB.vector3和Vector3之间的类型转换操作符，不必再使用该函数
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static PB.vector3 ChangeVector3ToPbVec(Vector3 pos)
		{
            PB.vector3 _PbPos = new PB.vector3();
            _PbPos.x = pos.x;
            _PbPos.y = pos.y;
            _PbPos.z = pos.z;

			return _PbPos;
		}

        public static void SetPbVector3(PB.vector3 v, Vector3 pos)
        {
            if (v == null) return;
            v.x = pos.x;
            v.y = pos.y;
            v.z = pos.z;
        }

        public static PB.vector3[] ChangeVector2ListToPbVec3List(Vector2[] pos)
        {
            int len = pos.Length;
            PB.vector3[] pbArray = new PB.vector3[len];
            for (int i = 0; i < len; ++i)
            {
                pbArray[i].x = pos[i].x;
                pbArray[i].y = 0;
                pbArray[i].z = pos[i].y;
            }
            return pbArray;
        }
#endif

        public static Vector3[] ChangeVector2ListToVector3List(Vector2[] pos)
        {
            int len = pos.Length;
            Vector3[] pbArray = new Vector3[len];
            for (int i = 0; i < len; ++i)
            {
                pbArray[i].x = pos[i].x;
                pbArray[i].y = 0;
                pbArray[i].z = pos[i].y;
            }
            return pbArray;
        }

        public static float GetMagnitudeH(this Vector3 v)
		{
			return (float)Math.Sqrt(v.x * v.x + v.z * v.z);
		}

        public static float SqrDistanceH(Vector3 v1, Vector3 v2)
        {
            return (float)((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z) * (v1.z - v2.z));
        }

		public static float DistanceH(Vector3 v1, Vector3 v2)
		{
			return (float)Math.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z) * (v1.z - v2.z));
		}
#if SERVER_USE
        public static float DistanceH(PB.vector3 v1, Vector3 v2)
        {
            if (v1 == null) return 0;
            return (float)Math.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z) * (v1.z - v2.z));
        }
        public static float DistanceH(Vector3 v1, PB.vector3 v2)
        {
            if (v2 == null) return 0;
            return (float)Math.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z) * (v1.z - v2.z));
        }
#endif
        public static float SquareDistanceH(float x0, float z0, float x1, float z1)
        {
            return (x0 - x1) * (x0 - x1) + (z0 - z1) * (z0 - z1);
        }

		public static bool IsLengthNotZero(this Vector3 v)
		{
			return (float)Math.Sqrt(v.x * v.x + v.z * v.z) > 1e-6;
		}

        public static Vector3 GetAOISightLt(Vector3 pos, Vector3 extent)
        {
            Vector3 sight = Vector3.zero;
            sight.x = pos.x - extent.x;
            sight.z = pos.z + extent.z;
            return sight;
        }
        public static Vector3 GetAOISightRb(Vector3 pos, Vector3 extent)
        {
            Vector3 sight = Vector3.zero;
            sight.x = pos.x + extent.x;
            sight.z = pos.z - extent.z;
            return sight;
        }

#if SERVER_USE
        //检查名字有效性
        //[\u4e00-\u9fa5]	中文
        //[A-Za-z0-9]		字母及数字
        //[\uAC00-\uD7AF]	韩文
        //[\u3040-\u30FF]	日文
        //[\u0024-\u27BE]	主流标点
        //[\uFF00-\uFFEF]	全角ASCII、全角中英文标点、半宽片假名、半宽平假名、半宽韩文字母
        public static Data.ServerMessageBase CheckName(string name)
        {
            if (!ContainMainWord(name))
                return Data.ServerMessageBase.RoleNameNotNumber;

            if (!IsValidWord(name))
                return Data.ServerMessageBase.NameInvalid;

            return Data.ServerMessageBase.Success;
        }
#endif

        public static bool ContainMainWord(string name)
        {
            //不能为纯数字、符号
            var regex1 = new Regex(@"([\u4e00-\u9fa5]|[A-Za-z]|[\uAC00-\uD7AF]|[\u3040-\u30FF])");
            bool containMainWord = regex1.IsMatch(name);
            return containMainWord;
        }

        public static bool IsValidWord(string name)
        {
            //在规定字符范围内
            Regex regex2 = new Regex(@"^([\u4e00-\u9fa5]|[A-Za-z0-9]|[\uAC00-\uD7AF]|[\u0020-\u27BE]|[\uFF00-\uFFEF])+$");
            bool isValid = regex2.IsMatch(name);
            return isValid;
        }

        public static bool IsValidWord_KR(string name)
        {
            //在规定字符范围内
            //韩文版只允许韩文，字母，数字
            Regex regex2 = new Regex(@"^([A-Za-z0-9]|[\uAC00-\uD7AF])+$");
            bool isValid = regex2.IsMatch(name);
            return isValid;
        }


#if SERVER_USE

		//获得当前时间
		public static DateTime GetTimeNow()
		{
#if GAMESERVER
			return GameServer.Game.ECSSystem.SWorld.Instance.getTimerManager().LocalNow();
#elif WORLDSERVER
			return  WorldServer.WorldServer.Instance.Timer.LocalNow();
#else
			return  DateTime.Now;
#endif
		}

		//时间 每天的某个小时 某分钟触发
		public static DateTime GetTime(int hour, int min = 0)
		{
			var tempTime = GetTimeNow().Date.AddHours(hour);
			tempTime = tempTime.AddMinutes(min);
			return tempTime;
		}

		//时间 每周的某个小时 某分钟触发
		public static DateTime GetTime(int week, int hour, int min)
		{
			var tempTime = GetTimeNow().Date.AddDays(week - Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d")));
			tempTime = tempTime.AddHours(hour).AddMinutes(min);
			return tempTime;
		}


		//下次触发时间 每天的某个小时 某分钟触发
		public static DateTime GetNextActiveTime(int hour, int min = 0)
		{
			var tempTime = GetTime(hour, min);
			if (GetTimeNow() > tempTime)
			{
				tempTime = tempTime.AddDays(1);
			}
			return tempTime;
		}

		//下次触发时间 每天的某个小时 某分钟触发
		public static DateTime GetNextActiveTime(int week, int hour, int min)
		{
			var tempTime = GetTime(week, hour, min);
			if (GetTimeNow() > tempTime)
			{
				tempTime = tempTime.AddDays(7);
			}
			return tempTime;
		}

		/// <summary>
		/// 是否触发时间
		/// </summary>
		/// <param name="lastRefTime"> 上次刷新时间</param>
		/// <param name="refHour">刷新的整点时间</param>
		/// <returns></returns>
		public static bool IsTriggerTime(DateTime lastRefTime, int refHour)
		{
			var curTime = GetTimeNow();
			var dayTime = GetTime(refHour);
			if (lastRefTime < dayTime)
			{
				var timeSpan = dayTime - lastRefTime;
				if (timeSpan.Days > 1)
					return true;

				if (curTime > dayTime)
					return true;
			}
			return false;
		}

		//判断某个点是否在某个圆圈内
		public static bool IsInCircle(Vector3 pos, Vector3 centerPos, float radius)
		{
			return (DistanceH(pos, centerPos) <= radius);
		}

		//获得字符串输入长度
		public static int GetInputLength(string content)
		{
            return content.Length;
			int count = 0;
			for(int i=0; i<content.Length; i++)
			{
				if( (content[i] >= 0 && content[i] < 127) || (content[i] >= 194 && content[i] <= 244) )
				{
					count += 1;
				}
				else
				{
					count += 2;
				}
			}
			return count;
		}

        static System.Random rdInt;
        /// <summary>
        /// 在指定区间内[min, max)获取随机数
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="upBound">是否包含上边界</param>
        /// <returns></returns>
        public static int GetNextRandomNum(int min, int max, bool upBound = false)
        {
            if(rdInt == null)
            {
                rdInt = new System.Random((int)DateTime.Now.ToFileTimeUtc());
            }
            if(upBound)
            {
                max = max + 1;
            }
            int ret = rdInt.Next(min, max);
            return ret;
        }

        //判断字符串是否为纯数字，并out int值
        public static bool isNumberic(string message, out int result)
        {
            System.Text.RegularExpressions.Regex rex =
            new System.Text.RegularExpressions.Regex(@"^\d+$");
            result = -1;
            if (rex.IsMatch(message))
            {
                int.TryParse(message, out result);
                return true;
            }
            else
                return false;
        }

        //判断字符串是否为纯数字，并out long值
        public static bool isNumberic(string message, out long result)
        {
            System.Text.RegularExpressions.Regex rex =
            new System.Text.RegularExpressions.Regex(@"^\d+$");
            result = -1;
            if (rex.IsMatch(message))
            {
                bool ret = long.TryParse(message, out result);
                return ret;
            }
            else
                return false;
        }

        // 删除文本内容中以"//"开头或用"/**/"包裹的部分, 即:删除通常意义上的注释内容
        public static string removeComment(string s)
        {
            return Regex.Replace
            (
              s,
              @"(?ms)""[^""]*""|//.*?$|/\*.*?\*/",
              delegate (Match m)
              {
                  switch (m.Value.Substring(0, 2))
                  {
                      case "//": return "";
                      case "/*": return " ";
                      default: return m.Value;
                  }
              }
            );
        }

        // 获取本机所有IP, 基于DNS
        public static IPAddress getLocalIp(int idx = 1)
        {
            string hostName = Dns.GetHostName();
            IPAddress ipaddr = Dns.GetHostAddresses(hostName)[idx];
            return ipaddr;
        }

        public static int getProcessId()
        {
            Process p = Process.GetCurrentProcess();
            return p.Id;
        }

		public static List<int> getListInt(string str)
		{
			if (string.IsNullOrEmpty(str))
				return new List<int>();

			string[] path = str.Split(EditorSplitChar);
			List<string> listS = new List<string>(path);
			List<int> ls = listS.ConvertAll<int>(x => Convert.ToInt32(x));

			return ls;
		}

        public static List<float> getListFloat(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new List<float>();

            string[] path = str.Split(EditorSplitChar);
            List<string> listS = new List<string>(path);
            List<float> ls = listS.ConvertAll<float>(x => Convert.ToSingle(x));

            return ls;
        }


        public static List<string> getListString(string str,char splitStr = EditorSplitChar)
        {
            if (string.IsNullOrEmpty(str))
                return new List<string>();

            string[] path = str.Split(splitStr);
            List<string> listS = new List<string>(path);

            return listS;
        }

        //从List<string>转为string (1,2,3)
        public static string getStringFromListString(List<string> ls, char splitStr = EditorSplitChar)
        {
            string ret = "";
            if (null == ls || ls.Count <= 0)
                return ret;

            for (int i = 0; i < ls.Count; i++)
                ret += i == 0 ? ls[i].ToString() : splitStr + ls[i].ToString();

            return ret;
        }

        public static List<int> getListInt(string str, char splitStr = EditorSplitChar)
        {
            if (string.IsNullOrEmpty(str))
                return new List<int>();

            string[] path = str.Split(splitStr);
            List<string> listS = new List<string>(path);
            List<int> ls = listS.ConvertAll<int>(x => Convert.ToInt32(x));

            return ls;
        }


        //从List<int>转为string (1,2,3)
        public static string getStringFromListInt(List<int> ls)
        {
            string ret = "";
            if (null == ls || ls.Count <= 0)
                return ret;

            for(int i =0;i < ls.Count; i ++)
                ret += i == 0 ? ls[i].ToString() : EditorSplitChar + ls[i].ToString();

            return ret;
        }

        //获取RankData 主键
        public static  int getRankDataKey(int subjectId, int rankId, int subjectType)
        {
            return (subjectId * 1000) + (rankId * 10) + subjectType;
        }

        //从RankData主键 获取关键信息
        public static bool getInfoFromRankDataKey(long key, out int subjectId, out int rankId, out int subjectType)
        {
            subjectId = -1;
            rankId = -1;
            subjectType = -1;

            subjectId = (int)(key / 1000);
            rankId = (int)((key % 1000) / 10);
            subjectType = (int)(key % 10);

            if (subjectId <= 0 || rankId <= 0 || subjectType < 0)
                return false;

            return true;
        }


        //是否符合职业使用
        public static bool IsProfession(int mask, int professionid)
		{
            if (professionid <= (int)Data.Profession.Profession_default || professionid >= (int)Data.Profession.Profession_Max)
                return false;

            int pro2Mask = 1 << professionid - 1;
            return (pro2Mask & mask) == pro2Mask;
		}
#endif
        //         public static byte[] Serialize<T>(T value)
        //         {
        //             using (var memory = new MemoryStream())
        //             {
        //                 try
        //                 {
        //                     Serializer.Serialize(memory, value);
        //                     return memory.ToArray();
        //                 }
        //                 catch (Exception ex)
        //                 {
        // #if SERVER_USE
        //                     log.errorFormat("ex:{0}, type:{1}", ex.ToString(), typeof(T));
        // #else
        //                     HobaDebuger.LogErrorFormat("ex:{0}, type:{1}", ex.ToString(), typeof(T));
        // #endif
        //                     return null;
        //                 }
        //             }
        //         }
        //         public static T Deserialize<T>(byte[] data)
        //         {
        //             try
        //             {
        //                 if (data == null)
        //                     return default(T);
        // 
        //                 using (var stream = new MemoryStream(data))
        //                 {
        //                     return Serializer.Deserialize<T>(stream);
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        // #if SERVER_USE
        //                 log.errorFormat("ex:{0}, type:{1}", ex.ToString(), typeof(T));
        // #else
        //                 HobaDebuger.LogErrorFormat("ex:{0}, type:{1}", ex.ToString(), typeof(T));
        // #endif
        //                 return default(T);
        //             }
        //         }
        // 
        //遍历目录下的文件
        public static void ListFiles(FileSystemInfo info, List<string> fileNameList)
        {
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件
                if (file != null)
                    fileNameList.Add(file.FullName);
                else
                    ListFiles(files[i], fileNameList);
            }
        }

        public static void ListFiles(FileSystemInfo info, string ext, List<string> fileNameList)
        {
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件
                if (file != null)
                {
                    if (ext == "*" || Path.GetExtension(file.Name) == ext)
                        fileNameList.Add(file.FullName);
                }
                else
                {
                    ListFiles(files[i], ext, fileNameList);
                }
            }
        }

        public static System.Collections.Generic.List<int> Split(string token)
        {
            System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
            if (string.IsNullOrEmpty(token))
                return list;
            string[] strs = token.Split(Common.Utilities.EditorSplitChar);
            for (int i = 0; i < strs.Length; ++i)
            {
                list.Add(int.Parse(strs[i]));
            }
            return list;
        }

        public static System.Collections.Generic.HashSet<int> SplitHashSet(string token)
        {
            System.Collections.Generic.HashSet<int> set = new System.Collections.Generic.HashSet<int>();
            if (string.IsNullOrEmpty(token))
                return set;
            string[] strs = token.Split(Common.Utilities.EditorSplitChar);
            for (int i = 0; i < strs.Length; ++i)
            {
                set.Add(int.Parse(strs[i]));
            }
            return set;
        }

        public static int FixIntValueAdd(int srcValue, int addValue)
        {
            long tmpValue = (long)srcValue + addValue;
            return (int)Math.Min(tmpValue, int.MaxValue);
        }
    }
}

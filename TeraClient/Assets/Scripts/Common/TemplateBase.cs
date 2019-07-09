
#if SERVER_USE
using Google.Protobuf;
#else
using System.IO;
using ProtoBuf;
using System;
#endif


// ReSharper disable once CheckNamespace
namespace Template
{

#if SERVER_USE
    public interface TemplateBase
    {
    }

    public class TemplateBaseT<T> : TemplateBase
        where T : class, IMessage<T>, new()
    {
        public virtual T DeepClone()
        {
            IMessage<T> msg = this as IMessage<T>;
            return msg.Clone();
        }
    }

#else
    public struct STranslateValue
    {
        public string _field;       //字段简称，从那个字段来的  [Actor]<ExecutionUnits>.Remarks
        public string _desc;         //值的描述，从哪个template的哪个字段读取出来的  (6)v_ActivityContent.Name
        public string _val;          //值
    }

    public class TemplateBase
    {
        public virtual object Clone()
        {
            var temp = MemberwiseClone();
            return temp;
        }

        public virtual object DeepClone()
        {
            return Serializer.DeepClone(this);
        }

        public byte[] Serialize()
        {
            byte[] pbs;
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, this);
                pbs = memory.ToArray();
            }
            return pbs;
        }

        //处理csv中要显示的字符串
        public static string ProcessStringForCsv(string strOrig)
        {
            string str = strOrig;
            str = str.Replace("\"", "\"\"");
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"))
            {
                str = string.Format("\"{0}\"", str);
            }
            return str;
        }
    }
#endif
}

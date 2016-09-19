using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO
{
    public class RecordSerializer
    {
        public const uint BOOL_MAX_SIZE = 1;
        public const uint WORD_MAX_SIZE = 4;
        public const uint DWORD_MAX_SIZE = 8;
        public const uint GUID_MAX_SIZE = 16;
        public const uint STRING_MAX_SIZE = 8192;

        /// <summary>
        /// Uses reflection to determine a maximum record size in the following way:
        ///   Boolean: 1 byte per  (will be configurable eventually)
        ///   Int32, UInt32: 4 bytes per
        ///   Int64, UInt64: 8 bytes per
        ///   String: 8 kilobytes per (will be configurable eventually)
        /// </summary>
        /// <remarks>
        /// Does not recurse into custom subtypes.  Only considers public properties
        /// with both getters and setters when serializing or deserializing.
        /// </remarks>
        public uint CalculateRecordMaxSize(Type t)
        {
            PropertyInfo[] props = null;
            return CalculateRecordMaxSize(t, out props);
        }

        private uint CalculateRecordMaxSize(Type t, out PropertyInfo[] props)
        {
            var properties = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            List<PropertyInfo> propInfos = new List<PropertyInfo>();
            uint maxSize = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].CanWrite || !properties[i].CanRead || properties[i].SetMethod.IsPrivate || 
                    properties[i].GetMethod.IsPrivate)
                    continue;
                propInfos.Add(properties[i]);
                Type ptype = properties[i].PropertyType;
                if (ptype == typeof(String))
                {
                    maxSize += STRING_MAX_SIZE;
                }
                else if (ptype == typeof(Int32) || ptype == typeof(UInt32))
                {
                    maxSize += WORD_MAX_SIZE;
                }
                else if (ptype == typeof(Int64) || ptype == typeof(UInt64))
                {
                    maxSize += DWORD_MAX_SIZE;
                }
                else if (ptype == typeof(Boolean) || ptype == typeof(Boolean))
                {
                    maxSize += BOOL_MAX_SIZE;
                }
                else if (ptype == typeof(Guid) || ptype == typeof(Guid))
                {
                    maxSize += GUID_MAX_SIZE;
                }
            }
            props = propInfos.ToArray();
            return maxSize;
        }
        public byte[] SerializeRecord(Type t, Object o)
        {
            PropertyInfo[] properties = null;
            uint size = CalculateRecordMaxSize(t, out properties);
            byte[] serialized = null;
            if (o == null || size == 0 || properties == null)
                return serialized;

            serialized = new byte[size];
            int offset = 0;
            for(int i = 0; i < properties.Length; i++)
            {
                byte[] data = ExtractAndSerializeValue(properties[i], o);
                data.CopyTo(serialized, offset);
                offset += data.Length;
            }
            return serialized;
        }

        public byte[] ExtractAndSerializeValue(PropertyInfo pinfo, Object o)
        {
            Type ptype = pinfo.PropertyType;
            var value = pinfo.GetValue(o);
            if (ptype == typeof(String))
            {
                byte[] totalBytes = new byte[STRING_MAX_SIZE];
                if (value == null || value.ToString() == String.Empty)
                    return totalBytes;
                ASCIIEncoding asc = new ASCIIEncoding();
                byte[] bytes = asc.GetBytes(value.ToString().Substring(0, (int)STRING_MAX_SIZE));
                bytes.CopyTo(totalBytes, 0);
                return totalBytes;
            }
            else if (ptype == typeof(Int32))
            {
                return BitConverter.GetBytes((Int32)value);
            }
            else if(ptype == typeof(UInt32))
            {
                return BitConverter.GetBytes((UInt32)value);
            }
            else if (ptype == typeof(Int64))
            {
                return BitConverter.GetBytes((Int64)value);
            }
            else if (ptype == typeof(UInt64))
            {
                return BitConverter.GetBytes((UInt64)value);
            }
            else if (ptype == typeof(Boolean) || ptype == typeof(Boolean))
            {
                return BitConverter.GetBytes((Boolean)value);
            }
            else if (ptype == typeof(Guid) || ptype == typeof(Guid))
            {
                byte[] ret = ((Guid)value).ToByteArray();
                return ret;
            }
            return null;
        }

        public Object DeserializeRecord(Type t, byte[] stream, uint position)
        {
            PropertyInfo[] properties = null;
            uint size = CalculateRecordMaxSize(t, out properties);
            byte[] serialized = null;
            Object o = Activator.CreateInstance(t);
            if (o == null || size == 0 || properties == null)
                return serialized;

            uint offset = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                offset = DeserializeAndSet(properties[i], o, stream, position);
                if (offset == 0)
                    break;//this is an error condition - figure out what to do here
                position += offset;
            }
            return o;
        }

        public uint DeserializeAndSet(PropertyInfo pinfo, Object o, byte[] stream, uint startPosition)
        {
            Type ptype = pinfo.PropertyType;
            if (ptype == typeof(String))
            {
                byte[] totalBytes = new byte[STRING_MAX_SIZE];

                ASCIIEncoding asc = new ASCIIEncoding();
                string s = asc.GetString(stream, (int)startPosition, (int)STRING_MAX_SIZE).Trim(new char[] { '\0'});
                pinfo.SetValue(o, s);
                return STRING_MAX_SIZE;
            }
            else if (ptype == typeof(Int32))
            {
                pinfo.SetValue(o, BitConverter.ToInt32(stream, (int)startPosition));
                return WORD_MAX_SIZE;
            }
            else if (ptype == typeof(UInt32))
            {
                pinfo.SetValue(o, BitConverter.ToUInt32(stream, (int)startPosition));
                return WORD_MAX_SIZE;
            }
            else if (ptype == typeof(Int64))
            {
                pinfo.SetValue(o, BitConverter.ToInt64(stream, (int)startPosition));
                return DWORD_MAX_SIZE;
            }
            else if (ptype == typeof(UInt64))
            {
                pinfo.SetValue(o, BitConverter.ToUInt64(stream, (int)startPosition));
                return DWORD_MAX_SIZE;
            }
            else if (ptype == typeof(Boolean) || ptype == typeof(Boolean))
            {
                pinfo.SetValue(o, BitConverter.ToBoolean(stream, (int)startPosition));
                return BOOL_MAX_SIZE;
            }
            else if (ptype == typeof(Guid) || ptype == typeof(Guid))
            {
                byte[] guidBytes = new byte[GUID_MAX_SIZE];
                for(uint i = startPosition; i < startPosition + GUID_MAX_SIZE; i++)
                {
                    guidBytes[i - startPosition] = stream[i];
                }
                Guid g = new Guid(guidBytes);
                pinfo.SetValue(o, g);
                return GUID_MAX_SIZE;
            }
            return 0;
        }

    }
}

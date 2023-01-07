#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using CompressionLevel = System.IO.Compression.CompressionLevel;

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Features.RuntimeDebugger.Editor")]
namespace UnityEditor.XR.OpenXR.Features.RuntimeDebugger
{
    internal class DebuggerState
    {
        public enum Command
        {
            kStartFunctionCall,
            kStartStruct,

            kFloat,
            kString,
            kInt32,
            kInt64,
            kUInt32,
            kUInt64,

            kEndStruct,
            kEndFunctionCall,

            kCacheNotLargeEnough,

            kLUTDefineTables,
            kLUTEntryUpdateStart,
            kLutEntryUpdateEnd,
            kLUTLookup,
        };

        private const byte FileVersion = 2;
        private static readonly byte[] Header = new byte[] { 0xea, 0x24, 0x39, 0x5c, 0xe0, 0xac, 0x79, FileVersion };

        internal static List<FunctionCall> _functionCalls = new List<FunctionCall>();
        private static List<byte> saveToFile = new List<byte>(Header);
        private static byte openedFileVersion = FileVersion;

        internal static Dictionary<UInt32, Dictionary<UInt64, HandleDebugEvent>> xrLut = new Dictionary<UInt32, Dictionary<UInt64, HandleDebugEvent>>();
        internal static List<string> lutNames = new List<string>();

        internal static void Clear()
        {
            _functionCalls.Clear();
            saveToFile.Clear();
            saveToFile.AddRange(Header);

            openedFileVersion = FileVersion;
        }

        private static Action _doneCallback;
        internal static UInt32 _lastPayloadSize;
        internal static UInt32 _frameCount;
        internal static UInt32 _lutSize;

        internal static void SetDoneCallback(Action done)
        {
            _doneCallback = done;
        }

        private static StringBuilder _sb = new StringBuilder();
        internal static string ReadString(BinaryReader r)
        {
            _sb.Clear();
            byte b;
            while ((b = r.ReadByte()) != (byte)0)
            {
                _sb.Append((Char)b);
            }
            return _sb.ToString();
        }

        internal static void SaveToFile(string path)
        {
            using var stream = File.Open(path, FileMode.Create);
            using var gzip = new GZipStream(stream, CompressionLevel.Optimal);
            gzip.Write(saveToFile.ToArray(), 0, saveToFile.Count);
        }

        internal static void LoadFromFile(string path)
        {
            xrLut.Clear();
            lutNames.Clear();
            lutNames.Add("All Calls");
            using var inStream = File.OpenRead(path);
            var gzip = new GZipStream(inStream, CompressionMode.Decompress);
            byte[] bytes;
            using (var outStream = new MemoryStream())
            {
                gzip.CopyTo(outStream);
                bytes = outStream.ToArray();
            }

            var headerCounter = 0;
            while (headerCounter < 7)
            {
                if (Header[headerCounter] != bytes[headerCounter])
                {
                    Debug.Log("Wrong file format.");
                    return;
                }
                ++headerCounter;
            }

            openedFileVersion = bytes[7];
            if (openedFileVersion > FileVersion)
            {
                Debug.Log($"File created with newer version ({openedFileVersion} > {FileVersion}.");
            }

            OnMessageEvent(new MessageEventArgs() {data = bytes.Skip(8).ToArray()});
        }

        internal static void OnMessageEvent(MessageEventArgs args)
        {
            if (args == null || args.data == null)
                return;
            _lastPayloadSize = (UInt32)args.data.Length;
            _frameCount = 0;
            saveToFile.AddRange(args.data);
            try
            {
                using (MemoryStream ms = new MemoryStream(args.data))
                {
                    using (BinaryReader r = new BinaryReader(ms, Encoding.UTF8))
                    {
                        while (r.BaseStream.Position != r.BaseStream.Length)
                        {
                            var command = (Command)r.ReadUInt32();
                            switch (command)
                            {
                                case Command.kStartFunctionCall:
                                    var thread = ReadString(r);
                                    var funcName = ReadString(r);
                                    var funcCall = new FunctionCall(thread, funcName);
                                    _functionCalls.Add(funcCall);
                                    funcCall.Parse(r);

                                    if (funcName == "xrBeginFrame")
                                    {
                                        ++_frameCount;
                                    }
                                    break;
                                case Command.kLUTDefineTables:
                                    lutNames.Clear();
                                    lutNames.Add("All Calls");
                                    var numLUTs = r.ReadUInt32();
                                    for (UInt32 lutIndex = 0; lutIndex < numLUTs; ++lutIndex)
                                    {
                                        xrLut[lutIndex] = new Dictionary<UInt64, HandleDebugEvent>();
                                        lutNames.Add(ReadString(r));
                                    }

                                    break;
                                case Command.kLUTEntryUpdateStart:
                                    var lutKey = r.ReadUInt32();
                                    var handle = r.ReadUInt64();
                                    var handleName = ReadString(r);

                                    // struct command, skip it
                                    r.ReadUInt32();
                                    ReadString(r);
                                    ReadString(r);

                                    var evt = new HandleDebugEvent(handleName, handle);
                                    evt.Parse(r);
                                    xrLut[lutKey][handle] = evt;
                                    break;
                                case Command.kLutEntryUpdateEnd:
                                    break;
                                case Command.kCacheNotLargeEnough:
                                    funcCall = new FunctionCall(r.ReadUInt32().ToString(), ReadString(r));
                                    _functionCalls.Add(funcCall);
                                    var result = ReadString(r);
                                    funcCall.displayName += " = " + result + " (cache not large enough)";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            _doneCallback?.Invoke();
        }

        internal class DebugEvent : TreeViewItem
        {
            private static int idCounter = 1;
            private List<DebugEvent> childrenEvents = new List<DebugEvent>();
            protected string fieldname;

            protected DebugEvent(string fieldname, string display)
            : base(idCounter++, 0, display)
            {
                this.fieldname = fieldname;
            }

            public virtual DebugEvent Clone()
            {
                return null;
            }

            public virtual string GetValue()
            {
                return "";
            }

            public override string ToString()
            {
                string var = displayName;

                foreach (var child in childrenEvents)
                {
                    var += "\n\t" + child.ToString().Replace("\n", "\n\t");
                }

                return var;
            }

            public void Parse(BinaryReader r)
            {
                DebugEvent parsedChild = null;
                bool endEvent = false;

                do
                {
                    if (parsedChild != null)
                    {
                        AddChildEvent(parsedChild);
                        parsedChild.Parse(r);
                        parsedChild = null;
                    }

                    var command = (Command) r.ReadUInt32();
                    switch (command)
                    {
                        case Command.kStartStruct:
                            parsedChild = new StructDebugEvent(ReadString(r), ReadString(r));
                            break;
                        case Command.kLUTLookup:
                            var lutKey = r.ReadUInt32();
                            var fieldName = ReadString(r);
                            var handle = r.ReadUInt64();

                            if (xrLut[lutKey].TryGetValue(handle, out var evt))
                            {
                                AddChildEvent(evt.Clone(fieldName));
                            }
                            else
                            {
                                AddChildEvent(new UInt64DebugEvent(fieldName, handle));
                            }
                            break;
                        case Command.kFloat:
                            AddChildEvent(new FloatDebugEvent(ReadString(r), r.ReadSingle()));
                            break;
                        case Command.kString:
                            AddChildEvent(new StringDebugEvent(ReadString(r), ReadString(r)));
                            break;
                        case Command.kInt32:
                            AddChildEvent(new Int32DebugEvent(ReadString(r), r.ReadInt32()));
                            break;
                        case Command.kInt64:
                            AddChildEvent(new Int64DebugEvent(ReadString(r), r.ReadInt64()));
                            break;
                        case Command.kUInt32:
                            AddChildEvent(new UInt32DebugEvent(ReadString(r), r.ReadUInt32()));
                            break;
                        case Command.kUInt64:
                            AddChildEvent(new UInt64DebugEvent(ReadString(r), r.ReadUInt64()));
                            break;
                        case Command.kEndStruct:
                            endEvent = true;
                            break;
                        case Command.kEndFunctionCall:
                            var result = ReadString(r);
                            displayName += " = " + result;
                            endEvent = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } while (!endEvent && r.BaseStream.Position != r.BaseStream.Length);
            }

//            public IEnumerable<DebugEvent> GetChildren()
//            {
//                return childrenEvents;
//            }

            public DebugEvent GetFirstChild()
            {
                return childrenEvents[0];
            }

            private DebugEvent AddChildEvent(DebugEvent evt)
            {
                childrenEvents.Add(evt);
                AddChild(evt);
                return this;
            }

            protected DebugEvent AddClonedChildren(DebugEvent clone)
            {
                foreach (var evt in childrenEvents)
                {
                    clone.AddChildEvent(evt.Clone());
                }
                return clone;
            }
        }

        internal class HandleDebugEvent : DebugEvent
        {
            private string niceDisplay;
            private UInt64 handle;

            public HandleDebugEvent(string niceDisplay, UInt64 handle)
                : base("", $"{niceDisplay} = {handle}")
            {
                this.niceDisplay = niceDisplay;
                this.handle = handle;
            }

            public override string GetValue()
            {
                return niceDisplay;
            }

            public override DebugEvent Clone()
            {
                var evt = new HandleDebugEvent(niceDisplay, handle);
                evt.displayName = displayName;
                return AddClonedChildren(evt);
            }

            public DebugEvent Clone(string fieldName)
            {
                var ret = Clone();
                ret.displayName = $"{fieldName} = {niceDisplay} ({handle})";
                return ret;
            }
        }

        internal class FunctionCall : DebugEvent
        {
            public string threadId { get; }
            public string returnVal { get; set; }

            public FunctionCall(string threadId, string displayName)
            : base("", displayName)
            {
                this.threadId = threadId;
            }

            public override DebugEvent Clone()
            {
                return AddClonedChildren(new FunctionCall(threadId, displayName));
            }
        }

        internal class StructDebugEvent : DebugEvent
        {
            public string structname { get; }
            public StructDebugEvent(string fieldname, string structname)
                : base(fieldname, $"{fieldname} = {structname}")
            {
                this.structname = structname;
            }

            public override string GetValue()
            {
                return structname;
            }

            public override DebugEvent Clone()
            {
                return AddClonedChildren(new StructDebugEvent(fieldname, structname));
            }
        }

        internal class FloatDebugEvent : DebugEvent
        {
            public float value { get; }
            public FloatDebugEvent(string displayName, float val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }

            public override DebugEvent Clone()
            {
                return new FloatDebugEvent(fieldname, value);
            }
        }

        internal class StringDebugEvent : DebugEvent
        {
            public string value { get; }
            public StringDebugEvent(string displayName, string val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return value;
            }

            public override DebugEvent Clone()
            {
                return new StringDebugEvent(fieldname, value);
            }
        }

        internal class Int32DebugEvent : DebugEvent
        {
            public Int32 value { get; }
            public Int32DebugEvent(string displayName, Int32 val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }

            public override DebugEvent Clone()
            {
                return new Int32DebugEvent(fieldname, value);
            }
        }

        internal class Int64DebugEvent : DebugEvent
        {
            public Int64 value { get; }
            public Int64DebugEvent(string displayName, Int64 val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }

            public override DebugEvent Clone()
            {
                return new Int64DebugEvent(fieldname, value);
            }
        }

        internal class UInt32DebugEvent : DebugEvent
        {
            public UInt32 value { get; }
            public UInt32DebugEvent(string displayName, UInt32 val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }

            public override DebugEvent Clone()
            {
                return new UInt32DebugEvent(fieldname, value);
            }
        }

        internal class UInt64DebugEvent : DebugEvent
        {
            public UInt64 value { get; }
            public UInt64DebugEvent(string displayName, UInt64 val)
                : base(displayName, displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }

            public override DebugEvent Clone()
            {
                return new UInt64DebugEvent(fieldname, value);
            }
        }
    }
}
#endif

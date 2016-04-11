// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: TestMessages.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace GoodAI.Net.ConverseSharp {

  /// <summary>Holder for reflection information generated from TestMessages.proto</summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public static partial class TestMessagesReflection {

    #region Descriptor
    /// <summary>File descriptor for TestMessages.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TestMessagesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChJUZXN0TWVzc2FnZXMucHJvdG8SGEdvb2RBSS5OZXQuQ29udmVyc2VTaGFy",
            "cCInCgdDb21tYW5kEgwKBENvZGUYASABKAUSDgoGTWV0aG9kGAIgASgJYgZw",
            "cm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedCodeInfo(null, new pbr::GeneratedCodeInfo[] {
            new pbr::GeneratedCodeInfo(typeof(global::GoodAI.Net.ConverseSharp.Command), global::GoodAI.Net.ConverseSharp.Command.Parser, new[]{ "Code", "Method" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Command : pb::IMessage<Command> {
    private static readonly pb::MessageParser<Command> _parser = new pb::MessageParser<Command>(() => new Command());
    public static pb::MessageParser<Command> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::GoodAI.Net.ConverseSharp.TestMessagesReflection.Descriptor.MessageTypes[0]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public Command() {
      OnConstruction();
    }

    partial void OnConstruction();

    public Command(Command other) : this() {
      code_ = other.code_;
      method_ = other.method_;
    }

    public Command Clone() {
      return new Command(this);
    }

    /// <summary>Field number for the "Code" field.</summary>
    public const int CodeFieldNumber = 1;
    private int code_;
    public int Code {
      get { return code_; }
      set {
        code_ = value;
      }
    }

    /// <summary>Field number for the "Method" field.</summary>
    public const int MethodFieldNumber = 2;
    private string method_ = "";
    public string Method {
      get { return method_; }
      set {
        method_ = pb::Preconditions.CheckNotNull(value, "value");
      }
    }

    public override bool Equals(object other) {
      return Equals(other as Command);
    }

    public bool Equals(Command other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Code != other.Code) return false;
      if (Method != other.Method) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (Code != 0) hash ^= Code.GetHashCode();
      if (Method.Length != 0) hash ^= Method.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Code != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Code);
      }
      if (Method.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Method);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (Code != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Code);
      }
      if (Method.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Method);
      }
      return size;
    }

    public void MergeFrom(Command other) {
      if (other == null) {
        return;
      }
      if (other.Code != 0) {
        Code = other.Code;
      }
      if (other.Method.Length != 0) {
        Method = other.Method;
      }
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Code = input.ReadInt32();
            break;
          }
          case 18: {
            Method = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code

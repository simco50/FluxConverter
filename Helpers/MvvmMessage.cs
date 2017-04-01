namespace FluxConverterTool.Helpers
{
    public enum MessageType
    {
        MeshUpdate,
        MeshSetTexture,
    }

    public class MvvmMessage
    {
        public MvvmMessage(MessageType type, object data = null)
        {
            Data = data;
            Type = type;
        }

        public object Data;
        public MessageType Type;
    }
}

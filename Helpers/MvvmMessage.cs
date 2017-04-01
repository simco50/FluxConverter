namespace FluxConverterTool.Helpers
{
    public enum MessageType
    {
        MeshUpdate,
        MeshLoadDiffuseTexture,
        MeshLoadNormalTexture,
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

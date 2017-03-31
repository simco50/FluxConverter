using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxConverterTool.Helpers
{
    public enum MessageType
    {
        MeshUpdate,
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

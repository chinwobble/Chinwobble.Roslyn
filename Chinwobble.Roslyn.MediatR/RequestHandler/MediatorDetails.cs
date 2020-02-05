using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinwobble.Roslyn.MediatR.RequestHandler
{
    public struct MediatorDetails
    {
        public MediatorDetails(bool isGeneric, string returnTypeName)
        {
            IsGeneric = isGeneric;
            ReturnTypeName = returnTypeName;
        }

        public bool IsGeneric { get; }
        public string ReturnTypeName { get; }
    }
}

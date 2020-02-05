using Chinwobble.Roslyn.MediatR;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stream = System.IO.MemoryStream;

namespace Chinwobble.Roslyn.Sandbox.Mediator
{
    [Chinwobble.Roslyn.MediatR.GenerateRequestHandler]
    public class TestRequest : IRequest<string>
    {
        public Task Handle(string test, Stream stream)
        {
            return Task.CompletedTask;
        }
    }
}

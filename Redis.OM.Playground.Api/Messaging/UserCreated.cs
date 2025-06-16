using Mediator;
using Unit = TinyFp.Unit;

namespace Redis.OM.Playground.Api.Messaging;

public struct UserCreated : IRequest<Unit>
{
}

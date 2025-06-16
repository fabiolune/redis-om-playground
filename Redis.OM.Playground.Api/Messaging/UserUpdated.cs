using Mediator;
using Unit = TinyFp.Unit;

namespace Redis.OM.Playground.Api.Messaging;

public struct UserUpdated : IRequest<Unit>
{
}

namespace Portfolio.Api.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    // Fila em memória para processamento assíncrono
    private readonly Channel<IDomainEvent> _channel;

    public DomainEventDispatcher()
    {
        // Cria um canal ilimitado. O Worker (BackgroundService) vai ler deste canal.
        _channel = Channel.CreateUnbounded<IDomainEvent>();
    }

    public async Task DispatchAsync<TEvent>(TEvent evt, CancellationToken ct = default) where TEvent : IDomainEvent
    {
        // Apenas enfileira o evento e retorna imediatamente, sem bloquear a requisição HTTP.
        await _channel.Writer.WriteAsync(evt, ct);
    }
}
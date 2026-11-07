// Jonathan de Souza Pereira


using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoColaboradorRepository : IRepository<AcessoColaborador>
{
    Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default);
    Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademia(int colaboradorId, CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default);
}
// Validar se já não ultrapassa o limite de: 8 horas se for ctl, 6 horas se for estágio.
// Na saída, mostrar o tempo que permaneceu na academia, devendo ser somado todos os registros do dia.
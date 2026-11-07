// Jonathan de Souza Pereira
namespace AcademiaDoZe.Application.Services;

internal sealed record AcessoEvento(int Id, DateTime DataHora);

internal sealed record AcessoVisita(int Id, int? SaidaId, DateTime Entrada, DateTime? Saida);

// A tabela tb_acesso guarda UM registro (data/hora) por passagem na catraca.
// Dentro de cada dia, o 1º registro é entrada, o 2º é saída, o 3º é entrada, e assim por diante.
internal static class AcessoPareamento
{
    public static List<AcessoVisita> Parear(IEnumerable<AcessoEvento> eventos)
    {
        var visitas = new List<AcessoVisita>();

        var porDia = eventos
            .OrderBy(e => e.DataHora)
            .ThenBy(e => e.Id)
            .GroupBy(e => e.DataHora.Date);

        foreach (var dia in porDia)
        {
            var lista = dia.ToList();
            for (int i = 0; i < lista.Count; i += 2)
            {
                bool temSaida = i + 1 < lista.Count;
                DateTime? saida = temSaida ? lista[i + 1].DataHora : null;
                int? saidaId = temSaida ? lista[i + 1].Id : null;
                visitas.Add(new AcessoVisita(lista[i].Id, saidaId, lista[i].DataHora, saida));
            }
        }

        return visitas;
    }

    // Formata como hh:mm:ss (ex: 00:02:15)
    public static string Formatar(TimeSpan tempo)
        => $"{(int)tempo.TotalHours:00}:{tempo.Minutes:00}:{tempo.Seconds:00}";

    // Data/hora atual sem frações de segundo
    public static DateTime Agora()
    {
        var agora = DateTime.Now;
        return new DateTime(agora.Ticks - (agora.Ticks % TimeSpan.TicksPerSecond), agora.Kind);
    }

    // Traduz as notificações do domínio para uma mensagem amigável
    public static string TraduzirNotificacoes(IEnumerable<AcademiaDoZe.Domain.Common.Notification> notificacoes)
    {
        var mensagens = notificacoes.Select(n => n.Mensagem).ToList();
        if (mensagens.Any(m => m.Contains("DATA_HORA_INTERVALO_INVALIDO")))
            return "a academia só registra acessos entre 06:00 e 22:00.";
        return string.Join(", ", mensagens);
    }
}
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class MatriculaDto
{
    public int Id { get; set; }
    public AlunoDto? AlunoMatricula { get; set; }
    public AppMatriculaPlano Plano { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public string Objetivo { get; set; } = string.Empty;
    public AppMatriculaRestricoes RestricoesMedicas { get; set; }
    public string? ObservacoesRestricoes { get; set; }
    public ArquivoDto? LaudoMedico { get; set; }
}
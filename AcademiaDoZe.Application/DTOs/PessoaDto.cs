namespace AcademiaDoZe.Application.DTOs;

public abstract class PessoaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public LogradouroDto? Endereco { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string? Senha { get; set; }
    public ArquivoDto? Foto { get; set; }
}
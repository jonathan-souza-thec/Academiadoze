// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Aluno : Pessoa
{
    // construtor privado — criação somente via Factory Method
    private Aluno(int id, string nome, Cpf cpf, DateOnly dataNascimento,
                  Telefone telefone, Email email, Endereco endereco,
                  Senha senha, Arquivo foto)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }

    public static Result<Aluno> Criar(int id, string nome, string cpf,
        DateOnly dataNascimento, string telefone, string email,
        Logradouro logradouro, string numero, string complemento,
        string senha, byte[] foto)
    {
        var notifications = new List<Notification>();

        // — nome —
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        // — data de nascimento —
        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

        // — value objects —
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);

        var enderecoResult = Endereco.Criar(logradouro, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);

        var fotoResult = Arquivo.Criar(foto);
        if (fotoResult.IsFailure) notifications.AddRange(fotoResult.Notifications);

        if (notifications.Count != 0)
            return Result<Aluno>.Failure(notifications);

        return Result<Aluno>.Success(new Aluno(
            id, nome,
            cpfResult.Value!,
            dataNascimento,
            telefoneResult.Value!,
            emailResult.Value!,
            enderecoResult.Value!,
            senhaResult.Value!,
            fotoResult.Value!));
    }
}

// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

// Classe base — garante identidade única e valida o Id
public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        // Fail-Fast: Id negativo é irrecuperável
        if (id < 0) throw new DomainException("ID_NEGATIVO");
        Id = id;
    }
}

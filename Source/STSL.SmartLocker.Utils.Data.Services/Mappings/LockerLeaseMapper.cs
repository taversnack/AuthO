using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerLeaseMapper : IMapsToDTO<LockerLeaseDTO, LockerLease>
{
    public static LockerLeaseDTO ToDTO(LockerLease entity) => new
    (
        Id: entity.Id,
        StartedAt: entity.StartedAt,
        EndedAt: entity.EndedAt,
        LockerBankBehaviour: entity.LockerBankBehaviour,
        EndedByMasterCard: entity.EndedByMasterCard,
        CardCredential: entity.CardCredential is null ? null : CardCredentialMapper.ToDTO(entity.CardCredential),
        CardHolder: entity.CardHolder is null ? null : CardHolderMapper.ToDTO(entity.CardHolder),
        Locker: entity.Locker is null ? null : LockerMapper.ToDTO(entity.Locker),
        Lock: entity.Lock is null ? null : LockMapper.ToDTO(entity.Lock)
    );
}

internal sealed class CreatePermanentLockerLeaseMapper : IMapsToEntity<CreatePermanentLockerLeaseDTO, LockerLease>
{
    public static LockerLease ToEntity(CreatePermanentLockerLeaseDTO dto) => new()
    {
        StartedAt = dto.StartedAt,
        EndedAt = null,
        LockerBankBehaviour = LockerBankBehaviour.Permanent,
        EndedByMasterCard = false,
        CardCredentialId = dto.CardCredentialId,
        CardHolderId = dto.CardHolderId,
        LockerId = dto.LockerId,
        LockId = dto.LockId,
    };
}
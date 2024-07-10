namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

public interface IMapsToDTO<DTO, TEntity>
{
    static abstract DTO ToDTO(TEntity entity);
}

public interface IMapsToEntity<DTO, TEntity>
{
    static abstract TEntity ToEntity(DTO dto);
}

public interface IMapsToUpdatedEntity<DTO, TEntity>
{
    static abstract TEntity ToEntity(DTO dto,  TEntity entity);
}

public interface IEntityDTOMapper<DTO, TEntity> : IMapsToDTO<DTO, TEntity>, IMapsToEntity<DTO, TEntity> { }
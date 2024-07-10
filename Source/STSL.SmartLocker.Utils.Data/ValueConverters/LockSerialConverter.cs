using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Data.ValueConverters;

internal sealed class LockSerialConverter : ValueConverter<LockSerial, int>
{
    public LockSerialConverter() : base(x => x.Value, x => new(x)) { }
    public LockSerialConverter(ConverterMappingHints mappingHints) : base(x => x.Value, x => new(x), mappingHints) { }
}

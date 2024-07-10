using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace STSL.SmartLocker.Utils.Api.Helpers;

internal static class ApiConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    //[ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static void Delete(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid tenantId,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] params object[] parameters

    )
    { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    //[ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static void Get(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid tenantId,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] params object[] parameters
    )
    { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    //[ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static void Post(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid tenantId,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] params object[] parameters
    )
    { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    //[ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static void Put(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid tenantId,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] params object[] parameters
    )
    { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    //[ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static void Patch(
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid tenantId,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)] Guid id,
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)][ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)] params object[] parameters
    )
    { }
}

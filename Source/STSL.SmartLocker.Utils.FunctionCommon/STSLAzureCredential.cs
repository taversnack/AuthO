using Azure.Core;
using Azure.Identity;
using System.Diagnostics;

namespace STSL.SmartLocker.Utils.FunctionCommon
{
    public static class STSLAzureCredential
    {
        public static TokenCredential GetCredential()
        {
            TokenCredential tokenCredential;

            if (Debugger.IsAttached)
            {
                // return a VisualStudioCredential is we are debugging
                // allow the Azure tenant to be specified via an environment variable (e.g. useful if developer is logged in with a guest account)
                // (this does not need to be set if we are using an account that is owned by the tenant)

                string? tenant = Environment.GetEnvironmentVariable("STSL_AZURE_TENANT");

                if (tenant is null)
                {
                    tokenCredential = new VisualStudioCredential();
                }
                else
                {
                    tokenCredential = new VisualStudioCredential(
                        new VisualStudioCredentialOptions()
                        {
                            TenantId = tenant
                        });
                }
            }
            else
            {
                tokenCredential = new DefaultAzureCredential();
            }

            return tokenCredential;
        }
    }
}

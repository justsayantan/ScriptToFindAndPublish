using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.CoreService.Client;

namespace ScriptForFindAndPublish
{
    public static class Helper
    {
        public static CoreServiceClient client = Utility.CoreServiceSource;
        public static string IsTemplateValid(string itemUri)
        {
            try
            {
                ComponentTemplateData ctData = (ComponentTemplateData)client.Read(itemUri, new ReadOptions());
                return "Valid";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string IsPublicationValid(string itemUri)
        {
            try
            {
                PublicationData pbData = (PublicationData)client.Read(itemUri, new ReadOptions());
                return "Valid";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static bool HasChildren(string publicationTcmUri)
        {
            PublicationData pbData = (PublicationData)client.Read(publicationTcmUri, new ReadOptions());
            return pbData.HasChildren != null && pbData.HasChildren.Value;
        }

        public static bool IsValidTargetId(string publishingTarget)
        {
            TargetTypeData targetTypeDataData = (TargetTypeData)client.Read(publishingTarget, new ReadOptions());
            return targetTypeDataData != null;
        }
    }
}

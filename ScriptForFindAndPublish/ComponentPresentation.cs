using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.CoreService.Client;

namespace ScriptForFindAndPublish
{
    public class ComponentPresentation
    {
        public LinkToIdentifiableObjectData Component { get; set; }

        public LinkToTemplateData Template { get; set; }
    }
}

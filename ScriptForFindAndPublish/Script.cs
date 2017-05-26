using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace ScriptForFindAndPublish
{
    class Script
    {
        public static CoreServiceClient client = Utility.CoreServiceSource;
        public static string TemplateTcmUri { get; set; } //= "tcm:22-6579-32";
        public static bool WantToProcced = false;
        public static bool WantToPublish = false;
        public static bool IsValidTemplateTcmUri = false;
        public static bool IsValidPublicationTcmUri = false;
        public static bool IsValidPublishingTarget = false;
        public static bool includingChildren = false;
        public static string fileLocation { get; set; }
        public static string PublicationTcmUri { get; set; }
        public static string PublishingTarget { get; set; }

        public static List<string> publishComponentList = new List<string>();

        public static bool IsPublishingFromChild = false;

        static void Main(string[] args)
        {
            GatherUserInputAndValidate();

            Console.WriteLine("========== Please wait. We are processing your request==================");
            if (TemplateTcmUri != null)
            {
                TemplateTcmUri = ReplacePublicationIdFromTcmId(TemplateTcmUri, PublicationTcmUri);
                GetAllComponentByComponentTemplateAndPublish(TemplateTcmUri, PublishingTarget);

            }
            Console.WriteLine("==================== End ===========================");
            Console.ReadLine();
        }

        private static void GatherUserInputAndValidate()
        {
            while (!WantToProcced)
            {
                while (!IsValidTemplateTcmUri)
                {
                    Console.WriteLine("Please enter the Component Template Uri : ");
                    TemplateTcmUri = Console.ReadLine();
                    string message = Helper.IsTemplateValid(TemplateTcmUri);
                    if (message == "Valid")
                    {
                        IsValidTemplateTcmUri = true;
                    }
                    else
                    {
                        Console.WriteLine(String.Format("{0}", message));
                    }
                }

                while (!IsValidPublicationTcmUri)
                {
                    Console.WriteLine(
                        "Please enter Publication Id from Where you want to publish (Example:tcm:0-22-1) : ");
                    PublicationTcmUri = Console.ReadLine();
                    string message = Helper.IsPublicationValid(PublicationTcmUri);
                    if (message == "Valid")
                    {
                        IsValidPublicationTcmUri = true;
                        while (!includingChildren)
                        {
                            bool result = Helper.HasChildren(PublicationTcmUri);
                            if (result == true)
                            {
                                Console.WriteLine("Want to publish from child (Yes/No) : ");
                                string input = Console.ReadLine();
                                if (input == "Yes")
                                {
                                    IsPublishingFromChild = true;
                                    includingChildren = true;
                                }
                                else if (input == "No")
                                {
                                    IsPublishingFromChild = false;
                                    includingChildren = true;
                                }
                                else
                                {
                                    Console.WriteLine("Not a Valid Input. Please choose Yes Or No");
                                    IsPublishingFromChild = false;
                                    includingChildren = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Publication Don't have any children");
                                includingChildren = true;
                            }
                        }

                        PublicationTcmUri = GetPublicationIdFromTcmId(PublicationTcmUri, 2);

                    }
                    else
                    {
                        Console.WriteLine($"{message}");
                    }
                }

                while (!IsValidPublishingTarget)
                {
                    Console.WriteLine("Please Choose the Publishing Target (Target TCM ID/Purpose) :");
                    PublishingTarget = Console.ReadLine();
                    if (PublishingTarget == "Staging")
                    {
                        IsValidPublishingTarget = true;
                    }
                    else if (PublishingTarget == "Live")
                    {
                        IsValidPublishingTarget = true;
                    }
                    else if (PublishingTarget.Contains("tcm"))
                    {
                        if (Helper.IsValidTargetId(PublishingTarget))
                        {
                            IsValidPublishingTarget = true;
                        }
                    }
                    else
                    {

                        Console.WriteLine("Not a Valid Input. Please choose Staging Or Live");
                        IsValidPublishingTarget = false;
                    }
                }
                Console.WriteLine("");
                Console.WriteLine("================= your input's are==================");
                Console.WriteLine($"Component Template TCM Uri : {TemplateTcmUri}");
                Console.WriteLine($"Publication Tcm Uri : {PublicationTcmUri}");
                Console.WriteLine($"Publish Include Children Publication : {IsPublishingFromChild.ToString()}");
                Console.WriteLine($"Please Target : {PublishingTarget}");
                Console.WriteLine("================= your input's are==================");
                Console.WriteLine("");
                Console.WriteLine("Do you want to proceed (Yes/No)");
                string confirmation = Console.ReadLine();
                if (confirmation == "Yes")
                {
                    WantToProcced = true;
                }
                else if (confirmation == "No")
                {
                    WantToProcced = false;
                    Console.WriteLine("Close and restart the application");
                }
                else
                {
                    Console.WriteLine("Not a Valid Input. Please choose Yes Or No");
                    WantToProcced = false;
                }
            }
        }

        private static void GetAllComponentByComponentTemplateAndPublish(string templateTcmUri, string PublishingTarget)

        {
            try
            {
                string[] items = { templateTcmUri };

                string[] purpose = { PublishingTarget };

                var resolveInstractionData = new ResolveInstructionData()
                {
                    IncludeChildPublications = IsPublishingFromChild,
                    Purpose = ResolvePurpose.RePublish,
                    IncludeDynamicVersion = true,
                    IncludeWorkflow = false,
                    IncludeComponentLinks = false
                };
                ResolvedItemData[] componentList = new ResolvedItemData[] { };
                PublishContextData[] publishingContext =
                    client.ResolveItems(items, resolveInstractionData, purpose, new ReadOptions());

                List<ComponentPresentation> componentPresentations = (
                    from resolvedContext in publishingContext
                    from resolvedItem in resolvedContext.ResolvedItems
                    select new ComponentPresentation
                    {
                        Component = resolvedItem.Item,
                        Template = resolvedItem.Template

                    }).ToList();

                string targetId = publishingContext.FirstOrDefault().PublicationTarget.IdRef;

                if (componentPresentations.Count > 0)
                {
                    fileLocation = ConfigurationManager.AppSettings["FileLocationForLog"]
                        .Replace(".txt", string.Format("_{0}.txt", items[0].Replace("tcm:", "")));
                    using (StreamWriter writer = new StreamWriter(fileLocation, true))
                    {

                        foreach (var cp in componentPresentations)
                        {
                            
                            publishComponentList.Add(cp.Component.IdRef);
                            Console.WriteLine("{" + cp.Component.Title + "}" + " ({" + cp.Component.IdRef + "})" +
                                              "rendered with {" + cp.Template.Title + "})");
                            writer.WriteLine("{" + cp.Component.Title + "}" + " ({" + cp.Component.IdRef + "})" +
                                             "rendered with {" + cp.Template.Title + "})");
                        }

                        Console.WriteLine(
                            $"Total {publishComponentList.Distinct().Count()} published Component Presentations based on Componenent Template: {items[0]}");
                        writer.WriteLine(
                            $"Found {publishComponentList.Distinct().Count()} published Component Presentations based on Componenent Template: {items[0]}");
                    }

                    PublishInstructionData instruction = new PublishInstructionData
                    {
                        ResolveInstruction = new ResolveInstructionData()
                        {
                            IncludeChildPublications = IsPublishingFromChild,
                            Purpose = ResolvePurpose.RePublish,
                            IncludeWorkflow = false,
                            IncludeComponentLinks = false
                        },
                        RenderInstruction = new RenderInstructionData()

                    };

                    while (!WantToPublish)
                    {
                        Console.WriteLine("Want to Republish ? (Yes/No)");
                        string confirmation = Console.ReadLine();
                        if (confirmation == "Yes")
                        {
                            WantToPublish = true;
                            Console.WriteLine("======================= Adding Item into Publishing Queue ======================");
                            foreach (string componentId in publishComponentList.Distinct())
                            {
                                client.Publish(new[] { componentId }, instruction, new[] { targetId }, PublishPriority.Low, new ReadOptions { LoadFlags = LoadFlags.None });
                            }
                            Console.WriteLine("=============== Task Finished  ==============");
                        }
                        else if (confirmation == "No")
                        {
                            WantToProcced = false;
                            Console.WriteLine("Close the application");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        private static string GetPublicationIdFromTcmId(string itemUri, int groupId = 1)
        {
            string pattern = @"^tcm:(\d+)-(\d+)(?:-\d+)?$";
            Match match = Regex.Match(itemUri, pattern);
            if (match.Success)
            {
                return match.Groups[groupId].Value;
            }
            else
            {
                return null;
            }
        }

        private static string ReplacePublicationIdFromTcmId(string inputTcmUri, string targetTcmUri)
        {
            string pattern = @"^tcm:(\d+)-\d+(?:-\d+)?$";
            var regex = new Regex(pattern);
            var match = regex.Match(inputTcmUri);

            var firstPart = inputTcmUri.Substring(0, match.Groups[1].Index);
            var secondPart = inputTcmUri.Substring(match.Groups[1].Index + match.Groups[1].Length);
            var result = firstPart + targetTcmUri + secondPart;
            return result;
        }
    }
}

# ScriptToFindAndPublish
Script To Find Component By Templates And Re-Publish
In my previous blog I have described how easily we can add or update field of the page metadata in one go and publish all the pages using Coreservice API. Today I will describe another script which is not very common but sometime useful. 

Scenario: 
In a migration project (Or at maintenance time) if you need to change dynamic component template, it is really hard to republish that template especially when huge number components are associated with that template. 
Recently I have faced this scenario in a project at the time of upgradation. There was couple of dynamic component templates which are having more than 6 lakhs of components associated. The requirement was to change the output format of the template and republish them, but most of the templates failed to publish after taking long time. 

Solution: 
To overcome this scenario I have written a scripts. Once we are ready with our changes in the templates then script helps to republish all the published items. The script will take inputs from user and find the resolved items with that dynamic component template and republish only those to that target again so that we should not publish any unwanted(Which was not previously published) item to the target. 
I have discussed with our colleagues to take their expert suggestion and also fine-tuned this script couple of time.

Functionality: 
As input that script will accept below item –
•	Template Id
•	Publication Id (from where you want to find and publish)
•	Target Id or Purpose
•	Want to Publish?

Functionality of Script once all the inputs are validated: 
1)	Find all the resolved items based on dynamic component template.
2)	Find the Distinct items from that list (Remove duplicate items).
3)	Re-Publish those items to the same target from particular publication and also from child.

Additionally Logging and validation of the user input is enable on that script.

Improvement Areas:
•	As of now the script run for one template, In future we can enable this script for multiple dynamic component templates in a same go

Now let me describe one by one – 

Find all the resolved items based on dynamic component template – 

       var resolveInstractionData = new ResolveInstructionData()
                {
                    IncludeChildPublications = IsPublishingFromChild,
                    Purpose = ResolvePurpose.RePublish,
                    IncludeWorkflow = false,
                    IncludeComponentLinks = false
                };
                ResolvedItemData[] componentList = new ResolvedItemData[] { };
                PublishContextData[] publishingContext =
                    client.ResolveItems(items, resolveInstractionData, purpose, new ReadOptions());

Get the Components from That publishingContext - 

List<ComponentPresentation> componentPresentations = (
                    from resolvedContext in publishingContext
                    from resolvedItem in resolvedContext.ResolvedItems
                    select new ComponentPresentation
                    {
                        Component = resolvedItem.Item,
                        Template = resolvedItem.Template

                    }).ToList();



Find the Distinct items from that list (Remove duplicate items):

foreach (var cp in componentPresentations)
{                            
    publishComponentList.Add(cp.Component.IdRef);                            
}
publishComponentList.Distinct().Count();


Re-Publish those items to the same target from particular publication and also from child.

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
foreach (string componentId in publishComponentList.Distinct())
{
   client.Publish(new[] { componentId }, instruction, new[] { targetId }, PublishPriority.Low, new ReadOptions { LoadFlags = LoadFlags.None });
}


I have already prepared the draft version of the script. All you need to do – 
1.	Update the app.config file with proper version core service client. 
2.	Modify the code as per your requirement. 
Here is the source code of the script. I want to thanks especially Jan and Monica for their expert suggestion.

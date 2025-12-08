using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    /// <summary>
    /// Represents IGES type 504 Group entity.
    /// </summary>
    public class IgesGroup : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Group;
        public List<IgesEntity> AssociatedEntities { get; } = new List<IgesEntity>();

        public IgesGroup() : base() { }

        public IgesGroup(IEnumerable<IgesEntity> associatedEntities)
            : base()
        {
            AssociatedEntities.AddRange(associatedEntities);
        }

        internal static IgesGroup FromParameters(List<string> parameters, IgesDirectoryData directoryData, IgesReaderBinder binder)
        {
            // DEBUG: Print all parameters received for this group
            //System.Console.WriteLine($"IgesGroup.FromParameters: parameters.Count={parameters.Count}");
            //for (int i = 0; i < parameters.Count; i++)
            //    System.Console.WriteLine($"  param[{i}]: '{parameters[i]}'");

            int count = int.Parse(parameters[0]);
            var group = new IgesGroup();

            // FIX: Process all parameters, trim whitespace, and log every binding attempt.
            int processed = 0;
            for (int i = 1; i < parameters.Count; i++)
            {
                var param = parameters[i].Trim();
                if (int.TryParse(param, out int deIndex))
                {
                    //System.Console.WriteLine($"  Trying to bind DE index: {deIndex}");
                    binder.BindEntity(deIndex, entity =>
                    {
                        if (entity != null)
                        {
                            group.AssociatedEntities.Add(entity);
                            //System.Console.WriteLine($"    Bound entity: {entity.EntityType} label: {entity.EntityLabel}");
                        }
                        else
                        {
                            //System.Console.WriteLine($"    DE index {deIndex} did not resolve to an entity.");
                        }
                    });
                    processed++;
                }
                else
                {
                    //System.Console.WriteLine($"  Skipped non-integer parameter: '{param}'");
                }
            }
            //System.Console.WriteLine($"IgesGroup.FromParameters: processed {processed} parameters, found {group.AssociatedEntities.Count} entities (expected {count})");
            return group;
        }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            // This method is not used for group parsing; handled by FromParameters
            return parameters.Count;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(AssociatedEntities.Count);
            foreach (var entity in AssociatedEntities)
            {
                parameters.Add(binder.GetEntityId(entity));
            }
        }
    }
}